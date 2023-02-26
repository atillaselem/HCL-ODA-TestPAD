using System.Collections.ObjectModel;

namespace HCL_ODA_TestPAD.ViewModels
{
    public class TreeViewModel
    {
        public string Name { get; set; }
        public bool IsBold { get; set; }
        public string XmlElementTooltipText { get; set; }
        public int XmlLineNumber { get; set; }
        public TreeViewModel Parent { get; set; }
        public ObservableCollection<TreeViewModel> Children { get; set; } = new ObservableCollection<TreeViewModel>();
    }
}