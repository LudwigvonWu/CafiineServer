using System.Diagnostics;
using System.Security.Cryptography;
using Syroot.CafiineServer.Common.IO;

namespace Syroot.CafiineServer.Pack
{
    /// <summary>
    /// Represents a file in a <see cref="GamePack"/> (server view).
    /// </summary>
    [DebuggerDisplay(@"\{{Name}\}")]
    internal class GamePackFile
    {
        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------
        
        /// <summary>
        /// Initializes a new instance of the <see cref="GamePackFile"/> class, decrypting its contents with the
        /// given cryptopgraphic algorithm and reading the data from the provided reader.
        /// </summary>
        /// <param name="cryptoAlgorithm">The algorithm to decrypt with.</param>
        /// <param name="reader">The <see cref="BinaryDataReader"/> to read the data from.</param>
        internal GamePackFile(SymmetricAlgorithm cryptoAlgorithm, BinaryDataReader reader)
        {
            ICryptoTransform cryptoTransform = cryptoAlgorithm.CreateDecryptor();

            // Read the file information.
            Name = cryptoTransform.DecryptString(reader.ReadBytes(reader.ReadInt16()));
            Length = reader.ReadInt32();
            Offset = reader.ReadInt64();
            EncryptedSize = reader.ReadInt32();
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
        /// Gets the size in bytes of the decrypted, original file data.
        /// </summary>
        internal int Length
        {
            get;
            private set;
        }
        
        /// <summary>
        /// Gets the absolute offset to the encrypted file data.
        /// </summary>
        internal long Offset
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the size in bytes of the encrypted file data.
        /// </summary>
        internal int EncryptedSize
        {
            get;
            private set;
        }
    }
}
