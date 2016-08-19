using System.Diagnostics;
using System.IO;

namespace Syroot.CafiineServer.Storage
{
    /// <summary>
    /// Represents a file in the storage contents.
    /// </summary>
    [DebuggerDisplay(@"\{{Name}\}")]
    internal abstract class StorageFile
    {
        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageDirectory"/> class with the given file information.
        /// </summary>
        /// <param name="name">The name of the file.</param>
        /// <param name="length">The size of the file in bytes.</param>
        internal StorageFile(string name, int length)
        {
            Name = name;
            Length = length;
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        internal string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the size of the file in bytes.
        /// </summary>
        internal int Length
        {
            get;
            private set;
        }

        // ---- METHODS (INTERNAL) -------------------------------------------------------------------------------------

        /// <summary>
        /// Opens a read-only stream on the file.
        /// </summary>
        /// <returns>A read-only stream instance on the file.</returns>
        internal abstract Stream GetStream();
    }
}
