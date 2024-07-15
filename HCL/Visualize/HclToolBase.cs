using HCL_ODA_TestPAD.HCL.CAD.Math.API;
using HCL_ODA_TestPAD.ViewModels.Base;
using ODA.Visualize.TV_Visualize;
using System;

namespace HCL_ODA_TestPAD.HCL.Visualize
{
    public abstract class HclToolBase : IDisposable
    {
        public ModelLifeCycle ModelLifeCycle { get; set; }
        public OdTvModelId TvModelId { get; set; }
        public CadRasterImage CadRasterImage { get; set; }
        protected IHclTooling HclTooling { get; set; }
        public ulong ToolImageHandle { get; set; }

        public virtual void UpdateLocation(CadPoint3D location)
        {
            var tvModel = new TvModel(TvModelId);
            tvModel.UpdateLocation(location);
        }
        public abstract void UpdateOrientation();
        public abstract void UpdateTransformations(double scaleFactor);
        public virtual void ScaleModelAtEntityLevel(double scaleFactor)
        {
            var tvModel = new TvModel(TvModelId);
            tvModel.UpdateScaleOfEntity(scaleFactor);
        }
        public abstract void Remove();

        #region Disposable HclToolBase
        private bool IsDisposed { get; set; }
        ~HclToolBase()
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
