using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Syroot.CafiineServer.Storage;

namespace Syroot.CafiineServer
{
    /// <summary>
    /// Represents a Cafiine server accepting connections to Caffine <see cref="Client"/> instances and manages the file
    /// system.
    /// </summary>
    internal class Server
    {
        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="Server"/> class running on the given <see cref="IPAddress"/>
        /// and listening on the specified port. The data and log file directory is specified in the additional
        /// parameters.
        /// </summary>
        /// <param name="ipAddress">The IP address to run the server on.</param>
        /// <param name="port">The port on which to listen for incoming connections.</param>
        /// <param name="dataDirectory">The directory containing the game data.</param>
        /// <param name="dumpDirectory">The directory in which dumped files will be stored.</param>
        /// <param name="logsDirectory">The log directory into which log files will be written.</param>
        /// <param name="dumpAll">Dump every requested file instead of replacing them.</param>
        /// <param name="enableFileLogs"><c>true</c> to enable file logging.</param>
        internal Server(IPAddress ipAddress, int port, string dataDirectory, string dumpDirectory, string logsDirectory,
            bool dumpAll, bool enableFileLogs)
        {
            IPAddress = ipAddress;
            Port = port;
            DataDirectory = dataDirectory;
            DumpDirectory = dumpDirectory;
            LogsDirectory = logsDirectory;
            DumpAll = dumpAll;

            // Initialize the storage system and log manager.
            Storage = new StorageSystem(DataDirectory);

            // Ensure the dump directory exists.
            Directory.CreateDirectory(DumpDirectory);

            // Initialize the log manager.
            Log = new LogManager(LogsDirectory, enableFileLogs);
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets the IP address on which the server runs.
        /// </summary>
        internal IPAddress IPAddress
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the port on which the server listens for incoming connections.
        /// </summary>
        internal int Port
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the directory in which game data is available.
        /// </summary>
        internal string DataDirectory
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the directory in which dumped game data will be stored.
        /// </summary>
        internal string DumpDirectory
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the directory in which log files will be written.
        /// </summary>
        internal string LogsDirectory
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the server dumps all queried files rather than replacing files.
        /// </summary>
        internal bool DumpAll
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the <see cref="StorageSystem"/> over which all file reads have to go.
        /// </summary>
        internal StorageSystem Storage
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the <see cref="LogManager"/> writing down any logged messages.
        /// </summary>
        internal LogManager Log
        {
            get;
            private set;
        }

        // ---- METHODS (INTERNAL) -------------------------------------------------------------------------------------

        /// <summary>
        /// Starts listening for and dispatching Cafiine client connections into listening threads. This is a blocking
        /// call.
        /// </summary>
        internal void Run()
        {
            // Create the listener which waits for incoming connections.
            TcpListener listener = new TcpListener(IPAddress, Port);
            listener.Start();
            Log.Write(ConsoleColor.Yellow, "SERVER", "Cafiine server started{0}.", DumpAll ? " in dump mode" : null);
            Log.Write(ConsoleColor.Yellow, "SERVER", "Server IP     : {0} (on port {1})",
                String.Join(", ", GetLocalIPs()), Port);
            Log.Write(ConsoleColor.Yellow, "SERVER", "Data directory: {0}", Path.GetFullPath(DataDirectory));
            Log.Write(ConsoleColor.Yellow, "SERVER", "Dump directory: {0}", Path.GetFullPath(DumpDirectory));
            Log.Write(ConsoleColor.Yellow, "SERVER", "Logs directory: {0}", Log.EnableFileLogs
                ? Path.GetFullPath(LogsDirectory) : "disabled");
            Log.Write(ConsoleColor.DarkYellow, "SERVER", "Listening for new connections...");

            // Repeatedly wait for new incoming connections.
            while (true)
            {
                Client client = new Client(this, listener.AcceptTcpClient());
                client.HandleAsync();
            }
        }

        /// <summary>
        /// Returns the path a dumped file with the given path would have.
        /// </summary>
        /// <param name="path">The path to transform into a dump path.</param>
        /// <returns>The path under which a dump file would be located.</returns>
        internal string GetDumpPath(string titleID, string path)
        {
            return Path.Combine(DumpDirectory, titleID, path.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        }

        /// <summary>
        /// Creates a <see cref="FileStream"/> for a dumped file for the file with the given path and title ID.
        /// </summary>
        /// <param name="titleID">The title ID of the game which file will be dumped.</param>
        /// <param name="path">The original path of the file.</param>
        /// <returns>The <see cref="FileStream"/> opened for dumping the file in.</returns>
        internal FileStream GetDumpStream(string titleID, string path)
        {
            string dumpPath = GetDumpPath(titleID, path);

            // Ensure that the directory under the dump path exists.
            Directory.CreateDirectory(Path.GetDirectoryName(dumpPath));

            // Return the file stream in the directory.
            return new FileStream(dumpPath, FileMode.Create, FileAccess.Write, FileShare.Read);
        }

        // ---- METHODS (PRIVATE) --------------------------------------------------------------------------------------

        private IEnumerable<IPAddress> GetLocalIPs()
        {
            // Return the IPv4 interface IPs on this machine.
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ipAddress in host.AddressList)
            {
                if (ipAddress.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ipAddress))
                {
                    yield return ipAddress;
                }
            }
        }
    }
}
