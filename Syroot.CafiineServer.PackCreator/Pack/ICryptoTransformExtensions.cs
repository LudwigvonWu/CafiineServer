using System.IO;
using System.Security.Cryptography;
using Syroot.CafiineServer.Common.IO;

namespace Syroot.CafiineServer.PackCreator.Pack
{
    /// <summary>
    /// Extension methods for the <see cref="ICryptoTransform"/> interface.
    /// </summary>
    internal static class ICryptoTransformExtensions
    {
        // ---- METHODS (INTERNAL) -------------------------------------------------------------------------------------
        
        /// <summary>
        /// Encrypts the given text and returns the encrypted data.
        /// </summary>
        /// <param name="cryptoTransform">The encryption transformation to use.</param>
        /// <param name="value">The text to encrypt.</param>
        /// <returns>The encrypted text data.</returns>
        internal static byte[] EncryptString(this ICryptoTransform cryptoTransform, string value)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write))
            using (BinaryDataWriter writer = new BinaryDataWriter(cryptoStream))
            {
                writer.Write(value, BinaryStringFormat.ZeroTerminated);
                writer.Flush();
                cryptoStream.FlushFinalBlock();
                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// Encrypts the given file and returns the encrypted data.
        /// </summary>
        /// <param name="cryptoTransform">The encryption transformation to use.</param>
        /// <param name="fileName">The name of the file to encrypt.</param>
        /// <returns>The encrypted file data.</returns>
        internal static byte[] EncryptFile(this ICryptoTransform cryptoTransform, string fileName)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write))
            using (BinaryDataWriter writer = new BinaryDataWriter(cryptoStream))
            {
                writer.Write(File.ReadAllBytes(fileName));
                writer.Flush();
                cryptoStream.FlushFinalBlock();
                return memoryStream.ToArray();
            }
        }
    }
}
