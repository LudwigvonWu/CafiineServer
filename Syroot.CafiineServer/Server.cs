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
        /// <param name="logDirectory">The log directory into which log files will be written.</param>
        internal Server(IPAddress ipAddress, int port, string dataDirectory, string dumpDirectory, string logDirectory)
        {
            IPAddress = ipAddress;
            Port = port;
            DataDirectory = dataDirectory;
            DumpDirectory = dumpDirectory;
            LogDirectory = logDirectory;

            // Ensure the directories exist.
            Directory.CreateDirectory(DataDirectory);
            Directory.CreateDirectory(DumpDirectory);
            Directory.CreateDirectory(LogDirectory);

            // Initialize the storage system.
            Storage = new StorageSystem(DataDirectory);
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
        internal string LogDirectory
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
            Log(ConsoleColor.Yellow, "Cafiine server started.");
            Log(ConsoleColor.Yellow, "Local Server IPs: {0} (on port {1})", String.Join(", ", GetLocalIPs()), Port);
            Log(ConsoleColor.Yellow, "Data directory  : {0}", Path.GetFullPath(DataDirectory));
            Log(ConsoleColor.Yellow, "Dump directory  : {0}", Path.GetFullPath(DumpDirectory));
            Log(ConsoleColor.Yellow, "Log directory   : {0}", Path.GetFullPath(LogDirectory));
            Log(ConsoleColor.DarkYellow, "Listening for new connections...");

            // Repeatedly wait for new incoming connections.
            while (true)
            {
                Client client = new Client(this, listener.AcceptTcpClient());
                client.HandleAsync();
            }
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
        
        private void Log(ConsoleColor color, string format, params object[] args)
        {
            Console.ForegroundColor = color;
            Console.WriteLine("[SERVER] " + format, args);
        }
    }
}
