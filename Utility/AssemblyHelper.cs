using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace HCL_ODA_TestPAD.Utility;

public class AssemblyHelper
{
    private const string X32Bit = "(32-bit)";
    private const string X64Bit = "(64-bit)";
    private const string OdaRel = "[ODA Rel : 25.8]";
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

    private static string GetProcessArchitecture()
    {
        return RuntimeInformation.ProcessArchitecture == Architecture.X86 ? X32Bit : X64Bit; ;
    }

    public static string GetAppTitle()
    {
        var assemblyFullPath = Assembly.GetExecutingAssembly().Location;
        var assemblyName = AssemblyName.GetAssemblyName(assemblyFullPath).Name;
        //return $"{assemblyName.Substring(0, assemblyName.Length)} {GetProcessArchitecture().ToLower()} {ODA_REL}- Version {{0.{GetAssemblyVersionText()}}}";
        return $"{assemblyName.Substring(0, assemblyName.Length)} - {OdaRel} - Version {{0.{GetAssemblyVersionText()}}}";
    }
}