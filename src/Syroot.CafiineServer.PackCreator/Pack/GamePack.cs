using System;
using System.IO;
using System.Security.Cryptography;
using Syroot.IO;

namespace Syroot.CafiineServer.PackCreator.Pack
{
    /// <summary>
    /// Represents an encrypted container file providing game data (creator view).
    /// </summary>
    internal class GamePack
    {
        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        /// <summary>
        /// The extension which has to be used for game packs.
        /// </summary>
        internal const string FileExtension = ".csgp";

        // ---- METHODS (INTERNAL) -------------------------------------------------------------------------------------

        /// <summary>
        /// Stores a <see cref="GamePack"/> under the given file name, encrypting it with the given key and including
        /// all child directories and files in the provided root directory.
        /// </summary>
        /// <param name="fileName">The name of the file to save the game pack under.</param>
        /// <param name="directory">The root directory which contents will be included.</param>
        /// <param name="rootName">The name under which the root directory will be stored.</param>
        /// <param name="validFrom">The date and time at which the package starts to be usable.</param>
        /// <param name="validTo">The date and time at which the package cannot be used anymore.</param>
        internal static void CreateFile(string fileName, string directory, string rootName, DateTime validFrom,
            DateTime validTo)
        {
            fileName = Path.ChangeExtension(fileName, FileExtension);

            // Generate a new AES encryption key and IV to encrypt with.
            Aes cryptoProvider = Aes.Create();
            ICryptoTransform cryptoTransform = cryptoProvider.CreateEncryptor();

            // Create the root directory structure.
            GamePackDirectory root = new GamePackDirectory(cryptoTransform, new DirectoryInfo(directory), rootName);

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
                writer.Position = 12; // Start of the MD5 hash field.
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
                writer.Write(file.Length);
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
