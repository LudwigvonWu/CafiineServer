using System.Collections.Generic;
using Syroot.CafiineServer.Pack;

namespace Syroot.CafiineServer.Storage
{
    /// <summary>
    /// Represents a directory stored in a <see cref="GamePack"/>.
    /// </summary>
    internal class PackStorageDirectory : StorageDirectory
    {
        // ---- MEMBERS ------------------------------------------------------------------------------------------------

        private List<PackStorageDirectory> _directories;
        private List<PackStorageFile>      _files;

        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="PackStorageDirectory"/> class for the given directory in the
        /// provided <see cref="GamePack"/>.
        /// </summary>
        /// <param name="gamePack">The <see cref="GamePack"/> containing the directory.</param>
        /// <param name="directory">The directory to represent.</param>
        internal PackStorageDirectory(GamePack gamePack, GamePackDirectory directory)
            : base(directory.Name)
        {
            GamePack = gamePack;
            GamePackDirectory = directory;

            // Read the child directories.
            _directories = new List<PackStorageDirectory>();
            foreach (GamePackDirectory subDirectory in GamePackDirectory.Directories)
            {
                _directories.Add(new PackStorageDirectory(gamePack, subDirectory));
            }

            // Read the files.
            _files = new List<PackStorageFile>();
            foreach (GamePackFile file in GamePackDirectory.Files)
            {
                _files.Add(new PackStorageFile(gamePack, file));
            }
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets the <see cref="GamePack"/> in which this directory is stored.
        /// </summary>
        internal GamePack GamePack
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the <see cref="GamePackDirectory"/> which represents the original directory.
        /// </summary>
        internal GamePackDirectory GamePackDirectory
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
            return _directories;
        }

        /// <summary>
        /// Returns the files in this directory.
        /// </summary>
        /// <returns>The list of files.</returns>
        internal override IEnumerable<StorageFile> GetFiles()
        {
            return _files;
        }
    }
}
