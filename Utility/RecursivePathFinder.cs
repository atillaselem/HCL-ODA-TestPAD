using System.Diagnostics;
using System.IO;

namespace HCL_ODA_TestPAD.Utility
{
    public static class RecursivePathFinder
    {
        public static string SearchFileFolderPath(string fileName, bool isDir = false)
        {
            string parentDirectory = PathResolver.AbsolutePathOfExecution();
            return FindTargetFileFolderPath(parentDirectory, fileName, isDir);
        }

        private static string FindTargetFileFolderPath(string currentDirectory, string fileName, bool isDir = false)
        {
            if (!string.IsNullOrEmpty(currentDirectory))
            {
                string searchedPath = Path.Combine(currentDirectory, fileName);
                Debug.WriteLine($"Current Searching Path : {searchedPath}");
                if (isDir)
                {
                    var dirPath = Path.GetDirectoryName(searchedPath);
                    if (!Directory.Exists(dirPath))
                    {
                        string parentDirectory = Directory.GetParent(currentDirectory)?.FullName;
                        return FindTargetFileFolderPath(parentDirectory, fileName, isDir);
                    }
                    else
                    {
                        return dirPath;
                    }
                }
                else
                {
                    if (!File.Exists(searchedPath))
                    {
                        string parentDirectory = Directory.GetParent(currentDirectory)?.FullName;
                        return FindTargetFileFolderPath(parentDirectory, fileName);
                    }
                    else
                    {
                        return searchedPath;
                    }
                }
            }
            return null;
        }
    }
}
