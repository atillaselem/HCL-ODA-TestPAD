using System;
using System.Windows;

namespace HCL_ODA_TestPAD.HCL.MouseTouch
{
    public class ZoomHelper
    {
        public enum OperationMode
        {
            None,
            ZoomToScale,
            ZoomToArea
        }
        public static double CalculateDistance(Point firstPoint, Point secondPoint)
        {
            return Math.Sqrt(Math.Pow(firstPoint.X - secondPoint.X, 2) + Math.Pow(firstPoint.Y - secondPoint.Y, 2));
        }
    }
}
