using Syroot.IO;

namespace Syroot.CafiineServer.IO
{
    /// <summary>
    /// Represents extension methods for the <see cref="BinaryDataWriter"/> class.
    /// </summary>
    internal static class BinaryDataWriterExtensions
    {
        // ---- METHODS (INTERNAL) -------------------------------------------------------------------------------------

        /// <summary>
        /// Writes an <see cref="FSStat"/> to the current stream and advances the current position by the size of the
        /// structure.
        /// </summary>
        /// <param name="writer">The extended <see cref="BinaryDataWriter"/>.</param>
        /// <param name="fsStat">The <see cref="FSStat"/> instance to write.</param>
        /// <returns>The <see cref="FSStat"/> to write the current stream.</returns>
        internal static void Write(this BinaryDataWriter writer, FSStat fsStat)
        {
            writer.Write((uint)fsStat.Flags);
            writer.Write(fsStat.Permission);
            writer.Write(fsStat.Owner);
            writer.Write(fsStat.Group);
            writer.Write(fsStat.FileSize);
            writer.Write(fsStat.Unk14Nonzero);
            writer.Write(fsStat.Unk18Zero);
            writer.Write(fsStat.Unk1CZero);
            writer.Write(fsStat.EntID);
            writer.Write(fsStat.CTimeU);
            writer.Write(fsStat.CTimeL);
            writer.Write(fsStat.MTimeU);
            writer.Write(fsStat.MTimeL);
            writer.Write(fsStat.Unk34Zero);
            writer.Write(fsStat.Unk38Zero);
            writer.Write(fsStat.Unk3CZero);
            writer.Write(fsStat.Unk40Zero);
            writer.Write(fsStat.Unk44Zero);
            writer.Write(fsStat.Unk48Zero);
            writer.Write(fsStat.Unk4cZero);
            writer.Write(fsStat.Unk50Zero);
            writer.Write(fsStat.Unk54Zero);
            writer.Write(fsStat.Unk58Zero);
            writer.Write(fsStat.Unk5CZero);
            writer.Write(fsStat.Unk60Zero);
        }
    }
}
