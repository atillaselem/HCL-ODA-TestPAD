using HCL_ODA_TestPAD.HCL.CAD.Math.API;
using HCL_ODA_TestPAD.HCL.Visualize.Extensions;
using HCL_ODA_TestPAD.ViewModels.Base;
using ODA.Visualize.TV_Visualize;

namespace HCL_ODA_TestPAD.HCL.Visualize
{
    public class HclStationBuilder
    {
        private static HclStation _hclStation;
        private static IHclTooling _hclTooling;
        public static HclStation ShowPltStation(IHclTooling hclViewModel, CadPoint3D location)
        {
            _hclTooling = hclViewModel;
            if (_hclStation != null)
            {
                _hclStation.Remove();
                _hclStation = null;
                return _hclStation;
            }
            _hclStation = BuildModel("station-plt");
            //OK : Location Updates to click point
            _hclStation.UpdateLocation(location);
            //OK : Orientation Updates to camera even rotated view
            _hclStation.UpdateOrientation();
            
            return _hclStation;
        }
        public static void Dispose(IHclTooling hclViewModel)
        {
            if (_hclStation != null)
            {
                _hclStation.Remove();
                _hclStation = null;
            }
        }
        private static HclStation BuildModel(string resourceName)
        {
            _hclStation ??= new HclStation(_hclTooling);

            var imageFilePath = RasterImageHelper.GetResourceFilePath(resourceName);
            using var tvDatabase = _hclTooling.TvDatabaseId.openObject(OdTv_OpenMode.kForWrite);
            var rasterImageId = tvDatabase.createRasterImage(System.IO.Path.GetFileNameWithoutExtension(imageFilePath), imageFilePath);
            var rasterImage = new TvRasterImage(rasterImageId);

            using var odTvGsViewId = _hclTooling.GetViewId();
            var pixel = odTvGsViewId.GetPixelScaleFactorAtViewTarget();

            using var tvImage = rasterImageId.openObject(OdTv_OpenMode.kForWrite);
            if (!tvImage.isLoaded())
            {
                tvImage.load();
            }

            const double mapFactor = 1.5;
            var width = pixel * rasterImage.PixelWidth() * mapFactor;
            var height = pixel * rasterImage.PixelHeight() * mapFactor;

            
            using var upVector = CadVector3D.With(0, 1, 0);
            using var xAxis = CadVector3D.With(1, 0, 0);
            using var xVec = xAxis * width;
            using var yVec = upVector * height;
            
            //OK : No Z-Axis shift
            using var originPt = CadPoint3D.With(-width / 2, -height / 2, 0);

            var cadRasterImage = new CadRasterImage(rasterImage, originPt, xVec, yVec);
            // Create model
            var modelId = tvDatabase.createModel("Tv_Model_Station", OdTvModel_Type.kDirect);
            using var odTvGsView = odTvGsViewId.openObject(OdTv_OpenMode.kForWrite);
            odTvGsView.addModel(modelId);

            var tvModel = new TvModel(modelId);
            using var entityId = tvModel.AppendEntity("Tool" + resourceName);
            using var entity = entityId.openObject(OdTv_OpenMode.kForWrite);
            using var entityGeometryDataId = entity.appendRasterImage(rasterImageId, cadRasterImage.Origin, cadRasterImage.XVector, cadRasterImage.YVector);
            _hclStation.TvModelId = modelId;
            _hclStation.ToolImageHandle = entity.getDatabaseHandle();
            _hclStation.CadRasterImage = cadRasterImage;
            _hclStation.ModelLifeCycle = ModelLifeCycle.Created;

            return _hclStation;
        }
        //private HclStation UpdateModel(ref OdTvModelId modelId, string resourceName, CadPoint3D location, ref ulong toolImageHandle)
    }

}
