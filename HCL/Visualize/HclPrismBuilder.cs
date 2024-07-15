using HCL_ODA_TestPAD.HCL.CAD.Math.API;
using HCL_ODA_TestPAD.HCL.Visualize.Extensions;
using HCL_ODA_TestPAD.Settings;
using HCL_ODA_TestPAD.ViewModels;
using HCL_ODA_TestPAD.ViewModels.Base;
using ODA.Kernel.TD_RootIntegrated;
using ODA.Visualize.TV_Visualize;
using System;
using System.IO;
using System.Linq;

namespace HCL_ODA_TestPAD.HCL.Visualize
{
    public class HclPrismBuilder
    {
        private static HclPrism _hclPrism;
        private static IHclTooling _hclTooling;
        private const double PixelFactor = 100;
        public static HclPrism ShowPrism(IHclTooling hclViewModel, CadPoint3D location)
        {
            _hclTooling = hclViewModel;
            if (_hclPrism != null)
            {
                Dispose(_hclTooling);
                _hclPrism = null;
                return _hclPrism;
            }

            _hclPrism = BuildModel();

            //OK : All parts other than image transforms properly in dwg with rotated screen.
            AppendArrow();
            AppendCircle();
            AppendCrossLines();
            BuildImage();

            //OK : Location Updates to click point
            _hclPrism.UpdateLocation(location);
            //OK : Orientation Updates to camera even rotated view
            _hclPrism.UpdateOrientation();

            return _hclPrism;
        }
        public static void Dispose(IHclTooling hclViewModel)
        {
            if (_hclPrism != null)
            {
                _hclPrism.Remove();
                _hclPrism = null;
            }
        }
        private static HclPrism BuildModel()
        {
            _hclPrism = new HclPrism(_hclTooling);
            using var tvDatabase = _hclTooling.TvDatabaseId.openObject(OdTv_OpenMode.kForWrite);
            using var odTvGsViewId = _hclTooling.GetViewId();
            // Create model
            _hclPrism.TvModelId = tvDatabase.createModel("Tv_Model_Prism", OdTvModel_Type.kDirect);
            using var odTvGsView = odTvGsViewId.openObject(OdTv_OpenMode.kForWrite);
            odTvGsView.addModel(_hclPrism.TvModelId);
            return _hclPrism;
        }
        private static void AppendArrow()
        {
            //Arrow
            using var odTvGsViewId = _hclTooling.GetViewId();

            using var upVec = CadVector3D.With(0, 1, 0);
            using var xAxis = CadVector3D.With(1, 0, 0);

            var scaleFactor = odTvGsViewId.GetPixelScaleFactorAtViewTarget(PixelFactor/2);
            using var topVector = upVec.Mul(scaleFactor * Math.Sqrt(2));
            using var rightVector = xAxis.Mul(scaleFactor);
            using var leftVector = -xAxis.Mul(scaleFactor);

            var arrowPointList = new CadPoint3D[4];
            //arrow bottom point
            var startLocation = CadPoint3D.Origin;
            using var middleTop = startLocation.Add(topVector);
            arrowPointList[0] = CadPoint3D.With(startLocation);
            //arrow left point
            using var topLeft = middleTop.Add(leftVector);
            arrowPointList[1] = topLeft;
            //arrow right point
            using var topRight = middleTop.Add(rightVector);
            arrowPointList[2] = topRight;
            //closing polygon
            arrowPointList[3] = CadPoint3D.With(startLocation);

            using var arrow = new CadPolygon(arrowPointList);
            arrow.IsSliceable = false;
            arrow.LineWeight = LineWeightType.BoldL;
            var reflector = _hclTooling.ServiceFactory.AppSettings.PrismType;
            using var arrowLength = upVec.Mul(scaleFactor * GetArrowLength(reflector));

            using var lineStartPoint = CadPoint3D.With(startLocation);

            using var lineEndPoint = startLocation.Add(arrowLength);

            using var arrowLine = new CadLine(lineStartPoint, lineEndPoint);

            using var lineWeight = new OdTvLineWeightDef((byte)LineWeightType.BoldL);

            var tvModel = new TvModel(_hclPrism.TvModelId);
            using var entityArrow = tvModel.OpenEntity("ToolPrism_Arrow");
            entityArrow.setLineWeight(lineWeight);
            using var polygon = entityArrow.appendPolygon(arrow.GetPoints().Select(p => (OdGePoint3d)p).ToArray());
            using var polygonId = polygon.openAsPolygon();
            polygonId.setFilled(true);
            using var geometry = polygon.openObject();
            using var color = new OdTvColorDef(arrow.Color.R, arrow.Color.G, arrow.Color.B);
            geometry.setColor(color);
            _hclPrism.VisibleEntityDict.Add(VisibleEntityType.ArrowEntity, entityArrow.getDatabaseHandle());

            using var entityArrowLine = tvModel.OpenEntity("ToolPrism_ArrowLine");
            entityArrowLine.setLineWeight(lineWeight);
            using var polyLineGeometryId = entityArrowLine.appendPolyline(arrowLine.StartLoc, arrowLine.EndLoc);
            var entityHandle = entityArrowLine.getDatabaseHandle();
            _hclPrism.VisibleEntityDict.Add(VisibleEntityType.ArrowLineEntity, entityHandle);
        }
        private static void AppendCircle()
        {
            using var odTvGsViewId = _hclTooling.GetViewId();
            var radius = odTvGsViewId.GetPixelScaleFactorAtViewTarget(PixelFactor);
            using var origin = CadPoint3D.Default;
            using var cadCircle = new CadCircle(radius, origin);
            cadCircle.Normal = CadVector3D.With(0, 0, 1);
            cadCircle.LineWeight = LineWeightType.BoldL;
            var tvModel = new TvModel(_hclPrism.TvModelId);
            using var entity = tvModel.OpenEntity("ToolPrism_Circle");
            using var lineWeight = new OdTvLineWeightDef((byte)cadCircle.LineWeight);
            entity.setLineWeight(lineWeight);
            using var circleEntity = entity.appendCircle(cadCircle.CenterLoc, cadCircle.Radius, cadCircle.Normal);
            _hclTooling.LastRadius = radius;
        }
        private static void AppendCrossLines()
        {
            using var odTvGsViewId = _hclTooling.GetViewId();
            var radius = odTvGsViewId.GetPixelScaleFactorAtViewTarget(PixelFactor);
            var length = radius * 0.9;
            var location = CadPoint3D.Default;
            //First Cross Line (-)
            using var firstLineStartLoc = CadPoint3D.With(location.X - length, location.Y, location.Z);
            using var firstLineEndLoc = CadPoint3D.With(location.X + length, location.Y, location.Z);
            //Second Cross Line (|)
            using var secondLineStartLoc = CadPoint3D.With(location.X, location.Y - length, location.Z);
            using var secondLineEndLoc = CadPoint3D.With(location.X, location.Y + length, location.Z);

            using var firstCrossLine = new CadLine(firstLineStartLoc, firstLineEndLoc);
            using var secondCrossLine = new CadLine(secondLineStartLoc, secondLineEndLoc);

            using var lineWeight = new OdTvLineWeightDef((byte)LineWeightType.BoldL);

            var tvModel = new TvModel(_hclPrism.TvModelId);
            using var entityFirstId = tvModel.AppendEntity("ToolPrism_CrossHairFirstLine");
            using var entityFirst = entityFirstId.openObject(OdTv_OpenMode.kForWrite);
            entityFirst.setLineWeight(lineWeight);
            using var lineFirst = entityFirst.appendPolyline(firstCrossLine.StartLoc, firstCrossLine.EndLoc);

            using var entitySecondId = tvModel.AppendEntity("ToolPrism_CrossHairSecondLine");
            using var entitySecond = entitySecondId.openObject(OdTv_OpenMode.kForWrite);
            entitySecond.setLineWeight(lineWeight);
            using var lineSecond = entitySecond.appendPolyline(secondCrossLine.StartLoc, secondCrossLine.EndLoc);
        }
        private static void BuildImage()
        {
            var resourceName = _hclTooling.ServiceFactory.AppSettings.PrismType.ToString();
            var imageFilePath = RasterImageHelper.GetResourceFilePath(resourceName);
            using var tvDatabase = _hclTooling.TvDatabaseId.openObject(OdTv_OpenMode.kForWrite);
            var rasterImageId = tvDatabase.createRasterImage(Path.GetFileNameWithoutExtension(imageFilePath), imageFilePath);
            var rasterImage = new TvRasterImage(rasterImageId);

            using var odTvGsViewId = _hclTooling.GetViewId();
            var pixel = odTvGsViewId.GetPixelScaleFactorAtViewTarget();

            using var tvImage = rasterImageId.openObject(OdTv_OpenMode.kForWrite);
            if (!tvImage.isLoaded())
            {
                tvImage.load();
            }

            const double MapFactor = 1.5;
            var width = pixel * rasterImage.PixelWidth() * MapFactor;
            var height = pixel * rasterImage.PixelHeight() * MapFactor;

            using var upVector = CadVector3D.With(0, 1, 0);
            using var xAxis = CadVector3D.With(1, 0, 0);
            using var xVec = xAxis * width;
            using var yVec = upVector * height;

            var pixelFactor = odTvGsViewId.GetPixelScaleFactorAtViewTarget(PixelFactor / 2);
            var reflector = _hclTooling.ServiceFactory.AppSettings.PrismType;
            using var arrowLength = upVector.Mul(pixelFactor * GetArrowLength(reflector));

            //OK : No Z-Axis shift
            using var originPt = CadPoint3D.With(-width / 2 + arrowLength.X, -height / 2 + arrowLength.Y, 0);

            var cadRasterImage = new CadRasterImage(rasterImage, originPt, xVec, yVec);
            // Create model

            var tvModel = new TvModel(_hclPrism.TvModelId);
            using var entityId = tvModel.AppendEntity("Tool" + resourceName);
            using var entity = entityId.openObject(OdTv_OpenMode.kForWrite);
            entity.appendRasterImage(rasterImageId, cadRasterImage.Origin, cadRasterImage.XVector, cadRasterImage.YVector);
            _hclPrism.AddVisibleEntity(VisibleEntityType.PrismImageEntity, entity.getDatabaseHandle());
            _hclPrism.ToolImageHandle = entity.getDatabaseHandle();
            _hclPrism.CadRasterImage = cadRasterImage;
            _hclPrism.ModelLifeCycle = ModelLifeCycle.Created;
        }
        public static double GetArrowLength(HPLReflector reflector, double lineFactor = 2)
    => lineFactor * reflector switch
    {
        HPLReflector.POA101 or HPLReflector.ReflectiveSticker => 2.73,
        HPLReflector.POA102 or HPLReflector.POA103 => 2.18,
        HPLReflector.CatEye360 => 2.27,
        HPLReflector.GlassPrism360 => 2.22,
        HPLReflector.CatEyePrism => 2.65,
        HPLReflector.WallPrism => 2.68,
        _ => 3
    };
    }
}
