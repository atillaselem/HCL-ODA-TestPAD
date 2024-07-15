using HCL_ODA_TestPAD.HCL.CAD.Math.API;
using HCL_ODA_TestPAD.ViewModels.Base;
using ODA.Visualize.TV_Visualize;
using System;
using System.Collections.Generic;

namespace HCL_ODA_TestPAD.HCL.Visualize
{
    public sealed class CadLine : CadEntity, IDisposable
    {
        public Dictionary<ulong, OdTvGeometryDataId> VisibleEntityGeometryDict { get; } = new();

        public CadLine(CadPoint3D startLoc, CadPoint3D endLoc)
        {
            StartLoc = CadPoint3D.With(startLoc);
            EndLoc = CadPoint3D.With(endLoc);
        }

        public CadPoint3D StartLoc { internal set; get; }
        public CadPoint3D EndLoc { internal set; get; }
        public OdTvModelId TvModelId { internal set; get; }
        public bool IsRay { internal set; get; }

        public void Dispose()
        {
            StartLoc.Dispose();
            EndLoc.Dispose();
            TvModelId?.Dispose();
        }
        public void Remove(IHclTooling hclTooling)
        {
            using var tvGsViewId = hclTooling.GetViewId();
            using var tvGsViewObj = tvGsViewId.openObject(OdTv_OpenMode.kForWrite);
            tvGsViewObj.eraseModel(TvModelId);
            using var db = hclTooling.TvDatabaseId.openObject(OdTv_OpenMode.kForWrite);
            db.removeModel(TvModelId);
            Dispose();
        }
    }
}
