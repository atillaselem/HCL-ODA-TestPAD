using HCL_ODA_TestPAD.ViewModels;
using HCL_ODA_TestPAD.ViewModels.Base;
using ODA.Visualize.TV_Visualize;
using System.Collections.Generic;
using System.Linq;

namespace HCL_ODA_TestPAD.HCL.Visualize
{
    public class HclPrism : HclToolBase
    {
        public Dictionary<VisibleEntityType, ulong> VisibleEntityDict { get; } = new();
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
        public override void Remove()
        { 
            var model = new TvModel(TvModelId);
            model.RemoveModel(HclTooling);
            Dispose();
        }
        public void AddVisibleEntity(VisibleEntityType type, ulong entityHandleId)
        {
            VisibleEntityDict.Add(type, entityHandleId);
        }


    }
}
