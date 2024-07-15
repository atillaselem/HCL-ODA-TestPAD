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
        public override void UpdateOrientation()
        {
            var tvModel = new TvModel(TvModelId);
            tvModel.UpdateEntityOrientation(HclTooling.GetViewId(), ToolImageHandle);
        }
        public override void UpdateTransformations(double scaleFactor)
        {
            ScaleModelAtEntityLevel(scaleFactor);
            UpdateOrientation();
        }
        public override void Remove()
        {
            var model = new TvModel(TvModelId);
            model.RemoveModel(HclTooling);
            Dispose();
        }
    }
}
