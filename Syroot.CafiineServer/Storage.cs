using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Syroot.CafiineServer
{
    internal class Storage
    {
        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        internal Storage(string rootDirectory)
        {
            RootDirectory = rootDirectory;

            LoadRawFileSystem(RootDirectory);
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        internal StorageDirectory RootDirectory
        {
            get;
            private set;
        }

        // ---- METHODS (PRIVATE) --------------------------------------------------------------------------------------

        private void LoadRawFileSystem(string directory)
        {
            // Read in raw directories and files.
            foreach (string folder in Directory.GetDirectories(rootDirectory))
            {
            }
        }
    }

    internal abstract class StorageDirectory
    {
        internal abstract StorageDirectory GetDirectories();
    }

    internal abstract class StorageFile
    {
    }

    internal class StorageRootDirectory
    {

    }
}
