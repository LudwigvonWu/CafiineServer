using Syroot.CafiineServer.Pack;

namespace Syroot.CafiineServer.Storage
{
    /// <summary>
    /// Represents a directory stored in a <see cref="GamePack"/>.
    /// </summary>
    internal class PackStorageDirectory : StorageDirectory
    {
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
            foreach (GamePackDirectory subDirectory in GamePackDirectory.Directories)
            {
                Directories.Add(new PackStorageDirectory(gamePack, subDirectory));
            }

            // Read the files.
            foreach (GamePackFile file in GamePackDirectory.Files)
            {
                Files.Add(new PackStorageFile(gamePack, file));
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
    }
}
