#nullable enable

using System;
using System.IO;
using System.Reflection;


namespace HCL_ODA_TestPAD.Utility
{
    public static class AssemblyExtensions
    {
        /// <summary>
        /// Copy the resource file into temp folder and get the full file path.
        /// </summary>
        /// <param name="assembly">Currently executing assembly.</param>
        /// <param name="resourceFilePath">Full name of the assembly.</param>
        /// <param name="filePath">Full file path.</param>
        /// <returns>Returns the full file path in temp folder.</returns>
        public static string? CopyResourceToFile(this Assembly assembly, string resourceFilePath, string filePath)
        {
            ArgumentNullException.ThrowIfNull(assembly);

            if (File.Exists(filePath) && filePath.IsWriteLocked())
            {
                filePath = filePath.MakeFilePathUnique();
            }

            using var resourceStream = assembly.GetManifestResourceStream($"{resourceFilePath}");
            if (resourceStream == null)
            {
                return null;
            }

            try
            {
                using var file = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite);
                resourceStream.CopyTo(file);
            }
            catch (IOException)
            {
                return null;
            }

            return filePath;
        }
    }
}
