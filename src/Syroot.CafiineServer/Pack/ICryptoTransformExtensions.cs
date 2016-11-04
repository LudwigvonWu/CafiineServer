using System.IO;
using System.Security.Cryptography;
using Syroot.IO;

namespace Syroot.CafiineServer.Pack
{
    /// <summary>
    /// Extension methods for the <see cref="ICryptoTransform"/> interface.
    /// </summary>
    internal static class ICryptoTransformExtensions
    {
        // ---- METHODS (INTERNAL) -------------------------------------------------------------------------------------

        /// <summary>
        /// Decrypts text with from the given encrypted data.
        /// </summary>
        /// <param name="cryptoTransform">The encryption transformation to use.</param>
        /// <param name="value">The data to decrypt.</param>
        /// <returns>The decrypted text.</returns>
        internal static string DecryptString(this ICryptoTransform cryptoTransform, byte[] value)
        {
            using (MemoryStream memoryStream = new MemoryStream(value))
            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Read))
            using (BinaryDataReader reader = new BinaryDataReader(cryptoStream))
            {
                return reader.ReadString(BinaryStringFormat.ZeroTerminated);
            }
        }
    }
}
