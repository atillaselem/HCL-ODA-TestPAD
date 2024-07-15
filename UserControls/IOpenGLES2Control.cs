using HCL_ODA_TestPAD.ODA.Draggers;
using HCL_ODA_TestPAD.ODA.ModelBrowser;
using HCL_ODA_TestPAD.ViewModels.Base;
using ODA.Kernel.TD_RootIntegrated;
using ODA.Visualize.TV_Visualize;
using ODA.Visualize.TV_VisualizeTools;

namespace HCL_ODA_TestPAD.UserControls
{
    public interface IOpenGles2Control
    {
        void ShowCustomModels();
        void AddEntityToSet(OdTvEntityId enId);
        void AddBoldItem(TvTreeItem node);
        bool AddCuttingPlane(OdGeVector3d axis, OdTvResult rc);
        void RemoveCuttingPlanes();
        void ShowSectioningOptions();
        OdGeVector3d GetEyeDirection();
        OdTvSectioningOptions SectioningOptions { get; set; }
        bool ShowCuttingPlanes();
        void LoadFile(string filepath);
        OdTvDatabaseId TvDatabaseId { get; set; }
        bool AddDefaultViewOnLoad { get; set; }
        string FilePath { get; }
        void DrawCircMarkup();
        void DrawCloudMarkup();
        void DrawGeometry(string type);
        void DrawHandleMarkup();
        void DrawRectMarkup();
        void DrawTextMarkup();
        void ExportToPdf(string fileName, bool is2D = true);
        void FinishDragger();
        void LoadMarkup();
        void OnAppearSectioningPanel(bool bAppear);
        void OnOffAnimation(bool bEnable);
        void OnOffFPS(bool bEnable);
        void OnOffViewCube(bool bEnable);
        void OnOffWCS(bool bEnable);
        void Orbit();
        void Pan();
        void ZoomToArea(bool enable);
        void Regen();
        void Regen(OdTvGsDevice_RegenMode rm);
        void SaveFile(string filePath);
        void SaveMarkup();
        void Set3DView(OdTvExtendedView_e3DViewType type);
        void SetProjectionType(OdTvGsView_Projection projection);
        void SetRenderMode(OdTvGsView_RenderMode renderMode);
        void SetZoomStep(double dValue);
        void Zoom(ZoomType type);
        void ShowTool(HclToolType type);
        void SaveAsVsfx(string fileName);
    }
}