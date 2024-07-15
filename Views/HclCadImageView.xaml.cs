using HCL_ODA_TestPAD.Settings;
using HCL_ODA_TestPAD.ViewModels;
using HCL_ODA_TestPAD.ViewModels.Base;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using HCL_ODA_TestPAD.HCL.UserActions.States;
using ODA.Visualize.TV_Visualize;
using ODA.Visualize.TV_VisualizeTools;

namespace HCL_ODA_TestPAD.Views
{
    public partial class HclCadImageView : ICadImageViewControl, ICadOdaMenuView
    {
        public HclCadImageViewModel Vm { get; set; }
        public Func<AppSettings> AppSettingsFactory { get; set; }
        public HclCadImageView()
        {
            InitializeComponent();
            //Loaded += UserControl_Loaded;
            IsVisibleChanged += VisibilityChanged;
        }
        //private void UserControl_Loaded(object sender, RoutedEventArgs e)
        //{
        //    //VM.ShowFPS();
        //    //VM.ShowWCS();
        //}

        private void VisibilityChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var visible = (bool)e.NewValue;
            if (visible && Vm == null)
            {
                Vm = DataContext as HclCadImageViewModel;
                if (Vm != null)
                {
                    //VM.AddDefaultViewOnLoad = true;
                    Vm.CadOdaMenuView = this;
                    Vm.ViewControl = this;
                    Vm.InitViewModel();
                }
                Vm?.VisibilityChanged((bool)e.NewValue);
                Vm?.LoadFile(Vm.CadImageFilePath, new System.Threading.CancellationToken());
                Vm?.ShowCustomModels();
            }
        }
        public void InvalidateControl()
        {
            InvalidateVisual();
        }
        public void SetImageSource(WriteableBitmap writableBitmap)
        {
            CadWritableImage.Source = writableBitmap;
        }
        protected override void OnRender(DrawingContext drawingContext)
        {
            Vm.Update();
        }
        public void SetFileLoaded(bool isFileLoaded, string filePath, Action<string> emitEvent, bool isCancelled = false)
        {
            if (isFileLoaded)
            {
                emitEvent?.Invoke($"File : [{filePath}] loaded successfully.");
            }
            else
            {
                emitEvent?.Invoke($"File : {(isCancelled ? "cancelled" : "unloaded")}.");
            }
        }

        #region Update GL_Device & Image Buffer

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            Vm.RenderSizeChanged(sizeInfo);
        }
        #endregion
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            Vm.MouseDown(e, e.GetPosition(this));
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            Vm.MouseMove(e, e.GetPosition(this), UserInteraction.Idle);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            Vm.MouseUp(e);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            Vm.MouseWheel(e);
        }
        public void Pan()
        {
            Vm.Pan();
        }

        public void Orbit()
        {
            Vm.Orbit();
        }

        public void SetZoom(ZoomType type)
        {
            Vm.Zoom(type);
        }

        public void Set3DView(OdTvExtendedView_e3DViewType type)
        {
            Vm.Set3DView(type);
        }

        public void SetRenderMode(OdTvGsView_RenderMode renderMode)
        {
            Vm.SetRenderMode(renderMode);
        }

        public void SetProjectionType(OdTvGsView_Projection projection)
        {
            Vm.SetProjectionType(projection);
        }

        public void Regen(OdTvGsDevice_RegenMode rm)
        {
            Vm.Regen(rm);
        }
        public void RegenView()
        {
            Vm.Regen();
        }
    }

}
