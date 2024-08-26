using HCL_ODA_TestPAD.HCL.CAD.Math.API;
using HCL_ODA_TestPAD.ViewModels.Base;
using ODA.Visualize.TV_Visualize;
using System.Collections.Generic;

namespace HCL_ODA_TestPAD.HCL.Visualize
{
    public class HclPointContainer : HclContainerBase
    {
        private List<CadPoint3D> PointPositionList { get; set; } = new();
        public static int PointListCount { get; set; }
        public HclPointContainer(IHclTooling hclTooling)
        {
            HclTooling = hclTooling;
        }
        public override void UpdateTransformations(double scaleFactor)
        {
            var tvModel = new TvModel(TvModelId);
            tvModel.UpdateModelTransformation(HclTooling.GetViewId(), PointPositionList, scaleFactor, ViewInverseMatrix);
            //tvModel.UpdateModelTransformationDelta(HclTooling.GetViewId(), PointPositionList, scaleFactor);
        }

        public void AddPoint(CadPoint3D point)
        {
            PointPositionList.Add(CadPoint3D.With(point));
        }
        public void SetPointCount()
        {
            PointListCount = PointPositionList.Count;
        }
        public override void Remove()
        {
            using var tvGsViewId = HclTooling.GetViewId();
            using var tvGsViewObj = tvGsViewId.openObject(OdTv_OpenMode.kForWrite);
            tvGsViewObj.eraseModel(TvModelId);
            using var db = HclTooling.TvDatabaseId.openObject(OdTv_OpenMode.kForWrite);
            db.removeModel(TvModelId);
            Dispose();
        }

        #region IDisposable HclPointContainer
        private bool IsDisposed { get; set; }
        protected override void Dispose(bool isDisposing)
        {
            try
            {
                if (IsDisposed)
                {
                    return;
                }
                if (isDisposing)
                {
                    IsDisposed = true;
                }
            }
            finally
            {
                base.Dispose(isDisposing);
                PointPositionList.ForEach(p => p.Dispose());
                PointPositionList.Clear();
                PointListCount = 0;
            }
        }

        internal void TogglePointText(bool showText)
        {
            var tvModel = new TvModel(TvModelId);
            tvModel.ToogleVisibility(showText);
        }

        #endregion
    }
}
