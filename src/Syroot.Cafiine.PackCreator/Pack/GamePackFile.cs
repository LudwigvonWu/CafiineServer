using System.IO;
using System.Security.Cryptography;

namespace Syroot.Cafiine.PackCreator.Pack
{
    /// <summary>
    /// Represents a file in a <see cref="GamePack"/> (creator view).
    /// </summary>
    internal class GamePackFile
    {
        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------
        
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
            Length = (int)file.Length;
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets the full path to the file.
        /// </summary>
        internal string FullPath
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the encrypted name data of the file.
        /// </summary>
        internal byte[] EncryptedName
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
