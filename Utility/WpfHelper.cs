using System.Windows.Media;
using System.Windows;
using System;

namespace HCL_ODA_TestPAD.Utility;

public static class WpfHelper
{
    public static DependencyObject FindChild(DependencyObject parent, Func<DependencyObject, bool> predicate)
    {
        if (parent == null) return null;

        int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < childrenCount; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);

            if (predicate(child))
            {
                return child;
            }
            else
            {
                var foundChild = FindChild(child, predicate);
                if (foundChild != null)
                    return foundChild;
            }
        }
        return null;
    }
}