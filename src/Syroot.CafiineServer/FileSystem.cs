using System;
using System.Runtime.InteropServices;

namespace Syroot.CafiineServer
{
    /// <summary>
    /// Wii U structure holding information about a file system entry.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct FSStat
    {
        internal FSStatFlag Flags;
        internal uint Permission;
        internal uint Owner;
        internal uint Group;
        internal uint FileSize;
        internal uint Unk14Nonzero;
        internal uint Unk18Zero;
        internal uint Unk1CZero;
        internal uint EntID;
        internal uint CTimeU;
        internal uint CTimeL;
        internal uint MTimeU;
        internal uint MTimeL;
        internal uint Unk34Zero;
        internal uint Unk38Zero;
        internal uint Unk3CZero;
        internal uint Unk40Zero;
        internal uint Unk44Zero;
        internal uint Unk48Zero;
        internal uint Unk4cZero;
        internal uint Unk50Zero;
        internal uint Unk54Zero;
        internal uint Unk58Zero;
        internal uint Unk5CZero;
        internal uint Unk60Zero;
    }

    /// <summary>
    /// Wii U flags describing which information a file system entry structure contains.
    /// </summary>
    [Flags]
    internal enum FSStatFlag : uint
    {
        None         = 0,
        Unk14Present = 0x01000000,
        MTimePresent = 0x04000000,
        CTimePresent = 0x08000000,
        EntIDPresent = 0x10000000,
        Directory    = 0x80000000,
    }
}
