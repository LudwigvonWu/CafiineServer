using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Syroot.CafiineServer.Common.IO;
using Syroot.CafiineServer.IO;
using Syroot.CafiineServer.Storage;

namespace Syroot.CafiineServer
{
    /// <summary>
    /// Represents a connection with a Cafiine client and handles communication with it.
    /// </summary>
    internal class Client
    {
        // ---- MEMBERS ------------------------------------------------------------------------------------------------

        private static int _clientCounter;

        private Server    _server;
        private TcpClient _tcpClient;
        private int       _clientID;
        
        private BinaryDataReader            _reader;
        private BinaryDataWriter            _writer;
        private uint[]                      _titleIDParts;
        private string                      _titleID;
        private Stream[]                    _fileStreams;
        private Dictionary<int, FileStream> _dumpStreams;

        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="Client"/> class, accepted from the given <see cref="Server"/>
        /// with the provided <see cref="TcpClient"/>.
        /// </summary>
        /// <param name="server">The server which accepted the Cafiine client.</param>
        /// <param name="tcpClient">The TCP client representing the Cafiine client.</param>
        internal Client(Server server, TcpClient tcpClient)
        {
            _server = server;
            _tcpClient = tcpClient;
            _clientCounter++;
            _clientID = _clientCounter;

            _fileStreams = new Stream[256];
            _dumpStreams = new Dictionary<int, FileStream>();
        }
        
        // ---- METHODS (INTERNAL) -------------------------------------------------------------------------------------

        /// <summary>
        /// Starts handling the connection with the Cafiine client in a new thread.
        /// </summary>
        internal void HandleAsync()
        {
            // Start a new thread to handle the client on.
            Thread thread = new Thread(HandleThread);
            thread.Start();
        }

        // ---- METHODS (PRIVATE) --------------------------------------------------------------------------------------
        
        private void HandleThread()
        {
            try
            {
                // Open a reader and writer on the client network stream.
                using (NetworkStream stream = _tcpClient.GetStream())
                {
                    _reader = new BinaryDataReader(stream);
                    _reader.ByteOrder = ByteOrder.BigEndian;
                    _writer = new BinaryDataWriter(stream);
                    _writer.ByteOrder = ByteOrder.BigEndian;

                    // Get the requested title ID.
                    _titleIDParts = _reader.ReadUInt32s(4);
                    _titleID = String.Format("{0:X8}-{1:X8}", _titleIDParts[0], _titleIDParts[1]);
                    Log(ConsoleColor.White, "Client connected (endpoint={0}, title={1})", _tcpClient.Client.RemoteEndPoint,
                        _titleID);

                    // Check if any game data is available for this title.
                    if (!_server.Storage.DirectoryExists(_titleID))
                    {
                        Log(ConsoleColor.Gray, "> No data available for title {0}.", _titleID);
                        _writer.Write((byte)ClientCommand.Normal);
                        return;
                    }
                    // Send back that we have data for this title.  
                    Log(ConsoleColor.White, "> Data found for title {0}.", _titleID);
                    _writer.Write((byte)ClientCommand.Special);

                    // Repeatedly wait for commands sent by the client to handle them.
                    while (true)
                    {
                        ClientCommand command = (ClientCommand)_reader.ReadByte();
                        switch (command)
                        {
                            case ClientCommand.Open:       HandleCommandOpen();       break;
                            case ClientCommand.DumpCreate: HandleCommandDumpCreate(); break;
                            case ClientCommand.Dump:       HandleCommandDump();       break;
                            case ClientCommand.Read:       HandleCommandRead();       break;
                            case ClientCommand.Close:      HandleCommandClose();      break;
                            case ClientCommand.SetPos:     HandleCommandSetPos();     break;
                            case ClientCommand.StatFile:   HandleCommandStatFile();   break;
                            case ClientCommand.Eof:        HandleCommandEof();        break;
                            case ClientCommand.GetPos:     HandleCommandGetPos();     break;
                            case ClientCommand.Ping:       HandleCommandPing();       break;
                            default: throw new InvalidDataException("Unknown Cafiine command received.");
                        }
                    }
                }
            }
            catch (IOException)
            {
                Log(ConsoleColor.Red, "Communication issue, stream probably ended or client shut down.");
            }
            catch (Exception ex)
            {
                Log(ConsoleColor.Red, "Unexpected error ({0})", ex.Message);
            }
            finally
            {
                // Close all file handles.
                foreach (FileStream fileStream in _fileStreams)
                {
                    if (fileStream != null) fileStream.Dispose();
                }
                // Close all file dump handles.
                foreach (FileStream dumpStream in _dumpStreams.Values)
                {
                    if (dumpStream != null) dumpStream.Dispose();
                }
            }
            Log(ConsoleColor.DarkRed, "Connection dismissed.");
        }

        private void HandleCommandOpen()
        {
            // Read the message parameters.
            int pathLength = _reader.ReadInt32();
            int modeLength = _reader.ReadInt32();
            string path = _reader.ReadString(BinaryStringFormat.ZeroTerminated, Encoding.ASCII);
            string mode = _reader.ReadString(BinaryStringFormat.ZeroTerminated, Encoding.ASCII);

            // Get the server path to the requested file.
            string fullPath = GetServerPath(path);
            Log(ConsoleColor.Cyan, "Querying '{0}' (mode={1})", fullPath, mode.ToUpper());
            bool requestSlow = false;
            StorageFile file = _server.Storage.GetFile(_titleID + path);
            if (file != null)
            {
                // We have a replacement file, find and send back a new virtual file handle.
                int handle = -1;
                for (int i = 0; i < _fileStreams.Length; i++)
                {
                    if (_fileStreams[i] == null)
                    {
                        handle = i;
                        break;
                    }
                }
                // If no free handle could be found, we cannot handle this request.
                if (handle == -1)
                {
                    Log(ConsoleColor.Red, "> Cannot handle query, no more free file handles.");
                    _writer.Write((byte)ClientCommand.Special);
                    _writer.Write(-19);
                    _writer.Write(0);
                    return;
                }
                // Open a new file stream on the replacement file under this handle.
                if (file.GetType() == typeof(RawStorageFile))
                {
                    Log(ConsoleColor.Green, "> Replacing '{0}' (mode={1}, handle={2})", path, mode.ToUpper(), handle);
                }
                _fileStreams[handle] = file.GetStream();
                // Send back that we have a replacement file with the found handle.
                _writer.Write((byte)ClientCommand.Special);
                _writer.Write(0);
                _writer.Write(0x0FFF00FF | (handle << 8));
            }
            else if (!File.Exists(fullPath + "-dump")
                && (File.Exists(fullPath + "-request") || (requestSlow = File.Exists(fullPath + "-request_slow"))))
            {
                // We do not have a replacement file, but a first dump is requested for it. Tell Cafiine to send it.
                Log(ConsoleColor.Magenta, "> Requesting dump of '{0}' (slow={1})", path, requestSlow);
                _writer.Write(requestSlow ? (byte)ClientCommand.RequestSlow : (byte)ClientCommand.Request);
            }
            else
            {
                // No file was found and no dump was requested (or was already done).
                _writer.Write((byte)ClientCommand.Normal);
            }
        }

        private void HandleCommandDumpCreate()
        {
            // Read message parameters.
            int fileDescriptor = _reader.ReadInt32();
            int pathLength = _reader.ReadInt32();
            string path = _reader.ReadString(BinaryStringFormat.ZeroTerminated, Encoding.ASCII);
            
            // Create a new file to save the incoming dumped data in.
            _dumpStreams.Add(fileDescriptor, new FileStream(GetServerPath(path) + "-dump", FileMode.Create,
                FileAccess.Write, FileShare.Read));

            // Tell Cafiine that the file was created and we are now waiting to retrieve the file data.
            _writer.Write((byte)ClientCommand.Special);
        }

        private void HandleCommandDump()
        {
            // Read the message parameters.
            int fileDescriptor = _reader.ReadInt32();
            int size = _reader.ReadInt32();
            byte[] fileData = _reader.ReadBytes(size);

            // Get the stream writing the dumped data to the corresponding file.
            FileStream dumpStream;
            if (_dumpStreams.TryGetValue(fileDescriptor, out dumpStream))
            {
                // Write the received file data into the file stream.
                Log(ConsoleColor.Magenta, "Dumping '{0}' ({1} kB)", Path.GetFileName(dumpStream.Name), size / 1024);
                dumpStream.Write(fileData, 0, size);
            }

            // Tell Cafiine that dumping can continue.
            _writer.Write((byte)ClientCommand.Special);
        }

        private void HandleCommandRead()
        {
            // Read the message parameters.
            int size = _reader.ReadInt32();
            int count = _reader.ReadInt32();
            int fileDescriptor = _reader.ReadInt32();

            if ((fileDescriptor & 0x0FFF00FF) == 0x0FFF00FF)
            {
                // Get the file stream to read from.
                int handle = (fileDescriptor >> 8) & 0xFF;
                Stream fileStream = _fileStreams[handle];
                if (fileStream == null)
                {
                    // The file could not be read because it was not opened before.
                    Log(ConsoleColor.Red, "Cannot read file because it is not open (handle={0})", handle);
                    _writer.Write((byte)ClientCommand.Special);
                    _writer.Write(-19);
                    _writer.Write(0);
                }
                else
                {
                    // Read in the file data and send it to Cafiine.
                    byte[] buffer = new byte[size * count];
                    int readBytes = fileStream.Read(buffer, 0, buffer.Length);
                    _writer.Write((byte)ClientCommand.Special);
                    _writer.Write(readBytes / size);
                    _writer.Write(readBytes);
                    _writer.Write(buffer, 0, readBytes);
                    // Check if the sent data could be accepted.
                    if (_reader.ReadByte() != (byte)ClientCommand.OK)
                    {
                        throw new InvalidDataException("Cafiine could not accept sent file data.");
                    }
                }
            }
            else
            {
                _writer.Write((byte)ClientCommand.Normal);
            }
        }

        private void HandleCommandClose()
        {
            // Read the message parameters.
            int fileDescriptor = _reader.ReadInt32();

            if ((fileDescriptor & 0x0FFF00FF) == 0x0FFF00FF)
            {
                // Get the stream of the file to be closed.
                int handle = (fileDescriptor >> 8) & 0xFF;
                Stream fileStream = _fileStreams[handle];
                if (fileStream == null)
                {
                    // The file could not be closed because it was never open.
                    Log(ConsoleColor.Red, "Cannot close file because it was never open (handle={0})", handle);
                    _writer.Write((byte)ClientCommand.Special);
                    _writer.Write(-38);
                }
                else
                {
                    // Close the requested file and clear the slot in the file handle array.
                    Log(ConsoleColor.Gray, "Closing file (handle={0})", handle);
                    fileStream.Dispose();
                    _fileStreams[handle] = null;
                    // Send a response that closing the file was successful.
                    _writer.Write((byte)ClientCommand.Special);
                    _writer.Write(0);
                }
            }
            else
            {
                // Check if the closed file was requested to be dumped.
                FileStream fileStream;
                if (_dumpStreams.TryGetValue(fileDescriptor, out fileStream))
                {
                    // Close the stream for dumping the file and remove it from the request list.
                    fileStream.Dispose();
                    _dumpStreams.Remove(fileDescriptor);
                    Log(ConsoleColor.Magenta, "Completed dumping '{0}'", Path.GetFileName(fileStream.Name));
                }
                // Send a response that we could finish the dump successfully.
                _writer.Write((byte)ClientCommand.Normal);
            }
        }

        private void HandleCommandSetPos()
        {
            // Read the message parameters.
            int fileDescriptor = _reader.ReadInt32();
            int position = _reader.ReadInt32();

            if ((fileDescriptor & 0x0FFF00FF) == 0x0FFF00FF)
            {
                // Get the stream of the file to seek in.
                int handle = (fileDescriptor >> 8) & 0xFF;
                Stream fileStream = _fileStreams[handle];
                if (fileStream == null)
                {
                    // The file could not be seeked in because it is not open.
                    Log(ConsoleColor.Red, "Cannot seek in file because it is not open (handle={0})", handle);
                    _writer.Write((byte)ClientCommand.Special);
                    _writer.Write(-38);
                }
                else
                {
                    // Set the position.
                    fileStream.Position = position;
                    // Send a response that we could seek successfully.
                    _writer.Write((byte)ClientCommand.Special);
                    _writer.Write(0);
                }
            }
            else
            {
                _writer.Write((byte)ClientCommand.Normal);
            }
        }

        private void HandleCommandStatFile()
        {
            // Read the message parameters.
            int fileDescriptor = _reader.ReadInt32();

            if ((fileDescriptor & 0x0FFF00FF) == 0x0FFF00FF)
            {
                // Get the stream of the file which information is requested.
                int handle = (fileDescriptor >> 8) & 0xFF;
                Stream fileStream = _fileStreams[handle];
                if (fileStream == null)
                {
                    // The information could not be retrieved because the file is not open.
                    Log(ConsoleColor.Red, "Cannot retrieve file info because it is not open (handle={0})", handle);
                    _writer.Write((byte)ClientCommand.Special);
                    _writer.Write(-38);
                    _writer.Write(0);
                }
                else
                {
                    // Create the file information structure.
                    FSStat fileStats = new FSStat();
                    fileStats.Flags = FSStatFlag.None;
                    fileStats.Permission = 0x400;
                    fileStats.Owner = _titleIDParts[1];
                    fileStats.Group = 0x101E;
                    fileStats.FileSize = (uint)fileStream.Length;
                    // Send the file information to Cafiine.
                    _writer.Write((byte)ClientCommand.Special);
                    _writer.Write(0);
                    _writer.Write(Marshal.SizeOf(fileStats));
                    _writer.Write(fileStats);
                }
            }
            else
            {
                _writer.Write((byte)ClientCommand.Normal);
            }
        }

        private void HandleCommandEof()
        {
            // Read the message parameters.
            int fileDescriptor = _reader.ReadInt32();

            if ((fileDescriptor & 0x0FFF00FF) == 0x0FFF00FF)
            {
                // Get the stream of the file which position is queried to be at the end of the file.
                int handle = (fileDescriptor >> 8) & 0xFF;
                Stream fileStream = _fileStreams[handle];
                if (fileStream == null)
                {
                    // The information cannot be retrieved as the file is not open.
                    Log(ConsoleColor.Red, "Cannot retrieve EOF file info because it is not open (handle={0})", handle);
                    _writer.Write((byte)ClientCommand.Special);
                    _writer.Write(-38);
                }
                else
                {
                    // Respond to Cafiine whether the stream reached the end of the file (-5) or not (0).
                    _writer.Write((byte)ClientCommand.Special);
                    _writer.Write(fileStream.Position == fileStream.Length ? -5 : 0);
                }
            }
            else
            {
                _writer.Write((byte)ClientCommand.Normal);
            }
        }

        private void HandleCommandGetPos()
        {
            // Read the message parameters.
            int fileDescriptor = _reader.ReadInt32();

            if ((fileDescriptor & 0x0FFF00FF) == 0x0FFF00FF)
            {
                // Get the stream of the file which position is queried.
                int handle = (fileDescriptor >> 8) & 0xFF;
                Stream fileStream = _fileStreams[handle];
                if (fileStream == null)
                {
                    // The position cannot be retrieved as the file is not open.
                    Log(ConsoleColor.Red, "Cannot retrieve file position because it is not open (handle={0})", handle);
                    _writer.Write((byte)ClientCommand.Special);
                    _writer.Write(-38);
                    _writer.Write(0);
                }
                else
                {
                    // Return the file stream position back to Cafiine.
                    _writer.Write((byte)ClientCommand.Special);
                    _writer.Write(0);
                    _writer.Write((int)fileStream.Position);
                }
            }
            else
            {
                _writer.Write((byte)ClientCommand.Normal);
            }
        }

        private void HandleCommandPing()
        {
            // Read the message parameters.
            int value1 = _reader.ReadInt32();
            int value2 = _reader.ReadInt32();

            Log(ConsoleColor.Gray, "Pinged (value1={0}, value2={1})");
        }

        private string GetServerPath(string queriedPath)
        {
            queriedPath = queriedPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            return Path.Combine(_server.RootDirectory, _titleID, queriedPath);
        }

        private void Log(ConsoleColor color, string format, params object[] args)
        {
            Console.ForegroundColor = color;
            Console.WriteLine("[" + _clientID.ToString() + "] " + format, args);
        }

        // ---- ENUMERATIONS -------------------------------------------------------------------------------------------
        
        private enum ClientCommand : byte
        {
            Open        = 0x00,
            Read        = 0x01,
            Close       = 0x02,
            OK          = 0x03,
            SetPos      = 0x04,
            StatFile    = 0x05,
            Eof         = 0x06,
            GetPos      = 0x07,
            Request     = 0x08,
            RequestSlow = 0x09,
            DumpCreate  = 0x0A,
            Dump        = 0x0B,
            Ping        = 0x0C,
            Special     = 0xFE,
            Normal      = 0xFF
        }
    }
}
