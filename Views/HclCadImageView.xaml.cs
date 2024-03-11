using HCL_ODA_TestPAD.Settings;
using HCL_ODA_TestPAD.ViewModels;
using HCL_ODA_TestPAD.ViewModels.Base;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ODA.Visualize.TV_Visualize;
using ODA.Visualize.TV_VisualizeTools;

namespace HCL_ODA_TestPAD.Views
{
    public partial class HclCadImageView : ICadImageViewControl, ICadOdaMenuView
    {
        public HclCadImageViewModel VM { get; set; }
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
            if (visible && VM == null)
            {
                VM = DataContext as HclCadImageViewModel;
                if (VM != null)
                {
                    //VM.AddDefaultViewOnLoad = true;
                    VM.CadOdaMenuView = this;
                    VM.ViewControl = this;
                    VM.InitViewModel();
                }
                VM?.VisibilityChanged((bool)e.NewValue);
                VM.LoadFile(VM.CadImageFilePath);
                VM.ShowCustomModels();
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
            VM.Update();
        }
        public void SetFileLoaded(bool isFileLoaded, string filePath, Action<string> emitEvent)
        {
            if (isFileLoaded)
            {
                emitEvent?.Invoke($"File : [{filePath}] loaded successfully.");
            }
            else
            {
                emitEvent?.Invoke($"File : [{filePath}] unloaded.");
            }
        }

        #region Update GL_Device & Image Buffer

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            VM.RenderSizeChanged(sizeInfo);
        }
        #endregion
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            VM.MouseDown(e, e.GetPosition(this));
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            VM.MouseMove(e, e.GetPosition(this));
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            VM.MouseUp(e);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            VM.MouseWheel(e);
        }
        public void Pan()
        {
            VM.Pan();
        }

        public void Orbit()
        {
            VM.Orbit();
        }

        public void SetZoom(ZoomType type)
        {
            VM.Zoom(type);
        }

        public void Set3DView(OdTvExtendedView_e3DViewType type)
        {
            VM.Set3DView(type);
        }

        public void SetRenderMode(OdTvGsView_RenderMode renderMode)
        {
            VM.SetRenderMode(renderMode);
        }

        public void SetProjectionType(OdTvGsView_Projection projection)
        {
            VM.SetProjectionType(projection);
        }

        public void Regen(OdTvGsDevice_RegenMode rm)
        {
            VM.Regen(rm);
        }
        public void RegenView()
        {
            VM.Regen();
        }
    }

}
