using System.IO;
using Syroot.CafiineServer.Pack;

namespace Syroot.CafiineServer.Storage
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
        /// <param name="directory">The directory to represent.</param>
        internal RawStorageDirectory(DirectoryInfo directory)
            : base(directory.Name)
        {
            // Read the raw child directories.
            foreach (DirectoryInfo subDirectory in directory.GetDirectories())
            {
                if (!subDirectory.Attributes.HasFlag(FileAttributes.Hidden))
                {
                    Directories.Add(new RawStorageDirectory(subDirectory));
                }
            }
            // Read the pack child directories.
            foreach (FileInfo packFile in directory.GetFiles("*" + GamePack.FileExtension))
            {
                if (!packFile.Attributes.HasFlag(FileAttributes.Hidden))
                {
                    GamePack gamePack = new GamePack(packFile.FullName);
                    Directories.Add(new PackStorageDirectory(gamePack, gamePack.RootDirectory));
                }
            }

            // Read the files (which are not game packs).
            foreach (FileInfo file in directory.GetFiles())
            {
                if (!file.Attributes.HasFlag(FileAttributes.Hidden)
                    && file.Extension.ToLower() != GamePack.FileExtension)
                {
                    Files.Add(new RawStorageFile(file));
                }
            }
        }
    }
}
