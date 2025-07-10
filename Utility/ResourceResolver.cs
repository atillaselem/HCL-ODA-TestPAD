using System.Windows;

namespace HCL_ODA_TestPAD.Utility
{
    public class ResourcesResolver
    {
        public static T ResolveResource<T>(string key)
        {
            var value = Application.Current.FindResource(key);

            return value is not T retVal ? default : retVal;
        }
    }
}
