using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Teigha.Core;

namespace HCL_ODA_TestPAD.HCL.MouseTouch
{
    public interface ICadTransformation : IDisposable
    {
        OdGePoint3d GetWorldCoordinates(double x, double y);
        Point TransformWorldToScreen(double x, double y);
        OdGePoint3d ToEyeToWorld(int x, int y);

    }
    public interface ICadZoomOperations : IDisposable
    {
        void ZoomToArea(OdGePoint3d point1, OdGePoint3d point2);
        void Zoom(double scale);
    }
}
