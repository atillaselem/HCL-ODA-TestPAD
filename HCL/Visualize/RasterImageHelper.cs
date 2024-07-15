using System.IO;
using System.Reflection;
using HCL_ODA_TestPAD.Utility;

namespace HCL_ODA_TestPAD.HCL.Visualize
{
    internal class RasterImageHelper
    {
        public static string GetResourceFilePath(string fileName)
        {
            var ext = ".png";
            var assembly = Assembly.GetExecutingAssembly();
            var filePath = Path.Combine(Path.GetTempPath(), fileName+ext);
            if(!File.Exists(filePath))
            {
                assembly.CopyResourceToFile($"HCL_ODA_TestPAD.HCL.Visualize.Images.{fileName+ext}", filePath);
            }
            return filePath;
        }
    }
}
