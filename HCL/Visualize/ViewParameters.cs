using HCL_ODA_TestPAD.HCL.CAD.Math.API;
using HCL_ODA_TestPAD.ViewModels.Base;
using ODA.Visualize.TV_Visualize;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HCL_ODA_TestPAD.HCL.Visualize
{
    public enum ProjectionTypes
    {
        Parallel,
        Perspective
    }
    public sealed class ViewParameters : IDisposable
    {
        public static ViewParameters FromView(IHclTooling viewProvider)
        {
            ArgumentNullException.ThrowIfNull(viewProvider);

            using var view = viewProvider.GetViewId();
            return new ViewParameters(view);
        }
        public static void ToView(IHclTooling viewProvider, ViewParameters viewParameters)
        {
            ArgumentNullException.ThrowIfNull(viewProvider);
            ArgumentNullException.ThrowIfNull(viewParameters);

            using var view = viewProvider.GetViewId();
            view.SetView(viewParameters.Position, viewParameters.Target, viewParameters.UpVector, viewParameters.FieldWidth, viewParameters.FieldHeight, viewParameters.Projection);
        }


        internal ViewParameters(OdTvGsViewId view)
        {
            ArgumentNullException.ThrowIfNull(view);

            FieldWidth = view.FieldWidth();
            FieldHeight = view.FieldHeight();
            Target = view.Target();
            Position = view.Position();
            Direction = view.Direction();
            UpVector = view.UpVector();
            Projection = view.IsPerspective() ? ProjectionTypes.Perspective : ProjectionTypes.Parallel;
            IsInInteractivity = view.IsInInteractivity();
        }

        private void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }
            Target.Dispose();
            Position.Dispose();
            Direction.Dispose();
            UpVector.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~ViewParameters()
        {
            Dispose(false);
        }

        public bool IsInInteractivity { get; }

        public double FieldHeight { get; }

        public double FieldWidth { get; }

        public CadVector3D Direction { get; }

        private CadPoint3D Position { get; }

        public CadVector3D UpVector { get; }

        private CadPoint3D Target { get; }

        private ProjectionTypes Projection { get; }
    }

}
