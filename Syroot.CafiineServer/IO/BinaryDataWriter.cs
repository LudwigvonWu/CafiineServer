﻿using System;
using System.IO;
using System.Text;

namespace Syroot.CafiineServer.IO
{
    /// <summary>
    /// Represents an extended <see cref="BinaryWriter"/> supporting special file format data types.
    /// </summary>
    internal class BinaryDataWriter : BinaryWriter
    {
        // ---- MEMBERS ------------------------------------------------------------------------------------------------

        private ByteOrder _byteOrder;
        private bool      _needsReversion;

        // ---- CONSTRUCTORS -------------------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryDataWriter"/> class based on the specified stream and
        /// using UTF-8 encoding.
        /// </summary>
        /// <param name="output">The output stream.</param>
        /// <exception cref="ArgumentException">The stream does not support writing or is already closed.</exception>
        /// <exception cref="ArgumentNullException">output is null.</exception>
        public BinaryDataWriter(Stream output)
            : this(output, new UTF8Encoding(), false)
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryDataWriter"/> class based on the specified stream, UTF-8
        /// encoding and optionally leaves the stream open.
        /// </summary>
        /// <param name="output">The output stream.</param>
        /// <param name="leaveOpen"><c>true</c> to leave the stream open after the <see cref="BinaryDataWriter"/> object
        /// is disposed; otherwise <c>false</c>.</param>
        /// <exception cref="ArgumentException">The stream does not support writing or is already closed.</exception>
        /// <exception cref="ArgumentNullException">output is null.</exception>
        public BinaryDataWriter(Stream output, bool leaveOpen)
            : this(output, new UTF8Encoding(), leaveOpen)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryDataWriter"/> class based on the specified stream and
        /// character encoding.
        /// </summary>
        /// <param name="output">The output stream.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <exception cref="ArgumentException">The stream does not support writing or is already closed.</exception>
        /// <exception cref="ArgumentNullException">output or encoding is null.</exception>
        public BinaryDataWriter(Stream output, Encoding encoding)
            : this(output, encoding, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryDataWriter"/> class based on the specified stream and
        /// character encoding, and optionally leaves the stream open.
        /// </summary>
        /// <param name="output">The output stream.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <param name="leaveOpen"><c>true</c> to leave the stream open after the <see cref="BinaryDataWriter"/> object
        /// is disposed; otherwise <c>false</c>.</param>
        /// <exception cref="ArgumentException">The stream does not support writing or is already closed.</exception>
        /// <exception cref="ArgumentNullException">output or encoding is null.</exception>
        public BinaryDataWriter(Stream output, Encoding encoding, bool leaveOpen)
            : base(output, encoding, leaveOpen)
        {
            Encoding = encoding;
            ByteOrder = ByteOrder.GetSystemByteOrder();
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the byte order used to parse binary data with.
        /// </summary>
        public ByteOrder ByteOrder
        {
            get
            {
                return _byteOrder;
            }
            set
            {
                _byteOrder = value;
                _needsReversion = _byteOrder != ByteOrder.GetSystemByteOrder();
            }
        }

        /// <summary>
        /// Gets the encoding used for string related operations where no other encoding has been provided. Due to the
        /// way the underlying <see cref="BinaryWriter"/> is instantiated, it can only be specified at creation time.
        /// </summary>
        public Encoding Encoding
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the position within the current stream. This is a shortcut to the base stream Position
        /// property.
        /// </summary>
        public long Position
        {
            get { return BaseStream.Position; }
            set { BaseStream.Position = value; }
        }

        // ---- METHODS (PUBLIC) ---------------------------------------------------------------------------------------

        /// <summary>
        /// Sets the position within the current stream. This is a shortcut to the base stream Seek method.
        /// </summary>
        /// <param name="offset">A byte offset relative to the current position in the stream.</param>
        /// <returns>The new position within the current stream.</returns>
        public long Seek(long offset)
        {
            return Seek(offset, SeekOrigin.Current);
        }
        
        /// <summary>
        /// Sets the position within the current stream. This is a shortcut to the base stream Seek method.
        /// </summary>
        /// <param name="offset">A byte offset relative to the origin parameter.</param>
        /// <param name="origin">A value of type <see cref="SeekOrigin"/> indicating the reference point used to obtain
        /// the new position.</param>
        /// <returns>The new position within the current stream.</returns>
        public long Seek(long offset, SeekOrigin origin)
        {
            return BaseStream.Seek(offset, origin);
        }

        /// <summary>
        /// Creates a <see cref="SeekTask"/> with the given parameters. As soon as the returned <see cref="SeekTask"/>
        /// is disposed, the previous stream position will be restored.
        /// </summary>
        /// <param name="offset">A byte offset relative to the current position in the stream.</param>
        /// <returns>A <see cref="SeekTask"/> to be disposed to undo the seek.</returns>
        public SeekTask TemporarySeek(long offset)
        {
            return TemporarySeek(offset, SeekOrigin.Current);
        }

        /// <summary>
        /// Creates a <see cref="SeekTask"/> with the given parameters. As soon as the returned <see cref="SeekTask"/>
        /// is disposed, the previous stream position will be restored.
        /// </summary>
        /// <param name="offset">A byte offset relative to the origin parameter.</param>
        /// <param name="origin">A value of type <see cref="SeekOrigin"/> indicating the reference point used to obtain
        /// the new position.</param>
        /// <returns>A <see cref="SeekTask"/> to be disposed to undo the seek.</returns>
        public SeekTask TemporarySeek(long offset, SeekOrigin origin)
        {
            return new SeekTask(BaseStream, offset, origin);
        }

        /// <summary>
        /// Writes a <see cref="DateTime"/> to this stream. The <see cref="DateTime"/> will be available in the
        /// specified binary format.
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <param name="format">The binary format in which the <see cref="DateTime"/> will be written.</param>
        public void Write(DateTime value, BinaryDateTimeFormat format)
        {
            switch (format)
            {
                case BinaryDateTimeFormat.CTime:
                    Write((uint)(new DateTime(1970, 1, 1) - value.ToLocalTime()).TotalSeconds);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("format", "The specified binary datetime format is invalid.");
            }
        }

        /// <summary>
        /// Writes an 8-byte floating point value to this stream and advances the current position of the stream by
        /// eight bytes.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public override void Write(double value)
        {
            if (_needsReversion)
            {
                byte[] bytes = BitConverter.GetBytes(value);
                WriteReversed(bytes);
            }
            else
            {
                base.Write(value);
            }
        }

        /// <summary>
        /// Writes an 2-byte signed integer to this stream and advances the current position of the stream by two bytes.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public override void Write(short value)
        {
            if (_needsReversion)
            {
                byte[] bytes = BitConverter.GetBytes(value);
                WriteReversed(bytes);
            }
            else
            {
                base.Write(value);
            }
        }

        /// <summary>
        /// Writes an 4-byte signed integer to this stream and advances the current position of the stream by four
        /// bytes.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public override void Write(int value)
        {
            if (_needsReversion)
            {
                byte[] bytes = BitConverter.GetBytes(value);
                WriteReversed(bytes);
            }
            else
            {
                base.Write(value);
            }
        }

        /// <summary>
        /// Writes an 8-byte signed integer to this stream and advances the current position of the stream by eight
        /// bytes.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public override void Write(long value)
        {
            if (_needsReversion)
            {
                byte[] bytes = BitConverter.GetBytes(value);
                WriteReversed(bytes);
            }
            else
            {
                base.Write(value);
            }
        }

        /// <summary>
        /// Writes an 4-byte floating point value to this stream and advances the current position of the stream by four
        /// bytes.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public override void Write(float value)
        {
        }
        
        /// <summary>
        /// Writes a string to this stream in the current encoding of the <see cref="BinaryDataWriter"/> and advances
        /// the current position of the stream in accordance with the encoding used and the specific characters being
        /// written to the stream. The string will be available in the specified binary format.
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <param name="format">The binary format in which the string will be written.</param>
        public void Write(string value, BinaryStringFormat format)
        {
            Write(value, format, Encoding);
        }

        /// <summary>
        /// Writes a string to this stream with the given encoding and advances the current position of the stream in
        /// accordance with the encoding used and the specific characters being written to the stream. The string will
        /// be available in the specified binary format.
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <param name="format">The binary format in which the string will be written.</param>
        /// <param name="encoding">The encoding used for converting the string.</param>
        public void Write(string value, BinaryStringFormat format, Encoding encoding)
        {
            switch (format)
            {
                case BinaryStringFormat.ByteLengthPrefix:
                    WriteByteLengthPrefixString(value, encoding);
                    break;
                case BinaryStringFormat.WordLengthPrefix:
                    WriteWordLengthPrefixString(value, encoding);
                    break;
                case BinaryStringFormat.DwordLengthPrefix:
                    WriteDwordLengthPrefixString(value, encoding);
                    break;
                case BinaryStringFormat.ZeroTerminated:
                    WriteZeroTerminatedString(value, encoding);
                    break;
                case BinaryStringFormat.NoPrefixOrTermination:
                    WriteNoPrefixOrTerminationString(value, encoding);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("format", "The specified binary string format is invalid");
            }
        }

        /// <summary>
        /// Writes an 2-byte unsigned integer value to this stream and advances the current position of the stream by
        /// two bytes.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public override void Write(ushort value)
        {
            if (_needsReversion)
            {
                byte[] bytes = BitConverter.GetBytes(value);
                WriteReversed(bytes);
            }
            else
            {
                base.Write(value);
            }
        }

        /// <summary>
        /// Writes an 4-byte unsigned integer value to this stream and advances the current position of the stream by
        /// four bytes.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public override void Write(uint value)
        {
            if (_needsReversion)
            {
                byte[] bytes = BitConverter.GetBytes(value);
                WriteReversed(bytes);
            }
            else
            {
                base.Write(value);
            }
        }

        /// <summary>
        /// Writes an 8-byte unsigned integer value to this stream and advances the current position of the stream by
        /// eight bytes.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public override void Write(ulong value)
        {
            if (_needsReversion)
            {
                byte[] bytes = BitConverter.GetBytes(value);
                WriteReversed(bytes);
            }
            else
            {
                base.Write(value);
            }
        }
        
        // ---- METHODS (PRIVATE) --------------------------------------------------------------------------------------

        private void WriteReversed(byte[] bytes)
        {
            Array.Reverse(bytes);
            base.Write(bytes);
        }

        private void WriteByteLengthPrefixString(string value, Encoding encoding)
        {
            Write((byte)value.Length);
            Write(encoding.GetBytes(value));
        }

        private void WriteWordLengthPrefixString(string value, Encoding encoding)
        {
            Write((short)value.Length);
            Write(encoding.GetBytes(value));
        }

        private void WriteDwordLengthPrefixString(string value, Encoding encoding)
        {
            Write(value.Length);
            Write(encoding.GetBytes(value));
        }

        private void WriteZeroTerminatedString(string value, Encoding encoding)
        {
            Write(encoding.GetBytes(value));
            Write((byte)0);
        }

        private void WriteNoPrefixOrTerminationString(string value, Encoding encoding)
        {
            Write(encoding.GetBytes(value));
        }
    }
}
