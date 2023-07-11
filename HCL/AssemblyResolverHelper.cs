#nullable enable
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace HCL_ODA_TestPAD.HCL;

/// <summary>
///     Represents helper class for resolving assemblies.
/// </summary>
public class AssemblyResolverHelper
{
    private static AssemblyResolverHelper? _instance;

    /// <summary>
    ///     Absolute search paths.
    /// </summary>
    private readonly string[] _searchPaths = { Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "libs") };

    private AssemblyResolverHelper(string[]? searchPaths = null)
    {
        if (searchPaths != null)
        {
            _searchPaths = searchPaths;
        }

        AssemblyLoadContext.Default.Resolving += CustomResolving;
    }

    private Assembly? CustomResolving(AssemblyLoadContext loadContext, AssemblyName assemblyName)
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var dllName = $"{assemblyName.Name}.dll";
        var location = string.Concat(baseDirectory, dllName);

        if (!File.Exists(location))
        {
            // use subdirectories
            var subDirectory = _searchPaths.FirstOrDefault(searchPath =>
            {
                var searchDir = Path.Combine(baseDirectory, searchPath);
                var path = Path.Combine(searchDir, dllName);
                return File.Exists(path);
            });

            if (string.IsNullOrEmpty(subDirectory))
            {
                return null;
            }

            location = Path.Combine(baseDirectory, subDirectory, dllName);

            if (!File.Exists(location))
            {
                return null;
            }
        }

        var assembly = loadContext.LoadFromAssemblyPath(location);
        Debug.WriteLine($"AssemblyLoad: Loading {dllName} from {location}");
        return assembly;
    }

    public static void Initialize(string[]? searchPaths = null)
    {
        _instance ??= new AssemblyResolverHelper(searchPaths);
    }
}
