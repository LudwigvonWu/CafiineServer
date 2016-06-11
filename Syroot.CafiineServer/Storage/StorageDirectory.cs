using System.Collections.Generic;
using System.Diagnostics;

namespace Syroot.CafiineServer.Storage
{
    /// <summary>
    /// Represents a directory in the storage contents.
    /// </summary>
    [DebuggerDisplay(@"\{{Name}\}")]
    internal abstract class StorageDirectory
    {
        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageDirectory"/> class with the given directory information.
        /// </summary>
        /// <param name="name">The name of the directory.</param>
        internal StorageDirectory(string name)
        {
            Name = name;
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets the name of the directory.
        /// </summary>
        internal string Name
        {
            get;
            private set;
        }

        // ---- METHODS (INTERNAL) -------------------------------------------------------------------------------------

        /// <summary>
        /// Returns the child directories in this directory.
        /// </summary>
        /// <returns>The list of child directories.</returns>
        internal abstract IEnumerable<StorageDirectory> GetDirectories();

        /// <summary>
        /// Returns the files in this directory.
        /// </summary>
        /// <returns>The list of files.</returns>
        internal abstract IEnumerable<StorageFile> GetFiles();
    }
}
