using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

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
        /// <param name="rootDirectory">The root directory containing the game data.</param>
        /// <param name="logDirectory">The log directory into which log files will be written.</param>
        internal Server(IPAddress ipAddress, int port, string rootDirectory, string logDirectory)
        {
            IPAddress = ipAddress;
            Port = port;
            RootDirectory = rootDirectory;
            LogDirectory = logDirectory;

            // Ensure the directories exist.
            Directory.CreateDirectory(RootDirectory);
            Directory.CreateDirectory(LogDirectory);
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
        internal string RootDirectory
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
            Log(ConsoleColor.Yellow, "Listening for connections on port {0}...", Port);

            // Repeatedly wait for new incoming connections.
            while (true)
            {
                Client client = new Client(this, listener.AcceptTcpClient());
                client.HandleAsync();
            }
        }
        
        // ---- METHODS (PRIVATE) --------------------------------------------------------------------------------------

        /// <summary>
        /// Appends the message with the given format and arguments to the server log file.
        /// </summary>
        /// <param name="color">The color to use for the output in the console.</param>
        /// <param name="format">The format of the message.</param>
        /// <param name="args">The arguments to format the message with.</param>
        private void Log(ConsoleColor color, string format, params object[] args)
        {
            Console.ForegroundColor = color;
            Console.WriteLine("[SERVER] " + format, args);
        }
    }
}
