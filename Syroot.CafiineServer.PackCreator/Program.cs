using System;
using System.Collections.Generic;
using Syroot.CafiineServer.Common;

namespace Syroot.CafiineServer.PackCreator
{
    internal class Program
    {
        // ---- MEMBERS ------------------------------------------------------------------------------------------------

        private static Dictionary<string, string> _arguments;

        // ---- METHODS (PRIVATE) --------------------------------------------------------------------------------------

        private static void Main(string[] args)
        {
            ParseArguments(args);
            //GamePack.Create(@"D:\Pictures\test.csgp",
            //    @"D:\Archive\Hacking\Wii U\Cafiine\cafiine_root\00050000-1010ED00",
            //    DateTime.MinValue, DateTime.MaxValue);
            GamePack bla = new GamePack(@"D:\Pictures\test.csgp");
        }

        private static void ParseArguments(string[] args)
        {
            _arguments = new Dictionary<string, string>();
            foreach (string arg in args)
            {
                string argument = arg.Trim();
                int equalIndex = argument.IndexOf('=');
                if (equalIndex > 0)
                {
                    _arguments.Add(argument.Substring(0, equalIndex), argument.Substring(equalIndex + 1));
                }
                else
                {
                    _arguments.Add(argument, null);
                }
            }
        }
    }
}
