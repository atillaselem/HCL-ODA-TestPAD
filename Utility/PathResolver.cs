using System;
using System.IO;
using System.Reflection;

namespace HCL_ODA_TestPAD.Utility
{
    public class PathResolver
    {
        public static string AbsolutePathOfExecution()
        {
            return Environment.CurrentDirectory;
        }

        public static string GetTargetPathUsingRelativePath(string relFilePath)
        {
            string targetPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, relFilePath));
            return targetPath;
        }

        public static string GetAssemblyVersionText(string versionFilePath)
        {
            var versionFile = RecursivePathFinder.SearchFileFolderPath(versionFilePath);
            var version = AssemblyName.GetAssemblyName(versionFile).Version.ToString();
            version = version.Substring(0, version.LastIndexOf('.'));
            return version;
        }
    }
}
