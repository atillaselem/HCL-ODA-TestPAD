using HCL_ODA_TestPAD.ViewModels;
using HCL_ODA_TestPAD.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace HCL_ODA_TestPAD.Views
{
    /// <summary>
    /// Interaction logic for OdaMenuView.xaml
    /// </summary>
    public interface IOdaMenuView
    {
        void PlayNavRectAnimation(int right);
    }
    public partial class OdaMenuView : UserControl, IOdaMenuView
    {
        private OdaMenuViewModel ViewModel { get; set; }
        public OdaMenuView()
        {
            InitializeComponent();
            CreateContextMenu();
        }

        private void CreateContextMenu()
        {
            ContextMenu menu = new ContextMenu()
            {
                Background = new SolidColorBrush(Colors.White)
            };

            MenuItem openItm = new MenuItem() { Header = "Open", Height = 30 };
            openItm.Icon = new Image
            {
                Source = Application.Current.Resources["OpenImg"] as BitmapImage
            };
            menu.Items.Add(openItm);


            menu.Items.Add(new Separator());

            MenuItem exitItm = new MenuItem() { Header = "Exit", Height = 30 };
            menu.Items.Add(exitItm);

            DropMenuBtn.DropDown = menu;
            DropMenuBtn.DropDown.Opened += DropDown_Opened;
        }

        private void DropDown_Opened(object sender, RoutedEventArgs e)
        {
            ViewModel = DataContext as OdaMenuViewModel;
            if (ViewModel.OdaMenuView == null)
            {
                ViewModel.OdaMenuView = this;
                ViewModel.ButtonFactory = new Dictionary<ButtonName, Func<ToggleButton>>
                {
                    { ButtonName.PanBtn, () => PanBtn },
                    { ButtonName.OrbitBtn, () => OrbitBtn },
                    { ButtonName.IsometricBtn, () => IsometricBtn },
                    { ButtonName.PerspectiveBtn, () => PerspectiveBtn },
                    { ButtonName.Wireframe2DBtn, () => Wireframe2DBtn },
                    { ButtonName.Wireframe3DBtn, () => Wireframe3DBtn },
                    { ButtonName.HiddenLineBtn, () => HiddenLineBtn },
                    { ButtonName.ShadedBtn, () => ShadedBtn },
                    { ButtonName.ShadedWithEdgesBtn, () => ShadedWithEdgesBtn }
                };
            }
            else return;
            var menu = sender as ContextMenu;
            var menuItem = menu.Items[0] as MenuItem;
            if (menuItem.Header.ToString() == "Open" && menuItem.Command is null)
            {
                menuItem.Command = ViewModel.OpenCommand;
            }
            menuItem = menu.Items[2] as MenuItem;
            if (menuItem.Header.ToString() == "Exit" && menuItem.Command is null)
            {
                menuItem.Command = ViewModel.ExitCommand;
            }
        }
        public void PlayNavRectAnimation(int right)
        {
            var animation = new ThicknessAnimation();
            animation.From = ActiveRect.Margin;
            animation.To = new Thickness(ActiveRect.Margin.Left, ActiveRect.Margin.Top, right, ActiveRect.Margin.Bottom);
            animation.Duration = TimeSpan.FromSeconds(0.15);
            ActiveRect.BeginAnimation(FrameworkElement.MarginProperty, animation);
        }

    }
}
