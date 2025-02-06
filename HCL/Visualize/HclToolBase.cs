using HCL_ODA_TestPAD.HCL.CAD.Math.API;
using HCL_ODA_TestPAD.ViewModels;
using HCL_ODA_TestPAD.ViewModels.Base;
using ODA.Visualize.TV_Visualize;
using System;
using System.Collections.Generic;

namespace HCL_ODA_TestPAD.HCL.Visualize
{
    public abstract class HclToolBase : IDisposable
    {
        public ModelLifeCycle ModelLifeCycle { get; set; }
        public OdTvModelId TvModelId { get; set; }
        public CadRasterImage CadRasterImage { get; set; }
        protected IHclTooling HclTooling { get; set; }
        public ulong ToolImageHandle { get; set; }
        public Dictionary<VisibleEntityType, ulong> VisibleEntityDict { get; } = [];
        public Dictionary<VisibleEntityType, ulong> CrossHairDict { get; } = [];
        protected List<CadPoint3D> ToolLocationList { get; } = [];
        protected List<CadPoint3D> CrossLocationList { get; } = [];
        public void AddLocation(CadPoint3D location)
        {
            ToolLocationList.Add(CadPoint3D.With(location));
        }
        public void AddCrossLocation(CadPoint3D location)
        {
            CrossLocationList.Add(CadPoint3D.With(location));
        }

        public virtual void UpdateLocationDelta(CadPoint3D location, CadPoint3D oldLocation)
        {
            var tvModel = new TvModel(TvModelId);
            using var delta = location - oldLocation;
            tvModel.UpdateLocationDelta(delta);
        }

        public abstract void UpdateViewTransformation();
        public virtual void ScaleModelAtEntityLevel(double scaleFactor)
        {
            var tvModel = new TvModel(TvModelId);
            tvModel.UpdateScaleOfEntity(scaleFactor);
        }
        public virtual void ScaleModelAtLocation(double scaleFactor, CadPoint3D location)
        {
            var tvModel = new TvModel(TvModelId);
            tvModel.UpdateModelScale(scaleFactor, location);
        }
        public abstract void Remove();
        public void AddVisibleEntity(VisibleEntityType type, ulong entityHandleId)
        {
            VisibleEntityDict.Add(type, entityHandleId);
        }
        public void AddCrossHairEntity(VisibleEntityType type, ulong entityHandleId)
        {
            CrossHairDict.Add(type, entityHandleId);
        }
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
