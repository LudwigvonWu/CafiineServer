using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Syroot.CafiineServer.PackCreator.Pack;

namespace Syroot.CafiineServer.PackCreator
{
    /// <summary>
    /// The main class of the application containing the program entry point.
    /// </summary>
    internal static class Program
    {
        // ---- MEMBERS ------------------------------------------------------------------------------------------------

        private static string   _target;
        private static string   _source;
        private static DateTime _minDate;
        private static DateTime _maxDate;

        // ---- METHODS (PRIVATE) --------------------------------------------------------------------------------------

        private static int Main(string[] args)
        {
            try
            {
                Dictionary<string, string> arguments = GetParameterDictionary(args);
                ParseParameters(arguments);

                // Check for requested help.
                if (String.IsNullOrEmpty(_target) || arguments.ContainsKey("?") || arguments.ContainsKey("HELP"))
                {
                    PrintHelp();
                    return -1;
                }
                // Print the current options.
                Console.WriteLine("Target file       : " + _target);
                Console.WriteLine("Source directory  : " + _source);
                Console.WriteLine("Minimum usage date: " + _minDate);
                Console.WriteLine("Maximum usage date: " + _maxDate);
                // Check for problematic input.
                if (Path.GetExtension(_target).ToLower() != GamePack.FileExtension)
                {
                    Console.WriteLine("Warning: For game packs to be recognized by the server, they require a .csgp "
                        + "file extension.");
                }
                if (!Directory.Exists(_source))
                {
                    throw new InvalidOperationException("Source directory does not exist.");
                }
                if (_minDate >= _maxDate)
                {
                    throw new InvalidOperationException("Minimum usage date must be before the maximum usage date.");
                }
                // Create the game pack.
                Console.WriteLine("Creating game pack...");
                GamePack.CreateFile(_target, _source, _minDate, _maxDate);
                Console.WriteLine("Done.");

                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Error: " + ex.Message);
                return -1;
            }
        }

        private static void PrintHelp()
        {
            Console.WriteLine("Creates an encrypted game pack for use in Cafiine Server.");
            Console.WriteLine();
            Console.WriteLine("PACKCREATOR TARGET=FileName [SOURCE=SourceDirectory]");
            Console.WriteLine("            [MINDATE=00:00-01.01.2015] [MAXDATE=23:59-31.12.2015]");
            Console.WriteLine();
            Console.WriteLine("        TARGET   The name of the game pack file into which the game data will");
            Console.WriteLine("                 be stored. The file extension should be .csgp to be recognized");
            Console.WriteLine("                 by the server.");
            Console.WriteLine("        SOURCE   The source directory which will be stored in the game pack");
            Console.WriteLine("                 file. This would be a typical title ID directory. Defaults to");
            Console.WriteLine("                 the current directory.");
            Console.WriteLine("        MINDATE  The earliest UTC date and time from which on the game pack can");
            Console.WriteLine("                 be used. Defaults to no limit.");
            Console.WriteLine("        MAXDATE  The latest UTC date and time after which the game pack cannot");
            Console.WriteLine("                 be used anymore. Defaults to no limit.");
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
            // Get the target pack file name.
            arguments.TryGetValue("TARGET", out _target);

            // Get the source directory which files will be included. Defaults to current directory.
            if (!arguments.TryGetValue("SOURCE", out _source))
            {
                _source = Environment.CurrentDirectory;
            }

            // Get the minimum date and time from which on the game pack can be used.
            _minDate = DateTime.MinValue;
            string paramMinDate;
            if (arguments.TryGetValue("MINDATE", out paramMinDate))
            {
                _minDate = DateTime.ParseExact(paramMinDate, "HH:mm-dd.MM.yyyy", CultureInfo.InvariantCulture);
            }

            // Get the maximum date and time from which on the game pack stops working.
            _maxDate = DateTime.MaxValue;
            string paramMaxDate;
            if (arguments.TryGetValue("MAXDATE", out paramMaxDate))
            {
                _maxDate = DateTime.ParseExact(paramMaxDate, "HH:mm-dd.MM.yyyy", CultureInfo.InvariantCulture);
                _maxDate = _maxDate.ToUniversalTime();
            }
        }
    }
}
