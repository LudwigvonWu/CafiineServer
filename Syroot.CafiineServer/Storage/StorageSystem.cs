using System.IO;

namespace Syroot.CafiineServer.Storage
{
    /// <summary>
    /// Represents a unified access to data stored raw on the file system or inside game packs.
    /// </summary>
    internal class StorageSystem
    {
        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageSystem"/> class from the given root directory.
        /// </summary>
        /// <param name="rootDirectory">The directory to represent.</param>
        internal StorageSystem(string rootDirectory)
        {
            RootDirectory = new RawStorageDirectory(new DirectoryInfo(rootDirectory));
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets the root directory of the storage system which contains all available game data.
        /// </summary>
        internal StorageDirectory RootDirectory
        {
            get;
            private set;
        }
    }
}
