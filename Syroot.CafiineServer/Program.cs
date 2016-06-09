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
            // The first parameter can include an alternative path for the root directory, which must exist.
            string rootDirectory = "root";
            if (args.Length > 1)
            {
                Console.Error.WriteLine("Usage: CafiineServer [rootDirectory]");
                return;
            }
            else if (args.Length == 1)
            {
                rootDirectory = args[0];
                if (!Directory.Exists(rootDirectory))
                {
                    Console.Error.WriteLine("Custom root directory '{0}' does not exist.", rootDirectory);
                    return;
                }
            }
            
            // Create a server and make it listen for incoming connections.
            Server server = new Server(IPAddress.Any, 7332, rootDirectory, "logs");
            server.Run();
        }
    }
}
