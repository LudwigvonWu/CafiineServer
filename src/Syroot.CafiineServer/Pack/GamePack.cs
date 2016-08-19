using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Syroot.CafiineServer.Common.IO;

namespace Syroot.CafiineServer.Pack
{
    /// <summary>
    /// Represents an encrypted container file providing game data (server view).
    /// </summary>
    internal class GamePack
    {
        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        /// <summary>
        /// The extension which has to be used for game packs.
        /// </summary>
        internal const string FileExtension = ".csgp";

        // ---- MEMBERS ------------------------------------------------------------------------------------------------

        private SymmetricAlgorithm _cryptoAlgorithm;

        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="GamePack"/> class, loading the given pack file.
        /// </summary>
        /// <param name="fileName">The name of the file containing pack data.</param>
        internal GamePack(string fileName)
        {
            FileName = fileName;

            using (FileStream stream = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (BinaryDataReader reader = new BinaryDataReader(stream))
            {
                // Read the file header.
                if (reader.ReadString(4) != "CSGP")
                {
                    throw new InvalidDataException("Invalid game pack data.");
                }
                long flags = reader.ReadInt64(); // Unused yet.

                // Read and check the MD5 hash.
                byte[] md5Hash = reader.ReadBytes(16);
                using (reader.TemporarySeek(0))
                using (MD5 md5 = MD5.Create())
                {
                    if (!md5.ComputeHash(stream).SequenceEqual(md5Hash))
                    {
                        throw new InvalidDataException("Invalid game pack data.");
                    }
                }

                // Read in the time bomb dates and check if its still valid.
                ValidFrom = reader.ReadDateTime(BinaryDateTimeFormat.NetTicks);
                ValidTo = reader.ReadDateTime(BinaryDateTimeFormat.NetTicks);
                DateTime now = DateTime.UtcNow;
                if (now < ValidFrom || now > ValidTo)
                {
                    throw new InvalidDataException("Invalid game pack data.");
                }

                // Read in the keys and generate the crypto provider.
                _cryptoAlgorithm = new AesCryptoServiceProvider();
                _cryptoAlgorithm.Key = reader.ReadBytes(reader.ReadByte());
                _cryptoAlgorithm.IV = reader.ReadBytes(reader.ReadByte());

                // Read in the directory and file headers.
                RootDirectory = new GamePackDirectory(_cryptoAlgorithm, reader);
            }
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets the name of the file in which this game pack is stored.
        /// </summary>
        internal string FileName
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the time and date from which this pack can be used.
        /// </summary>
        internal DateTime ValidFrom
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the time and date until which this pack can be used.
        /// </summary>
        internal DateTime ValidTo
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the <see cref="SymmetricAlgorithm"/> to use for decrypting files.
        /// </summary>
        internal SymmetricAlgorithm CryptoAlgorithm
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the main directory of the pack, which should be named after the title ID of the affected game.
        /// </summary>
        internal GamePackDirectory RootDirectory
        {
            get;
            private set;
        }

        // ---- METHODS (INTERNAL) -------------------------------------------------------------------------------------

        /// <summary>
        /// Returns the decrypted file data of the given <see cref="GamePackFile"/>.
        /// </summary>
        /// <param name="file">The <see cref="GamePackFile"/> which data will be decrypted.</param>
        /// <returns>The decrypted file data.</returns>
        internal byte[] GetDecryptedFileData(GamePackFile file)
        {
            // If the time bomb is triggered, be nasty and erase the decryption key from the pack.
            bool isInFuture = false;
            DateTime now = DateTime.UtcNow;
            if (now < ValidFrom || (isInFuture = now > ValidTo))
            {
                if (isInFuture)
                {
                    // Overwrite the current key and IV with a new random one.
                    _cryptoAlgorithm.GenerateKey();
                    _cryptoAlgorithm.GenerateIV();
                    // If the game pack cannot be used anymore, be nasty and remove the MD5 hash.
                    using (FileStream fileStream = new FileStream(FileName, FileMode.Open, FileAccess.Write,
                        FileShare.ReadWrite))
                    {
                        fileStream.Position = 12;
                        fileStream.Write(new byte[16], 0, 16);
                    }
                }
                return new byte[0];
            }
            // The game pack can be used at the moment.
            byte[] decryptedData = new byte[file.Length];
            using (FileStream fileStream = new FileStream(FileName, FileMode.Open, FileAccess.Read,
                FileShare.ReadWrite))
            {
                // Seek to the start of the decrypted file data and decrypt it into the buffer.
                fileStream.Position = file.Offset;
                using (SafeCryptoStream cryptoStream = new SafeCryptoStream(fileStream,
                    _cryptoAlgorithm.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    cryptoStream.Read(decryptedData, 0, decryptedData.Length);
                }
            }
            return decryptedData;
        }
    }
}
