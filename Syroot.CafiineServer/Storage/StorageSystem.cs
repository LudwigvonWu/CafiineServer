using System.IO;

namespace Syroot.CafiineServer.Storage
{
    /// <summary>
    /// Represents a unified access to data stored raw on the file system or inside game packs.
    /// </summary>
    internal class StorageSystem
    {
        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        /// <summary>
        /// The seperator used to delimit directories and files.
        /// </summary>
        internal const char Separator = '/';

        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageSystem"/> class from the given root directory.
        /// </summary>
        /// <param name="rootDirectory">The directory to represent.</param>
        internal StorageSystem(string rootDirectory)
        {
            // Ensure the root directory exists.
            Directory.CreateDirectory(rootDirectory);

            Root = new RootStorageDirectory(new DirectoryInfo(rootDirectory));
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------
        
        /// <summary>
        /// Gets the root directory of the storage system which contains all available game data.
        /// </summary>
        internal RootStorageDirectory Root
        {
            get;
            private set;
        }

        // ---- METHODS (INTERNAL) -------------------------------------------------------------------------------------

        /// <summary>
        /// Returns whether the directory exists at the given path relative to the root.
        /// </summary>
        /// <param name="path">The path relative to the root.</param>
        /// <returns><c>true</c> when the directory exists.</returns>
        internal bool DirectoryExists(string path)
        {
            return GetDirectory(path, Root) != null;
        }
        
        /// <summary>
        /// Returns the <see cref="StorageDirectory"/> at the given path relative to the root.
        /// </summary>
        /// <param name="path">The path relative to the root.</param>
        /// <returns>The <see cref="StorageDirectory"/> when it exists or <c>null</c>.</returns>
        internal StorageDirectory GetDirectory(string path)
        {
            return GetDirectory(path, Root);
        }

        /// <summary>
        /// Returns the <see cref="StorageFile"/> at the given path relative to the root.
        /// </summary>
        /// <param name="path">The path relative to the root.</param>
        /// <returns>The <see cref="StorageFile"/> when it exists or <c>null</c>.</returns>
        internal StorageFile GetFile(string path)
        {
            return GetFile(path, Root);
        }

        // ---- METHODS (PRIVATE) --------------------------------------------------------------------------------------

        private StorageDirectory GetDirectory(string path, StorageDirectory directory)
        {
            // Get the name of the left-most directory in the path.
            path = path.TrimStart(Separator);
            int separatorIndex = path.IndexOf(Separator);
            bool isLastDirectory = separatorIndex == -1;
            string directoryName = isLastDirectory ? path : path.Substring(0, separatorIndex);

            // Check if this directory exists in the given or child ones.
            foreach (StorageDirectory subDirectory in directory.GetDirectories())
            {
                if (subDirectory.Name == directoryName)
                {
                    if (isLastDirectory)
                    {
                        return subDirectory;
                    }
                    else
                    {
                        StorageDirectory storageDirectory = GetDirectory(path.Substring(separatorIndex + 1),
                            subDirectory);
                        // Check other paths (like in packs) if it could not be found here.
                        // TODO: Not the most performant solution. Merge the file systems instead.
                        if (storageDirectory != null)
                        {
                            return storageDirectory;
                        }
                    }
                }
            }
            return null;
        }
        
        private StorageFile GetFile(string path, StorageDirectory directory)
        {
            // Get the name of the left-most directory or file in the path.
            path = path.TrimStart(Separator);
            int separatorIndex = path.IndexOf(Separator);
            bool isFileName = separatorIndex == -1;
            string name = isFileName ? path : path.Substring(0, separatorIndex);

            if (isFileName)
            {
                // Try to find the file in the final directory.
                foreach (StorageFile file in directory.GetFiles())
                {
                    if (file.Name == name)
                    {
                        return file;
                    }
                }
            }
            else
            {
                // Try to find the current directory in the path.
                foreach (StorageDirectory subDirectory in directory.GetDirectories())
                {
                    if (subDirectory.Name == name)
                    {
                        StorageFile storageFile = GetFile(path.Substring(separatorIndex + 1), subDirectory);
                        // Check other paths (like in packs) if it could not be found here.
                        // TODO: Not the most performant solution. Merge the file systems instead.
                        if (storageFile != null)
                        {
                            return storageFile;
                        }
                    }
                }
            }
            return null;
        }
    }
}
