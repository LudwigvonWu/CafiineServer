using System.IO;
using Syroot.CafiineServer.Pack;

namespace Syroot.CafiineServer.Storage
{
    /// <summary>
    /// Represents a file stored in a <see cref="GamePack"/>.
    /// </summary>
    internal class PackStorageFile : StorageFile
    {
        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="PackStorageFile"/> class for the given file in the provided
        /// <see cref="GamePack"/>.
        /// </summary>
        /// <param name="gamePack">The <see cref="GamePack"/> containing the file.</param>
        /// <param name="file">The file to represent.</param>
        internal PackStorageFile(GamePack gamePack, GamePackFile file)
            : base(file.Name, file.Length)
        {
            GamePack = gamePack;
            GamePackFile = file;
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets the <see cref="GamePack"/> in which this file is stored.
        /// </summary>
        internal GamePack GamePack
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the <see cref="GamePackFile"/> which represents the original file.
        /// </summary>
        internal GamePackFile GamePackFile
        {
            get;
            private set;
        }

        // ---- METHODS (INTERNAL) -------------------------------------------------------------------------------------

        /// <summary>
        /// Opens a read-only stream on the file.
        /// </summary>
        internal override Stream GetStream()
        {
            return null;
        }
    }
}
