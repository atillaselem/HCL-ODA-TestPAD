using HCL_ODA_TestPAD.ODA.Draggers;
using HCL_ODA_TestPAD.ODA.ModelBrowser;
using HCL_ODA_TestPAD.ViewModels;
using Teigha.Visualize;

namespace HCL_ODA_TestPAD.ViewModels.Base;

public interface IOdaSectioning
{
    OdTvExtendedView GetActiveTvExtendedView();
    OdTvSectioningOptions SectioningOptions { get; }
    OdTvGsViewId CuttingPlanesViewId { get; }
    OdTvModelId CuttingPlaneModelId { get; }
    TvDatabaseInfo DatabaseInfo { get; set; }
    OdTvGsDeviceId TvDeviceId { get; set; }
    double Width { get; set; }
    double Height { get; set; }
    MainWindowViewModel VM { get; set; }
    void ApplySectioningOptions();
    void SetAnimation(OdTvAnimation odTvAnimation);
    void UpdateCadView(bool invalidate = false);
    void ClearSelectedNodes();
    void AddEntityToSet(OdTvEntityId id);
}
