using HCL_ODA_TestPAD.HCL.CAD.Math.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using ODA.Kernel.TD_RootIntegrated;

namespace HCL_ODA_TestPAD.HCL.Visualize
{
    public sealed class CadText : CadEntity, IDisposable
    {
        public CadText(CadPoint3D position, double textScale, double textSize, string text, Color color)
        {
            Position = CadPoint3D.With(position);
            TextSize = textSize;
            TextScale = textScale;
            Text = text;
            Color = color;
        }

        public CadPoint3D Position { get; }
        public string Text { get; }
        public double TextScale { get; }
        public double TextSize { get; }

        public void Dispose()
        {
            Position?.Dispose();
        }
    }
}
