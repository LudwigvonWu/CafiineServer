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
            Directories = new List<StorageDirectory>();
            Files = new List<StorageFile>();
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

        /// <summary>
        /// Gets the child directories in this directory.
        /// </summary>
        internal List<StorageDirectory> Directories
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the files in this directory.
        /// </summary>
        internal List<StorageFile> Files
        {
            get;
            private set;
        }
    }
}
