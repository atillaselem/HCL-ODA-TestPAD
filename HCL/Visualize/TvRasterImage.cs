using ODA.Visualize.TV_Visualize;
using System;
using System.Diagnostics.CodeAnalysis;

namespace HCL_ODA_TestPAD.HCL.Visualize
{
    public interface IRasterImage
    {
        uint PixelWidth();
        uint PixelHeight();
        T GetImplementation<T>() where T : class;
    }
    public sealed class TvRasterImage : IRasterImage
    {
        private OdTvRasterImageId _rasterImageId;

        public TvRasterImage([NotNull] OdTvRasterImageId rasterImageId)
        {
            _rasterImageId = rasterImageId;
        }
        public T GetImplementation<T>() where T : class
        {
            return _rasterImageId as T;
        }

        public uint PixelHeight()
        {
            using var rasterImage = _rasterImageId.openObject(OdTv_OpenMode.kForRead);
            var size = rasterImage.getSize();
            return (uint)Math.Ceiling(size.y);
        }

        public uint PixelWidth()
        {
            using var rasterImage = _rasterImageId.openObject(OdTv_OpenMode.kForRead);
            var size = rasterImage.getSize();
            return (uint)Math.Ceiling(size.x);
        }
        public void Dispose()
        {
            using var rasterImage = _rasterImageId.openObject(OdTv_OpenMode.kForRead);
            using var dbId = rasterImage.getDatabase();
            using var db = dbId.openObject(OdTv_OpenMode.kForWrite);
            db.removeRasterImage(_rasterImageId);
            _rasterImageId.Dispose();
            _rasterImageId = null;
        }
    }
}
