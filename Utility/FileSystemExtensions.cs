#nullable enable

using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace HCL_ODA_TestPAD.Utility
{
    public static class FileSystemExtensions
    {
        /// <summary>
        ///     Copy content of the <paramref name="sourcePath"/> directory to the <paramref name="destinationPath"/> directory,
        ///     including subdirectories and files.
        /// </summary>
        /// <param name="sourcePath">The source directory.</param>
        /// <param name="destinationPath">The destination directory.</param>
        public static void CopyDirectoryContents(this string sourcePath, string destinationPath)
        {
            if (!Directory.Exists(sourcePath))
            {
                return;
            }

            var srcDirInfo = new DirectoryInfo(sourcePath);

            Directory.CreateDirectory(destinationPath);

            foreach (var srcFile in srcDirInfo.GetFiles())
            {
                var fileDstPath = Path.Combine(destinationPath, srcFile.Name);
                File.Copy(srcFile.FullName, fileDstPath);
            }

            foreach (var srcSubDir in srcDirInfo.GetDirectories())
            {
                var subDirDstPath = Path.Combine(destinationPath, srcSubDir.Name);
                CopyDirectoryContents(srcSubDir.FullName, subDirDstPath);
            }
        }

        /// <summary>
        ///     Copy file at <paramref name="sourcePath"/> to <paramref name="destinationPath"/>.
        /// </summary>
        /// <param name="sourcePath">Source path.</param>
        /// <param name="destinationPath">Destination path.</param>
        public static void CopyFile(this string sourcePath, string destinationPath)
        {
            if (File.Exists(sourcePath))
            {
                File.Copy(sourcePath, destinationPath);
            }
        }

        /// <summary>
        ///     Ensures that the directory exists. If it does not exist, it will be created.
        /// </summary>
        /// <param name="path">Path to the directory.</param>
        public static void EnsureDirectoryExists(this string path)
        {
            ArgumentNullException.ThrowIfNull(path);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        ///     Delete directory if it exists.
        /// </summary>
        /// <param name="path">Path to the directory.</param>
        public static void DeleteDirectory(this string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }

        /// <summary>
        ///     Delete file if it exists.
        /// </summary>
        /// <param name="path"></param>
        public static void DeleteFile(this string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        /// <summary>
        ///     Initialize file with contents.
        /// </summary>
        /// <param name="path">Path to the file.</param>
        /// <param name="contents">Initial content.</param>
        public static void InitializeFile(this string path, string contents)
        {
            if (!File.Exists(path) || File.ReadAllText(path) != contents)
            {
                File.WriteAllText(path, contents);
            }
        }

        /// <summary>
        ///     Remove oldest files in directory and keep the latest <paramref name="filesToKeep"/> files.
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <param name="filesToKeep"></param>
        public static void RemoveOldestFilesInDir(this string directoryPath, int filesToKeep)
        {
            if (!Directory.Exists(directoryPath))
            {
                return;
            }

            var directory = new DirectoryInfo(directoryPath);
            var filesToRemove = directory
                .GetFiles()
                .OrderByDescending(f => f.CreationTime)
                .Skip(filesToKeep);

            foreach (var file in filesToRemove)
            {
                file.Delete();
            }
        }

        /// <summary>
        /// Check if file is locked. Throws an <see cref="FileNotFoundException"/> is file does not exist.
        /// </summary>
        /// <param name="filePath">Path to file.</param>
        /// <returns><c>True</c> if file is locked, <c>false</c> otherwise.</returns>
        public static bool IsWriteLocked(this string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(filePath);
            }

            try
            {
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    stream.Close();
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch
#pragma warning restore CA1031 // Do not catch general exception types
            {
                return true;
            }

            return false;
        }

        private static readonly object s_uniqueLock = new();

        /// <summary>
        /// Create unique file path if file already exists.
        /// </summary>
        /// <param name="filePath">Base file path.</param>
        /// <param name="createFile">Create empty file for new path to create a lock.</param>
        /// <param name="separator">Optional separator between file name and running number. </param>
        /// <returns>Unique file path.</returns>
        public static string MakeFilePathUnique(this string filePath, bool createFile = false, string separator = " ")
        {
            lock (s_uniqueLock)
            {
                var dir = Path.GetDirectoryName(filePath);
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                var fileExt = Path.GetExtension(filePath);

                for (var iRun = 1; ; ++iRun)
                {
                    if (!File.Exists(filePath))
                    {
                        break;
                    }

                    filePath = Path.Combine(dir ?? string.Empty, fileName + separator + iRun + fileExt);
                }

                if (createFile)
                {
                    using (var stream = File.Create(filePath))
                    {
                        stream.Close();
                    }
                }

                return filePath;
            }
        }

        public static bool CanCreateFile(this string filePath)
        {
            bool canCreate;
            try
            {
                using (File.Create(filePath)) { }
                File.Delete(filePath);
                canCreate = true;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch
#pragma warning restore CA1031 // Do not catch general exception types
            {
                canCreate = false;
            }

            return canCreate;
        }

        /// <summary>
        ///     Remove all invalid path characters.
        /// </summary>
        public static string ToValidPath(this string str, string replacement = "")
        {
            var illegalCharacters = new string(Path.GetInvalidFileNameChars().Concat(Path.GetInvalidPathChars()).Distinct().ToArray());
            var regex = new Regex($"[{Regex.Escape(illegalCharacters)}]");
            return regex.Replace(str, replacement);
        }

        public static bool TryDeleteFile(this string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            try
            {
                File.Delete(path);

                return true;
            }
            catch (IOException)
            {
                return false;
            }
        }

        /// <summary>
        ///     Get the available free space in bytes for drive of given directory path.    
        /// </summary>
        /// <returns>Available space in bytes if drive for path exists, else -1.</returns>
        public static long GetAvailableFreeSpaceBytes(this string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return -1;
            }

            try
            {
                var di = new DirectoryInfo(path);
                var driveInfo = new DriveInfo(di.Root.FullName);

                return driveInfo.AvailableFreeSpace;
            }
            catch (ArgumentException)
            {
            }
            catch (IOException)
            {
            }

            return -1;
        }

        /// <summary>
        ///     Get size of file.
        /// </summary>
        /// <returns>File size in bytes if file exists, else -1.</returns>
        public static long GetFileSizeBytes(this string filePath)
        {
            if (!File.Exists(filePath))
            {
                return -1;
            }

            var fi = new FileInfo(filePath);

            return fi.Length;
        }
    }
}
