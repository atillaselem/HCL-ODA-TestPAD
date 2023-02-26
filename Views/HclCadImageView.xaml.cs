using HCL_ODA_TestPAD.ViewModels;
using HCL_ODA_TestPAD.ViewModels.Base;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Teigha.Visualize;

namespace HCL_ODA_TestPAD.Views
{
    public interface ICadOdaMenuView
    {
        void Pan();
        void Orbit();
        void Set3DView(OdTvExtendedView.e3DViewType type);
        void SetRenderMode(OdTvGsView.RenderMode renderMode);
        void SetProjectionType(OdTvGsView.Projection projection);
        void Regen(OdTvGsDevice.RegenMode rm);
    }
    public partial class HclCadImageView : ICadImageViewControl, ICadOdaMenuView
    {
        public HclCadImageViewModel VM { get; set; }

        public HclCadImageView()
        {
            InitializeComponent();
            Loaded += HclCadImageView_Loaded;
            IsVisibleChanged += VisibilityChanged;
        }
        private void HclCadImageView_Loaded(object sender, RoutedEventArgs e)
        {
            VM.LoadFile(VM.CadImageFilePath);
        }
        private void SetControlViewModelLink()
        {
            VM = DataContext as HclCadImageViewModel;
            if (VM != null)
            {
                VM.CadOdaMenuView = this;
                VM.ViewControl = this;
                VM.InitViewModel();                
            }
        }
        private void VisibilityChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SetControlViewModelLink();
            VM?.VisibilityChanged((bool)e.NewValue);
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
        public void SetFileLoaded(bool isFileLoaded, string filePath)
        {
            //VM.FileIsExist = isFileLoaded;
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

        public void Set3DView(OdTvExtendedView.e3DViewType type)
        {
            throw new NotImplementedException();
        }

        public void SetRenderMode(OdTvGsView.RenderMode renderMode)
        {
            throw new NotImplementedException();
        }

        public void SetProjectionType(OdTvGsView.Projection projection)
        {
            throw new NotImplementedException();
        }

        public void Regen(OdTvGsDevice.RegenMode rm)
        {
            throw new NotImplementedException();
        }
        public void SetRenderModeButton(OdTvGsView.RenderMode mode)
        {
            //VM.SetRenderModeButton(mode);
        }
    }

}
