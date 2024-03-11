using System;
using System.Globalization;
using System.Xml.Linq;

namespace HCL_ODA_TestPAD.Functional.Extensions
{
    public static class XElementExtensions
    {
        public static T GetAs<T>(this XElement elem, T defaultValue = default)
        {
            T ret = defaultValue;

            if (elem != null && !string.IsNullOrEmpty(elem.Value))
            {
                // Cast to Return Data Type
                // NOTE: ChangeType can not cast to a Nullable type
                ret = (T)Convert.ChangeType(elem.Value, typeof(T), CultureInfo.InvariantCulture);
            }

            return ret;
        }

        public static T GetAs<T>(this XAttribute attr, T defaultValue = default)
        {
            T ret = defaultValue;

            if (attr != null && !string.IsNullOrEmpty(attr.Value))
            {
                // Cast to Return Data Type
                // NOTE: ChangeType can not cast to a Nullable type
                ret = (T)Convert.ChangeType(attr.Value, typeof(T), CultureInfo.InvariantCulture);
            }

            return ret;
        }

        public static XElement GetParent(this XElement node, string parentNodeName)
        {
            if (node == null) return null;
            var parentNode = node.Parent;
            if (parentNode != null && parentNode.Name.LocalName == parentNodeName) return parentNode;
            return parentNode.GetParent(parentNodeName);
        }
    }
}
