using System.IO;
using System.Security.Cryptography;
using Syroot.CafiineServer.Common.IO;

namespace Syroot.CafiineServer.Common
{
    /// <summary>
    /// Represents a file in a <see cref="GamePack"/>.
    /// </summary>
    public class GamePackFile
    {
        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        internal GamePackFile(SymmetricAlgorithm cryptoAlgorithm, BinaryDataReader reader)
        {
            ICryptoTransform crypotTransform = cryptoAlgorithm.CreateDecryptor();

            // Read the file information.
            Name = crypotTransform.DecryptString(reader.ReadBytes(reader.ReadInt16()));
            Size = reader.ReadInt32();
            Offset = reader.ReadInt64();
            EncryptedSize = reader.ReadInt32();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GamePackFile"/> class with the provided encryptor, representing
        /// the contents of the given file.
        /// </summary>
        /// <param name="cryptoTransform">The encryptor to encrypt the name of the file and contents with.</param>
        /// <param name="file">The file which contents will be represented.</param>
        internal GamePackFile(ICryptoTransform cryptoTransform, FileInfo file)
        {
            // Store the file information.
            EncryptedName = cryptoTransform.EncryptString(file.Name);
            FullPath = file.FullName;
            Size = (int)file.Length;
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets the name of the file. This is only set for loaded packs.
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the size in bytes of the decrypted, original file data.
        /// </summary>
        public int Size
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the encrypted name data of the file. This is only set while a pack is being created.
        /// </summary>
        internal byte[] EncryptedName
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the full path to the file. This is only set while a pack is being created.
        /// </summary>
        internal string FullPath
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
