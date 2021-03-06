﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Syroot.Cafiine.Server.Desktop
{
    /// <summary>
    /// The main class of the application containing the program entry point.
    /// </summary>
    internal static class Program
    {
        // ---- MEMBERS ------------------------------------------------------------------------------------------------

        private static int _port;
        private static IPAddress _ipAddress;
        private static string _dataPath;
        private static string _logsPath;
        private static string _dumpPath;
        private static bool _dumpAll;
        private static bool _dumpAllSlow;
        private static bool _enabledFileLogs;

        // ---- METHODS (PRIVATE) --------------------------------------------------------------------------------------

        private static void Main(string[] args)
        {
            // Wrap in an asynchronous context to prevent .NET standard 1.6 async methods to not end the program.
            Task.Run(async () =>
            {
                try
                {
                    Dictionary<string, string> arguments = ParameterParser.ParseToDictionary(args);
                    ParseParameters(arguments);

                    // Check for requested help.
                    if (arguments.ContainsKey("?") || arguments.ContainsKey("HELP"))
                    {
                        PrintHelp();
                    }
                    // Create a server and make it listen for incoming connections.
                    Server server = new Server(_ipAddress, _port, _dataPath, _dumpPath, _logsPath, _dumpAll,
                        _dumpAllSlow, _enabledFileLogs);
                    await server.Run();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unexpected error: " + ex.Message);
                }
            }).Wait();
        }

        private static void PrintHelp()
        {
            Console.WriteLine("Communicates with Wii U Cafiine clients and allows file dump and replacement.");
            Console.WriteLine();
            Console.WriteLine("CAFIINESERVER [/PORT=7332] [/IP=192.168.1.10] [/DATA=DataPath] [/DUMP=DumpPath]");
            Console.WriteLine("              [/LOGS=LogsPath] [/DUMPALL] [/DUMPALLSLOW] [/NOLOGS]");
            Console.WriteLine();
            Console.WriteLine("        PORT     	The port under which the server will listen for incoming client");
            Console.WriteLine("                 	connections. Defaults to 7332.");
            Console.WriteLine("        IP       	The IP address on which the server runs. Defaults to all IPv4");
            Console.WriteLine("                 	network interfaces of the computer.");
            Console.WriteLine("        DATA     	The path to the game data directory containing either game packs");
            Console.WriteLine("                 	or title ID folders. Defaults to 'data'.");
            Console.WriteLine("        DUMP     	The path to the directory in which file dumps will be stored.");
            Console.WriteLine("                 	Defaults to 'dump'.");
            Console.WriteLine("        LOGS     	The path to the directory in which logs will be stored. Defaults");
            Console.WriteLine("                 	to 'logs'.");
            Console.WriteLine("        DUMPALL  	When specified, the server dumps any file queried by the client.");
            Console.WriteLine("                 	Files will not be replaced even if available.");
            Console.WriteLine("        DUMPALLSLOW	When specified, the server dumps any file queried by the client.");
            Console.WriteLine("                 	Slow mode will be used for better stability. Files will not be ");
            Console.WriteLine("                 	replaced even if available.");
            Console.WriteLine("        NOLOGS   	When specified, no file logs will be written (but console output");
            Console.WriteLine("                 	is still visible).");
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

            // Check if slow dump mode is set.
            _dumpAllSlow = arguments.ContainsKey("DUMPALLSLOW");

            // Check if no logging should be done.
            _enabledFileLogs = !arguments.ContainsKey("NOLOGS");
        }
    }
}