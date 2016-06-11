using System;
using System.IO;
using System.Net;

namespace Syroot.CafiineServer
{
    /// <summary>
    /// The main class of the application containing the program entry point.
    /// </summary>
    internal static class Program
    {
        // ---- METHODS (PRIVATE) --------------------------------------------------------------------------------------

        private static void Main(string[] args)
        {
            // The first parameter can include an alternative path for the data directory, which must exist.
            string dataDirectory = "data";
            if (args.Length > 1)
            {
                Console.Error.WriteLine("Usage: CafiineServer [dataDirectory]");
                return;
            }
            else if (args.Length == 1)
            {
                dataDirectory = args[0];
                if (!Directory.Exists(dataDirectory))
                {
                    Console.Error.WriteLine("Custom data directory '{0}' does not exist.", dataDirectory);
                    return;
                }
            }
            
            // Create a server and make it listen for incoming connections.
            Server server = new Server(IPAddress.Any, 7332, dataDirectory, "dump", "logs");
            server.Run();
        }
    }
}
