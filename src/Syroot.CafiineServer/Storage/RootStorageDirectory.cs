using System.Collections.Generic;
using System.IO;
using Syroot.CafiineServer.Pack;

namespace Syroot.CafiineServer.Storage
{
    /// <summary>
    /// Represents a directory stored directly in the file system, containing game packs or other sub directories.
    /// </summary>
    internal class RootStorageDirectory : StorageDirectory
    {
        // ---- MEMBERS ------------------------------------------------------------------------------------------------
        
        private Dictionary<string, GamePack> _loadedGamePacks;
        private object                       _loadedGamePacksMutex;

        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="RootStorageDirectory"/> class for the given
        /// <see cref="DirectoryInfo"/>.
        /// </summary>
        /// <param name="directoryInfo">The directory to represent.</param>
        internal RootStorageDirectory(DirectoryInfo directoryInfo)
            : base(directoryInfo.Name)
        {
            DirectoryInfo = directoryInfo;

            // Create a dictionary to remember instantiated game packs (these are kept for the root directory).
            _loadedGamePacks = new Dictionary<string, GamePack>();
            _loadedGamePacksMutex = new object();
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets the represented <see cref="DirectoryInfo"/>.
        /// </summary>
        internal DirectoryInfo DirectoryInfo
        {
            get;
            private set;
        }

        // ---- METHODS (INTERNAL) -------------------------------------------------------------------------------------

        /// <summary>
        /// Returns the child directories in this directory.
        /// </summary>
        /// <returns>The list of child directories.</returns>
        internal override IEnumerable<StorageDirectory> GetDirectories()
        {
            // Read the raw child directories.
            foreach (DirectoryInfo subDirectory in DirectoryInfo.GetDirectories())
            {
                if (!subDirectory.Attributes.HasFlag(FileAttributes.Hidden))
                {
                    yield return new RawStorageDirectory(subDirectory);
                }
            }
            // Read the pack child directories.
            foreach (FileInfo packFile in DirectoryInfo.GetFiles("*" + GamePack.FileExtension))
            {
                if (!packFile.Attributes.HasFlag(FileAttributes.Hidden))
                {
                    // Load the game pack if it has not been loaded yet.
                    GamePack gamePack;
                    lock (_loadedGamePacksMutex)
                    {
                        if (!_loadedGamePacks.TryGetValue(packFile.FullName, out gamePack))
                        {
                            gamePack = new GamePack(packFile.FullName);
                            _loadedGamePacks.Add(packFile.FullName, gamePack);
                        }
                    }
                    yield return new PackStorageDirectory(gamePack, gamePack.RootDirectory);
                }
            }
        }

        /// <summary>
        /// Returns the files in this directory.
        /// </summary>
        /// <returns>The list of files.</returns>
        internal override IEnumerable<StorageFile> GetFiles()
        {
            // Read the files (which are not game packs).
            foreach (FileInfo file in DirectoryInfo.GetFiles())
            {
                if (!file.Attributes.HasFlag(FileAttributes.Hidden)
                    && file.Extension.ToLower() != GamePack.FileExtension)
                {
                    yield return new RawStorageFile(file);
                }
            }
        }
    }
}
