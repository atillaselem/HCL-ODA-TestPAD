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

        public override void UpdateViewTransformation()
        {
            var tvModel = new TvModel(TvModelId);
            tvModel.UpdateModelViewTransformations(HclTooling.GetViewId(), ToolLocationList, VisibleEntityDict.Values.ToArray());
            tvModel.UpdateCrossViewTransformations(HclTooling.GetViewId(), CrossLocationList, CrossHairDict.Values.ToArray());
        }

        public override void Remove()
        { 
            var model = new TvModel(TvModelId);
            model.RemoveModel(HclTooling);
            ToolLocationList.ForEach(p => p.Dispose());
            ToolLocationList.Clear();
            VisibleEntityDict.Clear();
            CrossHairDict.Clear();
            CrossLocationList.ForEach(p => p.Dispose());
            CrossLocationList.Clear();
            Dispose();
        }



    }
}
