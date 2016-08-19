using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using Syroot.CafiineServer.Common.IO;

namespace Syroot.CafiineServer.Pack
{
    /// <summary>
    /// Represents a directory in a <see cref="GamePack"/> (server view).
    /// </summary>
    [DebuggerDisplay(@"\{{Name}\}")]
    internal class GamePackDirectory
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
