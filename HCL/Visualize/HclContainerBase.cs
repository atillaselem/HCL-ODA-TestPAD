using HCL_ODA_TestPAD.HCL.CAD.Math.API;
using HCL_ODA_TestPAD.ViewModels.Base;
using ODA.Visualize.TV_Visualize;
using System;

namespace HCL_ODA_TestPAD.HCL.Visualize
{
    public readonly record struct ViewInverseMatrix(CadVector3D XAxis, CadVector3D YAxis, CadVector3D ZAxis);

    public abstract class HclContainerBase : IDisposable
    {
        public ModelLifeCycle ModelLifeCycle { get; set; }
        public OdTvModelId TvModelId { get; set; }
        public CadRasterImage CadRasterImage { get; set; }
        protected IHclTooling HclTooling { get; set; }
        public ulong ToolImageHandle { get; set; }
        public ViewInverseMatrix ViewInverseMatrix { get; set; }
        public void SetViewMatrixCoordinates()
        {
            using var view = HclTooling.GetViewId();
            using var eyeToWorldMatrix = view.EyeToWorldMatrix();
            ViewInverseMatrix = new ViewInverseMatrix(eyeToWorldMatrix.XAxis(), eyeToWorldMatrix.YAxis(), eyeToWorldMatrix.ZAxis());
        }
        public abstract void UpdateTransformations(double scaleFactor);

        public abstract void Remove();

        #region Disposable HclToolBase
        private bool IsDisposed { get; set; }
        ~HclContainerBase()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool isDisposing)
        {
            try
            {
                if (IsDisposed || !isDisposing)
                {
                    return;
                }

                TvModelId?.Dispose();
                TvModelId = null;
                CadRasterImage?.Dispose();
                CadRasterImage = null;
                ToolImageHandle = default;
            }
            finally
            {
                IsDisposed = true;
            }
        }
        #endregion
    }

}
