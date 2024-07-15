using ODA.Kernel.TD_RootIntegrated;
using ODA.Visualize.TV_Visualize;
using System;

namespace HCL_ODA_TestPAD.HCL.Visualize
{
    public enum HighlightStyle
    {
        Red,
        Point
    }
    public class TvStyle
    {
        private readonly OdTvDatabase _database;

        public TvStyle(OdTvDatabase database)
        {
            _database = database;
        }

        public void CreateLineStyle()
        {
            using var shortDash = OdTvLinetypeDashElement.createObject(1);
            var ltShortDash = new OdTvLinetypeElementPtr(shortDash, OdRxObjMod.kOdRxObjAttach);
            using var shortSpace = OdTvLinetypeSpaceElement.createObject(1);
            var ltShortSpace = new OdTvLinetypeElementPtr(shortSpace, OdRxObjMod.kOdRxObjAttach);
            using var ltShortArr = new OdTvLinetypeElementArray() { ltShortDash, ltShortSpace };
            _database.createLinetype(CadModelConstants.ShortDash, ltShortArr);

            using var dash = OdTvLinetypeDashElement.createObject(1);
            var ltDash = new OdTvLinetypeElementPtr(dash, OdRxObjMod.kOdRxObjAttach);
            using var space = OdTvLinetypeSpaceElement.createObject(2);
            var ltSpace = new OdTvLinetypeElementPtr(space, OdRxObjMod.kOdRxObjAttach);
            using var ltArr = new OdTvLinetypeElementArray() { ltDash, ltSpace };
            _database.createLinetype(CadModelConstants.DashedStyle, ltArr);

            using var midDash = OdTvLinetypeDashElement.createObject(5);
            var midLtDash = new OdTvLinetypeElementPtr(midDash, OdRxObjMod.kOdRxObjAttach);
            using var midSpace = OdTvLinetypeSpaceElement.createObject(3);
            var midLtSpace = new OdTvLinetypeElementPtr(midSpace, OdRxObjMod.kOdRxObjAttach);
            using var midLtArr = new OdTvLinetypeElementArray() { midLtDash, midLtSpace };
            _database.createLinetype(CadModelConstants.MidiumDashedStyle, midLtArr);

            //Suppress finalize for ltDash and ltSpace because it is used in ltArr and its continuously looking for this object
            //Else we are getting unprotected memory access exception for ltArr
#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
            GC.SuppressFinalize(ltShortDash);
            GC.SuppressFinalize(ltShortSpace);
            GC.SuppressFinalize(ltDash);
            GC.SuppressFinalize(ltSpace);
            GC.SuppressFinalize(midLtDash);
            GC.SuppressFinalize(midLtSpace);
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
        }

        public void CreateHighlightStyle(HighlightStyle styleType)
        {
            var styleId = _database.createHighlightStyle(styleType.ToString());
            using var highlightStyle = styleId.openObject(OdTv_OpenMode.kForWrite);

            using var color = new OdTvRGBColorDef(CadModelConstants.HighlightColor.R, CadModelConstants.HighlightColor.G, CadModelConstants.HighlightColor.B);
            using var faceColor = new OdTvRGBColorDef(CadModelConstants.FaceHighlightColor.R, CadModelConstants.FaceHighlightColor.G, CadModelConstants.FaceHighlightColor.B);
            var lineWeight = styleType == HighlightStyle.Red ? LineWeightType.BoldS : LineWeightType.Normal;
            const uint Entries = (uint)OdTvHighlightStyle_Entry.k2D | (uint)OdTvHighlightStyle_Entry.k3D;

            highlightStyle.setEdgesColor(Entries, color);
            highlightStyle.setEdgesLineweight(Entries, (byte)lineWeight);
            highlightStyle.setEdgesVisibility(Entries, true);
            highlightStyle.setEdgesTransparency(Entries, 1);
            highlightStyle.setFacesColor(Entries, styleType == HighlightStyle.Point ? color : faceColor);
            highlightStyle.setFacesVisibility(Entries, true);
            highlightStyle.setFacesTransparency(Entries, styleType == HighlightStyle.Point ? (byte)1 : (byte)CadModelConstants.FaceTransparency);
        }

        public void CreateViewBackgroundStyle()
        {
            using var viewBackgroundGradientId = _database.createBackground(CadModelConstants.GradientBackground, OdTvGsViewBackgroundId_BackgroundTypes.kGradient);
            using var viewBackgroundGradient = viewBackgroundGradientId.openAsGradientBackground(OdTv_OpenMode.kForWrite);

            using var colorDefTop = new OdTvColorDef(CadModelConstants.GradientColorTop.R,
                                                   CadModelConstants.GradientColorTop.G,
                                                   CadModelConstants.GradientColorTop.B);

            using var colorDefMiddle = new OdTvColorDef(CadModelConstants.GradientColorMiddle.R,
                                                   CadModelConstants.GradientColorMiddle.G,
                                                   CadModelConstants.GradientColorMiddle.B);

            using var colorDefBottom = new OdTvColorDef(CadModelConstants.GradientColorBottom.R,
                                                   CadModelConstants.GradientColorBottom.G,
                                                   CadModelConstants.GradientColorBottom.B);

            viewBackgroundGradient.setColorTop(colorDefTop);
            viewBackgroundGradient.setColorMiddle(colorDefMiddle);
            viewBackgroundGradient.setColorBottom(colorDefBottom);

            using var viewBackgroundSolidId = _database.createBackground(CadModelConstants.SolidBackground, OdTvGsViewBackgroundId_BackgroundTypes.kSolid);
            using var viewBackgroundSolid = viewBackgroundSolidId.openAsSolidBackground(OdTv_OpenMode.kForWrite);
            using var lightColorDef = new OdTvColorDef(CadModelConstants.LightColor.R, CadModelConstants.LightColor.G,
                                                  CadModelConstants.LightColor.B);

            viewBackgroundSolid.setColorSolid(lightColorDef);
        }
    }
}
