using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace Syroot.Cafiine.PackCreator.Pack
{
    /// <summary>
    /// Represents a directory in a <see cref="GamePack"/> (creator view).
    /// </summary>
    internal class GamePackDirectory
    {
        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------
        
        /// <summary>
        /// Initializes a new instance of the <see cref="GamePackDirectory"/> class with the provided encryptor,
        /// representing the contents of the given directory.
        /// </summary>
        /// <param name="cryptoTransform">The encryptor to encrypt the name of the directory and contents with.</param>
        /// <param name="directory">The directory which contents will be represented.</param>
        /// <param name="newName">The optional new name of the directory to virtually rename it.</param>
        internal GamePackDirectory(ICryptoTransform cryptoTransform, DirectoryInfo directory, string newName = null)
        {
            // Use the new name if specified, and store it encrypted.
            EncryptedName = cryptoTransform.EncryptString(newName ?? directory.Name);

            // Add all files.
            Files = new List<GamePackFile>();
            foreach (FileInfo file in directory.GetFiles())
            {
                if (!file.Attributes.HasFlag(FileAttributes.Hidden))
                {
                    Files.Add(new GamePackFile(cryptoTransform, file));
                }
            }

            // Add all sub directories.
            Directories = new List<GamePackDirectory>();
            foreach (DirectoryInfo subDirectory in directory.GetDirectories())
            {
                if (!subDirectory.Attributes.HasFlag(FileAttributes.Hidden))
                {
                    Directories.Add(new GamePackDirectory(cryptoTransform, subDirectory));
                }
            }
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets the encrypted name data of the directory.
        /// </summary>
        internal byte[] EncryptedName
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the child directories in this directory.
        /// </summary>
        internal List<GamePackDirectory> Directories
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the files in this directory.
        /// </summary>
        internal List<GamePackFile> Files
        {
            get;
            private set;
        }
    }
}
