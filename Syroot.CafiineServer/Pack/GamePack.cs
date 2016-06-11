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

        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="GamePack"/> class, loading the given pack file.
        /// </summary>
        /// <param name="fileName">The name of the file containing pack data.</param>
        internal GamePack(string fileName)
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
        /// Gets the main directory of the pack, which should be named after the title ID of the affected game.
        /// </summary>
        internal GamePackDirectory RootDirectory
        {
            get;
            private set;
        }
    }
}
