using System;
using System.Collections.Generic;
using System.IO;
using Syroot.Cafiine.Server.Pack;

namespace Syroot.Cafiine.Server.Storage
{
    /// <summary>
    /// Represents a directory stored directly in the file system.
    /// </summary>
    internal class RawStorageDirectory : StorageDirectory
    {
        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="RawStorageDirectory"/> class for the given
        /// <see cref="DirectoryInfo"/>.
        /// </summary>
        /// <param name="directoryInfo">The directory to represent.</param>
        internal RawStorageDirectory(DirectoryInfo directoryInfo)
            : base(directoryInfo.Name)
        {
            DirectoryInfo = directoryInfo;
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
