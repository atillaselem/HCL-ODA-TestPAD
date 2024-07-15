using HCL_ODA_TestPAD.HCL.CAD.Math.API;
using HCL_ODA_TestPAD.HCL.Visualize.Extensions;
using HCL_ODA_TestPAD.ViewModels.Base;
using ODA.Kernel.TD_RootIntegrated;
using ODA.Visualize.TV_Visualize;
using System.Drawing;

namespace HCL_ODA_TestPAD.HCL.Visualize
{
    public class HclToolLineBuilder
    {
        private static CadLine _hclToolLine;
        private static IHclTooling _hclTooling;
        public static CadLine ShowToolLine(IHclTooling hclViewModel, CadPoint3D startLocation, CadPoint3D endLocation)
        {
            _hclTooling = hclViewModel;
            Dispose(_hclTooling);
            return BuildModel(startLocation, endLocation);
        }
        public static void Dispose(IHclTooling hclViewModel)
        {
            if (_hclToolLine != null)
            {
                _hclToolLine.Remove(hclViewModel);
                _hclToolLine = null;
            }
        }
        private static CadLine BuildModel(CadPoint3D startLocation, CadPoint3D endLocation)
        {
            // Create model
            using var tvDatabase = _hclTooling.TvDatabaseId.openObject(OdTv_OpenMode.kForWrite);
            var modelId = tvDatabase.createModel("Tv_Model_ToolLine", OdTvModel_Type.kDirect);
            using var odTvGsViewId = _hclTooling.GetViewId();
            using var odTvGsView = odTvGsViewId.openObject(OdTv_OpenMode.kForWrite);
            odTvGsView.addModel(modelId);

            var tvModel = new TvModel(modelId);
            using var entityId = tvModel.AppendEntity("ToolLine");
            //using var cadPolyLine = CadEntity.CreateLine(startLocation, endLocation);
            var gripSize = odTvGsViewId.GetPixelScaleAtEyeSystem(endLocation, 5);
            var cadPolyLine = new CadLine(startLocation, endLocation)
            {
                Color = Color.FromArgb(255, 0, 0),
                LineWeight = LineWeightType.BoldM,
                LineScale = gripSize,
                IsRay = false,
                LineStyle = OdPs_LineType.kLtpMediumDash,
                IsSliceable = false,
                TvModelId = modelId
            };
            using var entity = entityId.openObject(OdTv_OpenMode.kForWrite);
            var polyLineGeometryId = entity.appendPolyline(startLocation, endLocation);
            var entityHandle = entity.getDatabaseHandle();
            cadPolyLine.VisibleEntityGeometryDict.Add(entityHandle, polyLineGeometryId);
            using var lineWeight = new OdTvLineWeightDef((byte)cadPolyLine.LineWeight);
            entity.setLineWeight(lineWeight);
            string styleName = CadModelConstants.DashedStyle;
            switch (cadPolyLine.LineStyle)
            {
                case OdPs_LineType.kLtpShortDash:
                    styleName = CadModelConstants.ShortDash;
                    break;
                case OdPs_LineType.kLtpMediumDash:
                    styleName = CadModelConstants.MidiumDashedStyle;
                    break;
            }
            using var lineTypeId = tvDatabase.findLinetype(styleName);
            using var lineType = new OdTvLinetypeDef(lineTypeId);
            entity.setLinetype(lineType);
            entity.setLinetypeScale(cadPolyLine.LineScale);
            using var color = new OdTvColorDef(cadPolyLine.Color.R, cadPolyLine.Color.G, cadPolyLine.Color.B);
            entity.setColor(color);
            return cadPolyLine;
        }
    }

}
