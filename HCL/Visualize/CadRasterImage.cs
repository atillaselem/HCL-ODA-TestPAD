using HCL_ODA_TestPAD.HCL.CAD.Math.API;
using System;

namespace HCL_ODA_TestPAD.HCL.Visualize
{
    public sealed class CadRasterImage : CadEntity, IDisposable
    {
        public CadRasterImage(TvRasterImage tvRasterImage, CadPoint3D origin, CadVector3D u, CadVector3D v)
        {
            ImageId = tvRasterImage;
            Origin = CadPoint3D.With(origin);
            XVector = CadVector3D.With(u);
            YVector = CadVector3D.With(v);
        }

        public TvRasterImage ImageId { internal set; get; }
        public CadPoint3D Origin { internal set; get; }
        public CadVector3D XVector { internal set; get; }
        public CadVector3D YVector { internal set; get; }

        public void Dispose()
        {
            Origin?.Dispose();
            XVector?.Dispose();
            YVector?.Dispose();
            ImageId?.Dispose();
        }
    }
}
