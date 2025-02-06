using System.Linq;
using HCL_ODA_TestPAD.HCL.CAD.Math.API;
using HCL_ODA_TestPAD.ViewModels.Base;
using ODA.Visualize.TV_Visualize;

namespace HCL_ODA_TestPAD.HCL.Visualize
{
    public class HclStation : HclToolBase
    {
        public HclStation(IHclTooling hclTooling)
        {
            HclTooling = hclTooling;
        }

        public override void Remove()
        {
            var model = new TvModel(TvModelId);
            model.RemoveModel(HclTooling);
            ToolLocationList.ForEach(p => p.Dispose());
            ToolLocationList.Clear();
            Dispose();
        }

        public override void UpdateViewTransformation()
        {
            var tvModel = new TvModel(TvModelId);
            tvModel.UpdateModelViewTransformations(HclTooling.GetViewId(), ToolLocationList, VisibleEntityDict.Values.ToArray());
        }
    }
}
