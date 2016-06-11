using System.IO;
using System.Security.Cryptography;

namespace Syroot.CafiineServer.Pack
{
    /// <summary>
    /// Represents a cryptographic stream not throwing an exception when being disposed due to a .NET bug.
    /// S. https://github.com/dotnet/corefx/issues/7779
    /// </summary>
    internal class SafeCryptoStream : CryptoStream
    {
        // ---- MEMBERS ------------------------------------------------------------------------------------------------

        private readonly Stream _baseStream;

        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="SafeCryptoStream"/> class with the given parameters..
        /// </summary>
        /// <param name="stream">The stream to read from or write to.</param>
        /// <param name="transform">The cryptographic transform to use.</param>
        /// <param name="mode">The mode to use.</param>
        public SafeCryptoStream(Stream stream, ICryptoTransform transform, CryptoStreamMode mode)
            : base(stream, transform, mode)
        {
            _baseStream = stream;
        }

        // ---- METHODS (PROTECTED) ------------------------------------------------------------------------------------

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release
        /// only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            try
            {
                base.Dispose(disposing);
            }
            catch (CryptographicException)
            {
                // An exception is thrown when disposing, but it can be ignored as it is caused by a .NET bug.
                if (disposing)
                {
                    _baseStream.Dispose();
                }
            }
        }
    }
}
