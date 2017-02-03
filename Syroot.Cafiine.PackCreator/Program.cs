using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Syroot.Cafiine.PackCreator.Pack;

namespace Syroot.Cafiine.PackCreator
{
    /// <summary>
    /// The main class of the application containing the program entry point.
    /// </summary>
    internal static class Program
    {
        // ---- MEMBERS ------------------------------------------------------------------------------------------------

        private static string _target;
        private static string _source;
        private static string _rootName;
        private static DateTime _minDate;
        private static DateTime _maxDate;

        // ---- METHODS (PRIVATE) --------------------------------------------------------------------------------------

        private static int Main(string[] args)
        {
            try
            {
                Dictionary<string, string> arguments = ParameterParser.ParseToDictionary(args);
                ParseParameters(arguments);

                // Check for requested help.
                if (String.IsNullOrEmpty(_target) || arguments.ContainsKey("?") || arguments.ContainsKey("HELP"))
                {
                    PrintHelp();
                    return -1;
                }

                // Print the current options.
                PrintParameters();

                // Check for problematic input.
                if (!Directory.Exists(_source))
                {
                    throw new InvalidOperationException("Source directory does not exist.");
                }
                if (_minDate >= _maxDate)
                {
                    throw new InvalidOperationException("Minimum usage date must be before the maximum usage date.");
                }

                // Create the game pack.
                Console.Write("Creating game pack... ");
                GamePack.CreateFile(_target, _source, _rootName, _minDate, _maxDate);
                Console.WriteLine("done.");

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.Error.WriteLine("Error: " + ex.Message);
                return -1;
            }
        }

        private static void ParseParameters(Dictionary<string, string> arguments)
        {
            // Get the target pack file name. Force the csgp extension.
            if (arguments.TryGetValue("TARGET", out _target))
            {
                _target = Path.ChangeExtension(_target, GamePack.FileExtension);
            }

            // Get the source directory which files will be included. Defaults to current directory.
            if (!arguments.TryGetValue("SOURCE", out _source))
            {
                _source = Directory.GetCurrentDirectory();
            }

            // Get the optional new root name.
            if (!arguments.TryGetValue("ROOTNAME", out _rootName))
            {
                _rootName = Path.GetFileName(_source);
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

        private static void PrintHelp()
        {
            Console.WriteLine("Creates an encrypted game pack for use in Cafiine Server.");
            Console.WriteLine();
            Console.WriteLine("PACKCREATOR /TARGET=FileName [/SOURCE=SourceDirectory] [/ROOTNAME=NewRootName]");
            Console.WriteLine("            [/MINDATE=00:00-01.01.2015] [/MAXDATE=23:59-31.12.2015]");
            Console.WriteLine();
            Console.WriteLine("        TARGET   The name of the game pack file into which the game data will");
            Console.WriteLine("                 be stored. The file extension will be forced to {0} to be",
                GamePack.FileExtension);
            Console.WriteLine("                 recognized by the server.");
            Console.WriteLine("        SOURCE   The source directory which will be stored in the game pack");
            Console.WriteLine("                 file. This would be a typical title ID directory. Defaults to");
            Console.WriteLine("                 the current directory.");
            Console.WriteLine("        ROOTNAME If set, the source directory will be stored under this name");
            Console.WriteLine("                 instead of its real name (useful for changing the title ID).");
            Console.WriteLine("        MINDATE  The earliest UTC date and time from which on the game pack can");
            Console.WriteLine("                 be used. Defaults to no limit.");
            Console.WriteLine("        MAXDATE  The latest UTC date and time after which the game pack cannot");
            Console.WriteLine("                 be used anymore. Defaults to no limit.");
        }

        private static void PrintParameters()
        {
            // Target.
            Console.WriteLine("Target file       : {0}", _target);

            // Source.
            Console.WriteLine("Source directory  : {0}", _source);

            // Root name.
            Console.WriteLine("Root name         : {0}", _rootName);

            // Minimum date.
            Console.Write("Minimum usage date: ");
            if (_minDate == DateTime.MinValue)
            {
                Console.WriteLine("No limit");
            }
            else
            {
                Console.WriteLine("{0} ({1} local time)", _minDate, _minDate.ToLocalTime());
            }

            // Maximum date.
            Console.Write("Maximum usage date: ");
            if (_maxDate == DateTime.MaxValue)
            {
                Console.WriteLine("No limit");
            }
            else
            {
                Console.WriteLine("{0} ({1} local time)", _maxDate, _maxDate.ToLocalTime());
            }

            Console.WriteLine();
        }
    }
}
