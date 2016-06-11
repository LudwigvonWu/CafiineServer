using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Syroot.CafiineServer.Common.IO;

namespace Syroot.CafiineServer.Common
{
    /// <summary>
    /// Represents an encrypted container file providing game data.
    /// </summary>
    public class GamePack
    {
        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="GamePack"/> class, loading the given pack file.
        /// </summary>
        /// <param name="fileName">The name of the file containing pack data.</param>
        public GamePack(string fileName)
        {
            using (FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
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
                AesCryptoServiceProvider cryptoProvider = new AesCryptoServiceProvider();
                cryptoProvider.Key = reader.ReadBytes(reader.ReadByte());
                cryptoProvider.IV = reader.ReadBytes(reader.ReadByte());

                // Read in the directory and file headers.
                RootDirectory = new GamePackDirectory(cryptoProvider, reader);
            }
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets the time and date from which this pack can be used.
        /// </summary>
        public DateTime ValidFrom
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the time and date until which this pack can be used.
        /// </summary>
        public DateTime ValidTo
        {
            get;
            private set;
        }

        public GamePackDirectory RootDirectory
        {
            get;
            private set;
        }

        // ---- METHODS (PUBLIC) ---------------------------------------------------------------------------------------

        /// <summary>
        /// Stores a <see cref="GamePack"/> under the given file name, encrypting it with the given key and including
        /// all child directories and files in the provided root directory.
        /// </summary>
        /// <param name="fileName">The name of the file to save the game pack under.</param>
        /// <param name="key">The key to encrypt the data with.</param>
        /// <param name="directory">The root directory which contents will be included.</param>
        /// <param name="validFrom">The date and time at which the package starts to be usable.</param>
        /// <param name="validTo">The date and time at which the package cannot be used anymore.</param>
        public static void Create(string fileName, string directory, DateTime validFrom, DateTime validTo)
        {
            // Generate a new AES encryption key and IV to encrypt with.
            AesCryptoServiceProvider cryptoProvider = new AesCryptoServiceProvider();
            ICryptoTransform cryptoTransform = cryptoProvider.CreateEncryptor();

            // Create the root directory structure.
            GamePackDirectory root = new GamePackDirectory(cryptoTransform, new DirectoryInfo(directory));

            // Use the size of the headers to write the structures to the file with the correct file data offsets.
            using (FileStream stream = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            using (BinaryDataWriter writer = new BinaryDataWriter(stream))
            {
                // Write the file header (46 bytes).
                writer.Write("CSGP", BinaryStringFormat.NoPrefixOrTermination); // Magic bytes.
                writer.Write((long)0); // Flags (unused yet).
                writer.Write(new byte[16]); // Space for MD5 hash.
                // Write time bomb dates.
                writer.Write(validFrom.ToUniversalTime(), BinaryDateTimeFormat.NetTicks);
                writer.Write(validTo.ToUniversalTime(), BinaryDateTimeFormat.NetTicks);
                // Write key decryption information.
                writer.Write((byte)cryptoProvider.Key.Length);
                writer.Write(cryptoProvider.Key);
                writer.Write((byte)cryptoProvider.IV.Length);
                writer.Write(cryptoProvider.IV);

                // Get the initial offset for files, which starts after all the directory and file headers.
                long fileOffset = 46 + cryptoProvider.Key.Length + cryptoProvider.IV.Length
                    + GetHeaderSizesRecursive(root);
                WriteDirectory(cryptoTransform, writer, root, ref fileOffset);

                // Seek back, generate the MD5 hash and store it.
                byte[] md5Hash;
                using (MD5 md5 = MD5.Create())
                {
                    stream.Position = 28; // Right behind the MD5 hash field.
                    md5Hash = md5.ComputeHash(stream);
                }
                writer.Position = 12;
                writer.Write(md5Hash);
            }
        }

        // ---- METHODS (PRIVATE) --------------------------------------------------------------------------------------
        
        private static long GetHeaderSizesRecursive(GamePackDirectory directory)
        {
            long size = 2 + directory.EncryptedName.Length + 4 + 4; // Name and file / directory count
            foreach (GamePackFile file in directory.Files)
            {
                size += 2 + file.EncryptedName.Length + 4 + 8 + 4; // Name, size, offset, encrypted size
            }
            foreach (GamePackDirectory subDirectory in directory.Directories)
            {
                size += GetHeaderSizesRecursive(subDirectory);
            }
            return size;
        }
        
        private static void WriteDirectory(ICryptoTransform cryptoTransform, BinaryDataWriter writer,
            GamePackDirectory directory, ref long fileOffset)
        {
            // Write the directory information.
            writer.Write((short)directory.EncryptedName.Length);
            writer.Write(directory.EncryptedName);

            // Write the file headers of the directory.
            writer.Write(directory.Files.Count);
            foreach (GamePackFile file in directory.Files)
            {
                // Write the name and size of the file.
                writer.Write((short)file.EncryptedName.Length);
                writer.Write(file.EncryptedName);
                writer.Write(file.Size);
                // Write the encrypted data at the corresponding offset.
                writer.Write(fileOffset);
                byte[] encryptedFileData = cryptoTransform.EncryptFile(file.FullPath);
                writer.Write(encryptedFileData.Length);
                using (writer.TemporarySeek(fileOffset, SeekOrigin.Begin))
                {
                    writer.Write(encryptedFileData);
                }
                fileOffset += encryptedFileData.Length;
            }

            // Write the child directories.
            writer.Write(directory.Directories.Count);
            foreach (GamePackDirectory subDirectory in directory.Directories)
            {
                WriteDirectory(cryptoTransform, writer, subDirectory, ref fileOffset);
            }
        }
    }
}
