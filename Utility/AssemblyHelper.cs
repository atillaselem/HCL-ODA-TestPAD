using System;
using System.Linq;
using System.Reflection;

namespace HCL_ODA_TestPAD.Utility;

public class AssemblyHelper
{
    /// <summary>
    /// Call this method at the beginning of the program
    /// </summary>
    public static void LoadExternalDependencies(string[] externalDllFiles, string relativePath)
    {
        AppDomain.CurrentDomain.AssemblyResolve += delegate (object sender, ResolveEventArgs args)
        {
            string assemblyFile = (args.Name.Contains(','))
                ? args.Name.Substring(0, args.Name.IndexOf(','))
                : args.Name;

            assemblyFile += ".dll";

            // Forbid non handled dll's
            if (!externalDllFiles.Contains(assemblyFile))
            {
                return null;
            }

            string dllRelativePath = relativePath + assemblyFile;
            string targetPath = PathResolver.GetTargetPathUsingRelativePath(dllRelativePath);

            try
            {
                return Assembly.LoadFile(targetPath);
            }
            catch (Exception)
            {
                return null;
            }
        };
    }
    private static string GetAssemblyVersionText()
    {
        var assemblyFullPath = Assembly.GetExecutingAssembly().Location;
        var version = AssemblyName.GetAssemblyName(assemblyFullPath).Version.ToString();
        version = version.Substring(0, version.LastIndexOf('.'));
        return version;
    }

    private static string GetPlatformTarget()
    {
        string platformTarget = "AnyCPU";
#if x86
        platformTarget = "(32 bit)";
#elif x64
    platformTarget = "(64 bit)";
#endif
        return platformTarget;
    }

    public static string GetAppTitle()
    {
        var assemblyFullPath = Assembly.GetExecutingAssembly().Location;
        var assemblyName = AssemblyName.GetAssemblyName(assemblyFullPath).Name;
        return $"{assemblyName.Substring(0, assemblyName.Length-4)} {GetPlatformTarget()} - Version {{0.{GetAssemblyVersionText()}}}";
    }
}