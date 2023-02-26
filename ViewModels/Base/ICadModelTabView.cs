using System.Threading.Tasks;

namespace HCL_ODA_TestPAD.ViewModels.Base
{
    public interface ICadImageTabViewModel
    {
        Task LoadCadModelViewAsync();
        string TabItemTitle { get; set; }
        void OnPanClicked();
        void OnOrbitClicked();
        void OnZoomInClicked();
        void OnZoomOutClicked();
        void OnZoomExtentClicked();
        void CloseTabView();
    }
}
