using System.IO;

namespace Syroot.CafiineServer.Storage
{
    /// <summary>
    /// Represents a file stored directly in the file system.
    /// </summary>
    internal class RawStorageFile : StorageFile
    {
        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="RawStorageFile"/> class for the given <see cref="FileInfo"/>.
        /// </summary>
        /// <param name="file">The file to represent.</param>
        internal RawStorageFile(FileInfo file)
            : base(file.Name, (int)file.Length)
        {
            FileInfo = file;
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets the <see cref="FileInfo"/> which represents the raw file.
        /// </summary>
        internal FileInfo FileInfo
        {
            get;
            private set;
        }

        // ---- METHODS (INTERNAL) -------------------------------------------------------------------------------------

        /// <summary>
        /// Opens a read-only stream on the file.
        /// </summary>
        /// <returns>A read-only stream instance on the file.</returns>
        internal override Stream GetStream()
        {
            return new FileStream(FileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
        }
    }
}
