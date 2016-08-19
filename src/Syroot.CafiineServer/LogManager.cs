using System;
using System.Collections.Concurrent;
using System.IO;

namespace Syroot.CafiineServer
{
    /// <summary>
    /// Represents a manager of log files, accepting log messages and writing them into console and files.
    /// </summary>
    internal class LogManager
    {
        // ---- MEMBERS ------------------------------------------------------------------------------------------------
        
        private object                               _consoleMutex;
        private ConcurrentDictionary<string, object> _fileMutexes;
        
        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="LogManager"/> class, creating a session folder in the specified
        /// directory to store log files in.
        /// </summary>
        /// <param name="logsDirectory">The directory in which a session folder will be created to store logs in.</param>
        internal LogManager(string logsDirectory, bool enableFileLogs)
        {
            EnableFileLogs = enableFileLogs;
            SessionDirectory = Path.Combine(logsDirectory, DateTime.Now.ToString("yyyyMMdd HH.mm.ss"));

            _consoleMutex = new object();
            _fileMutexes = new ConcurrentDictionary<string, object>();

            // Ensure the output directory exists.
            if (EnableFileLogs)
            {
                Directory.CreateDirectory(SessionDirectory);
            }
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------
        
        /// <summary>
        /// Gets or sets a value indicating whether file logs will be written.
        /// </summary>
        internal bool EnableFileLogs
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the path in which log files are be stored.
        /// </summary>
        internal string SessionDirectory
        {
            get;
            private set;
        }

        // ---- METHODS (INTERNAL) -------------------------------------------------------------------------------------

        /// <summary>
        /// Writes the formatted message coming from the specified source into the console with the given color and into
        /// a corresponding log file.
        /// </summary>
        /// <param name="color">The color to use for console output.</param>
        /// <param name="source">The source which sent this message.</param>
        /// <param name="format">The format of the message.</param>
        /// <param name="args">The arguments to format the message with.</param>
        internal void Write(ConsoleColor color, string source, string format, params object[] args)
        {
            string message = String.Format(format, args) + Environment.NewLine;

            // Write the message to the console.
            lock (_consoleMutex)
            {
                ConsoleColor lastColor = Console.ForegroundColor;
                Console.ForegroundColor = color;
                Console.Write("[{0}] {1}", source, message);
                Console.ForegroundColor = lastColor;
            }

            // Write the message to the corresponding log file.
            if (EnableFileLogs)
            {
                object fileMutex = _fileMutexes.GetOrAdd(source, new object());
                lock (fileMutex)
                {
                    message = String.Format("[{0}] {1}", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss.fff"), message);
                    File.AppendAllText(Path.Combine(SessionDirectory, source) + ".txt", message);
                }
            }
        }
    }
}
