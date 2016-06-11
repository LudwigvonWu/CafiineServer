using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Syroot.CafiineServer
{
    /// <summary>
    /// The main class of the application containing the program entry point.
    /// </summary>
    internal static class Program
    {
        // ---- MEMBERS ------------------------------------------------------------------------------------------------

        private static int       _port;
        private static IPAddress _ipAddress;
        private static string    _dataPath;
        private static string    _logsPath;
        private static string    _dumpPath;
        private static bool      _dumpAll;

        // ---- METHODS (PRIVATE) --------------------------------------------------------------------------------------

        private static int Main(string[] args)
        {
            try
            {
                Dictionary<string, string> arguments = GetParameterDictionary(args);
                ParseParameters(arguments);

                // Check for requested help.
                if (arguments.ContainsKey("?") || arguments.ContainsKey("HELP"))
                {
                    PrintHelp();
                    return -1;
                }
                // Create a server and make it listen for incoming connections.
                Server server = new Server(_ipAddress, _port, _dataPath, _dumpPath, _logsPath);
                server.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected error: " + ex.Message);
                return -1;
            }
            return 0;
        }

        private static void PrintHelp()
        {
            Console.WriteLine("Communicates with Wii U Cafiine clients and allows file dump and replacement.");
            Console.WriteLine();
            Console.WriteLine("CAFIINESERVER [PORT=7332] [IP=192.168.1.10] [DATA=DataPath] [DUMP=DumpPath]");
            Console.WriteLine("              [LOGS=LogsPath] [DUMPALL]");
            Console.WriteLine();
            Console.WriteLine("        PORT     The port under which the server will listen for incoming client");
            Console.WriteLine("                 connections. Defaults to 7332.");
            Console.WriteLine("        IP       The IP address on which the server runs. Defaults to all IPv4");
            Console.WriteLine("                 network interfaces of the computer.");
            Console.WriteLine("        DATA     The path to the game data directory containing either game packs");
            Console.WriteLine("                 or title ID folders. Defaults to 'data'.");
            Console.WriteLine("        DUMP     The path to the directory in which file dumps will be stored.");
            Console.WriteLine("                 Defaults to 'dump'.");
            Console.WriteLine("        LOGS     The path to the directory in which logs will be stored. Defaults");
            Console.WriteLine("                 to 'logs'.");
            Console.WriteLine("        DUMPALL  When specified, the server dumps any file queried by the client.");
        }

        private static Dictionary<string, string> GetParameterDictionary(string[] args)
        {
            // Arguments are split at a equals character. E.g. param1=value1 param2=value2 param3=value3
            Dictionary<string, string> arguments = new Dictionary<string, string>();
            foreach (string arg in args)
            {
                string argument = arg.Trim();
                int equalIndex = argument.IndexOf('=');
                if (equalIndex > 0)
                {
                    arguments.Add(argument.Substring(0, equalIndex).ToUpper(), argument.Substring(equalIndex + 1));
                }
                else
                {
                    arguments.Add(argument.ToUpper(), null);
                }
            }
            return arguments;
        }

        private static void ParseParameters(Dictionary<string, string> arguments)
        {
            // Get the port under which to listen.
            _port = 7332;
            string paramPort;
            if (arguments.TryGetValue("PORT", out paramPort))
            {
                _port = short.Parse(paramPort);
            }

            // Get the IP address on which the server will be run.
            _ipAddress = IPAddress.Any;
            string paramIP;
            if (arguments.TryGetValue("IP", out paramIP))
            {
                _ipAddress = IPAddress.Parse(paramIP);
            }

            // Get the data path.
            if (!arguments.TryGetValue("DATA", out _dataPath))
            {
                _dataPath = "data";
            }

            // Get the dump path.
            if (!arguments.TryGetValue("DUMP", out _dumpPath))
            {
                _dumpPath = "dump";
            }

            // Get the logs path.
            if (!arguments.TryGetValue("LOGS", out _logsPath))
            {
                _logsPath = "logs";
            }

            // Check if dump mode is set.
            _dumpAll = arguments.ContainsKey("DUMPALL");
        }
    }
}
