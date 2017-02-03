using System.Collections.Generic;

namespace Syroot.Cafiine.Server.Desktop
{
    /// <summary>
    /// Represents a collection of methods to parse command line parameters easier.
    /// </summary>
    public static class ParameterParser
    {
        // ---- METHODS (PUBLIC) ---------------------------------------------------------------------------------------

        /// <summary>
        /// Parses the provided command line parameters into a dictionary and returns it.
        /// </summary>
        /// <param name="args">The parameters to parse.</param>
        /// <returns>The dictionary containing the parameter keys and values.</returns>
        public static Dictionary<string, string> ParseToDictionary(string[] args)
        {
            // Arguments are split at a equals character. E.g. param1=value1 param2=value2 param3=value3
            Dictionary<string, string> arguments = new Dictionary<string, string>();
            foreach (string arg in args)
            {
                string argument = arg.Trim();
                int equalIndex = argument.IndexOf('=');
                string argumentKey = null;
                string argumentValue = null;
                if (equalIndex > 0)
                {
                    argumentKey = argument.Substring(0, equalIndex).TrimStart('-').TrimStart('/').ToUpper();
                    argumentValue = argument.Substring(equalIndex + 1);
                }
                else
                {
                    argumentKey = argument.TrimStart('-').TrimStart('/').ToUpper();
                }
                arguments.Add(argumentKey, argumentValue);
            }
            return arguments;
        }
    }
}
