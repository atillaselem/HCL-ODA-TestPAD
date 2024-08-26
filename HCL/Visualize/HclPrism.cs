using HCL_ODA_TestPAD.HCL.CAD.Math.API;
using HCL_ODA_TestPAD.ViewModels;
using HCL_ODA_TestPAD.ViewModels.Base;
using ODA.Visualize.TV_Visualize;
using System.Collections.Generic;
using System.Linq;

namespace HCL_ODA_TestPAD.HCL.Visualize
{
    public class HclPrism : HclToolBase
    {
        public HclPrism(IHclTooling hclTooling)
        {
            HclTooling = hclTooling;
        }
        public override void UpdateOrientation()
        {
            var tvModel = new TvModel(TvModelId);
            tvModel.UpdateEntityOrientation(HclTooling.GetViewId(), VisibleEntityDict.Values.ToArray());
        }

        public void UpdatePrismVisibility()
        {
            var tvModel = new TvModel(TvModelId);
            tvModel.Show();
        }
        public override void UpdateTransformations(double scaleFactor)
        {
            ScaleModelAtEntityLevel(scaleFactor);
            UpdateOrientation();
            UpdatePrismVisibility();
        }
        public override void UpdateViewTransformation()
        {
            var tvModel = new TvModel(TvModelId);
            tvModel.UpdateModelViewTransformations(HclTooling.GetViewId(), ToolLocationList, VisibleEntityDict.Values.ToArray());
        }

        public override void Remove()
        { 
            var model = new TvModel(TvModelId);
            model.RemoveModel(HclTooling);
            ToolLocationList.ForEach(p => p.Dispose());
            ToolLocationList.Clear();
            Dispose();
        }



    }
}
