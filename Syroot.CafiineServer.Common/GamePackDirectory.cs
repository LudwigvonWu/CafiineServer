using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Syroot.CafiineServer.Common.IO;

namespace Syroot.CafiineServer.Common
{
    /// <summary>
    /// Represents a directory in a <see cref="GamePack"/>.
    /// </summary>
    public class GamePackDirectory
    {
        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="GamePackDirectory"/> class, decrypting its contents with the
        /// given cryptopgraphic algorithm and reading the data from the provided reader.
        /// </summary>
        /// <param name="cryptoAlgorithm">The algorithm to decrypt with.</param>
        /// <param name="reader">The <see cref="BinaryDataReader"/> to read the data from.</param>
        internal GamePackDirectory(SymmetricAlgorithm cryptoAlgorithm, BinaryDataReader reader)
        {
            ICryptoTransform cryptoTransform = cryptoAlgorithm.CreateDecryptor();

            // Read the directory information.
            Name = cryptoTransform.DecryptString(reader.ReadBytes(reader.ReadInt16()));

            // Read the files.
            int fileCount = reader.ReadInt32();
            Files = new List<GamePackFile>(fileCount);
            for (int i = 0; i < fileCount; i++)
            {
                Files.Add(new GamePackFile(cryptoAlgorithm, reader));
            }

            // Read the directories.
            int subDirectoryCount = reader.ReadInt32();
            Directories = new List<GamePackDirectory>(subDirectoryCount);
            for (int i = 0; i < subDirectoryCount; i++)
            {
                Directories.Add(new GamePackDirectory(cryptoAlgorithm, reader));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GamePackDirectory"/> class with the provided encryptor,
        /// representing the contents of the given directory.
        /// </summary>
        /// <param name="cryptoTransform">The encryptor to encrypt the name of the directory and contents with.</param>
        /// <param name="directory">The directory which contents will be represented.</param>
        internal GamePackDirectory(ICryptoTransform cryptoTransform, DirectoryInfo directory)
        {
            // Store the name encrypted.
            EncryptedName = cryptoTransform.EncryptString(directory.Name);

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
        /// Gets the name of the directory. This is only set for loaded packs.
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the child directories in this directory.
        /// </summary>
        public List<GamePackDirectory> Directories
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the files in this directory.
        /// </summary>
        public List<GamePackFile> Files
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the encrypted name data of the directory. This is only set while a pack is being created.
        /// </summary>
        internal byte[] EncryptedName
        {
            get;
            private set;
        }
    }
}
