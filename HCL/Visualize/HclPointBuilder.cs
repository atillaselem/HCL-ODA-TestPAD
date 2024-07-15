using HCL_ODA_TestPAD.HCL.CAD.Math.API;
using HCL_ODA_TestPAD.HCL.Visualize.Extensions;
using HCL_ODA_TestPAD.Utility;
using HCL_ODA_TestPAD.ViewModels.Base;
using ODA.Visualize.TV_Visualize;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;



namespace HCL_ODA_TestPAD.HCL.Visualize
{
    public enum PointColor
    {
        Red,
        Green,
        Blue,
        Magenta,
        Yellow,
        Cyan,
        Orange,
        Pink,
        Purple
    }

    public class HclPointBuilder
    {
        private static IHclTooling _hclTooling;
        private static HclPointContainer _hclPointContainer;
        private const double PointRadiusFactor = 5;
        public static HclPointContainer ShowPoints(IHclTooling hclViewModel)
        {
            _hclTooling = hclViewModel;
            if (_hclPointContainer != null)
            {
                _hclPointContainer.Remove();
                _hclPointContainer = null;
                return _hclPointContainer;
            }
            BuildModel();
            var (min, max) = _hclTooling.GetViewExtent();
            using (min)
            using (max)
            {
                var color = Color.FromName(_hclTooling.ServiceFactory.AppSettings.PointColor.ToString());
                var isRandomColor = _hclTooling.ServiceFactory.AppSettings.IsRandomColor;
                for (int i = 0; i < _hclTooling.ServiceFactory.AppSettings.NumberOfPoints; i++)
                {
                    var X = RandomGen.Next(min.X, max.X);
                    var Y = RandomGen.Next(min.Y, max.Y);
                    var Z = RandomGen.Next(min.Z, max.Z);
                    using var location = CadPoint3D.With(X, Y, Z);
                    if(isRandomColor)
                    {
                        int enumInt = RandomGen.NextEnum(typeof(PointColor));
                        var enumVal = (PointColor)enumInt;
                        color = Color.FromName(enumVal.ToString());
                    }
                    var entityName = $"Point-{i}";
                    using var entityId = AppendPoint(location, color, entityName);
                    AppendText(entityId, location, Color.Black, $"P-{i+1}");
                }
            }
            _hclPointContainer.SetViewMatrixCoordinates();
            return _hclPointContainer;
        }
        public static void Dispose(IHclTooling hclViewModel)
        {
            if (_hclPointContainer != null)
            {
                _hclPointContainer.Remove();
                _hclPointContainer = null;
            }
        }
        private static void BuildModel()
        {
            _hclPointContainer ??= new HclPointContainer(_hclTooling);

            // Model created but image removed.
            if (_hclPointContainer.ModelLifeCycle == ModelLifeCycle.Removed)
            {
                return;
            }
            using var tvDatabase = _hclTooling.TvDatabaseId.openObject(OdTv_OpenMode.kForWrite);
            using var odTvGsViewId = _hclTooling.GetViewId();
            // Create model
            _hclPointContainer.TvModelId = tvDatabase.createModel("Tv_Model_Points", OdTvModel_Type.kDirect);
            using var odTvGsView = odTvGsViewId.openObject(OdTv_OpenMode.kForWrite);
            odTvGsView.addModel(_hclPointContainer.TvModelId);
        }
        private static OdTvEntityId AppendPoint(CadPoint3D location, Color color, string entityName)
        {
            using var odTvGsViewId = _hclTooling.GetViewId();
            var radius = odTvGsViewId.GetPixelScaleFactorAtViewTarget(PointRadiusFactor);
            using var cadPoint = new CadCircle(radius, location) {
                Color = color, 
                Filled = true, 
                Normal = CadVector3D.With(0, 0, 1),
                LineWeight = LineWeightType.Normal
            };

            var tvModel = new TvModel(_hclPointContainer.TvModelId);
            var entityId = tvModel.AppendEntity(entityName);
            using var entity = entityId.openObject(OdTv_OpenMode.kForWrite);
            using var lineWeight = new OdTvLineWeightDef((byte)cadPoint.LineWeight);
            entity.setLineWeight(lineWeight);
            using var colorDef = new OdTvColorDef(cadPoint.Color.R, cadPoint.Color.G, cadPoint.Color.B);
            entity.setColor(colorDef);
            using var pointEntityGeometryId = entity.appendCircle(cadPoint.CenterLoc, cadPoint.Radius, cadPoint.Normal);
            using var pointEntityGeometry = pointEntityGeometryId.openAsCircle();
            pointEntityGeometry.setFilled(cadPoint.Filled);
            _hclPointContainer.PointPositionList.Add(CadPoint3D.With(location));
            return entityId;
        }
        private static void AppendText(OdTvEntityId entityId, CadPoint3D location, Color color, string pointText)
        {
            using var odTvGsViewId = _hclTooling.GetViewId();
            using var eyeToWorldMatrix = odTvGsViewId.EyeToWorldMatrix();
            var textScale = 15.0;
            using var text = new CadText(location, 1.0,
                       odTvGsViewId.GetPixelScaleAtEyeSystem(location, textScale), pointText,
                       color);

            using var entity = entityId.openObject(OdTv_OpenMode.kForWrite);
            using var geometryDataId = entity.appendText(CadPoint3D.Origin, text.Text);
            using var geometry = geometryDataId.openObject();
            using var colorDef = new OdTvColorDef(text.Color.R, text.Color.G, text.Color.B);
            geometry.setColor(colorDef);

            using var textData = geometryDataId.openAsText();
            textData.setAlignmentPoint(text.Position);
            textData.setPosition(text.Position);
            textData.setTextSize(text.TextSize);
            textData.setNormal(odTvGsViewId.Direction());
            textData.setString(pointText);
            textData.setAlignmentMode(OdTvTextStyle_AlignmentType.kBottomCenter); 
        }
    }

}
