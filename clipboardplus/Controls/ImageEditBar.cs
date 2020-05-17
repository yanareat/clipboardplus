using clipboardplus.Models;
using System.Windows;
using System.Windows.Controls;

namespace clipboardplus.Controls
{
    public class ImageEditBar : Control
    {
        static ImageEditBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ImageEditBar), new FrameworkPropertyMetadata(typeof(ImageEditBar)));
        }

        public ImageEditBar()
        {
            _Current = this;
        }

        #region 属性 Current
        private static ImageEditBar _Current = null;
        public static ImageEditBar Current
        {
            get
            {
                return _Current;
            }
            set
            {
                _Current = value;
            }
        }

        private ToolButton _currentTool;
        public ToolButton CurrentTool
        {
            get
            {
                return _currentTool;
            }
            set
            {
                _currentTool = value;
            }
        }
        #endregion

        #region 定位
        public void ResetCanvas()
        {
            ResetCanvasLeft();
            ResetCanvasTop();
        }

        public void ControlResetCanvas()
        {
            ControlResetCanvasLeft();
            ControlResetCanvasTop();
        }
        #endregion

        #region CanvasLeft DependencyProperty
        public double CanvasLeft
        {
            get { return (double)GetValue(CanvasLeftProperty); }
            set { SetValue(CanvasLeftProperty, value); }
        }
        public static readonly DependencyProperty CanvasLeftProperty =
                DependencyProperty.Register("CanvasLeft", typeof(double), typeof(ImageEditBar),
                new PropertyMetadata(0.0, new PropertyChangedCallback(ImageEditBar.OnCanvasLeftPropertyChanged)));

        private static void OnCanvasLeftPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is ImageEditBar)
            {
                (obj as ImageEditBar).OnCanvasLeftValueChanged();
            }
        }

        protected void OnCanvasLeftValueChanged()
        {

        }

        private void ResetCanvasLeft()
        {
            CanvasLeft = AppModel.Current.MaskRightWidth > YanaImageEditor.ScreenWidth - Width ? 0 : YanaImageEditor.ScreenWidth - AppModel.Current.MaskRightWidth - Width;
        }

        private void ControlResetCanvasLeft()
        {
            CanvasLeft = AppModel.Current.MaskRightWidth > YanaImageEditor.Current.MainCanvas.ActualWidth - Width ? 0 : YanaImageEditor.Current.MainCanvas.ActualWidth - AppModel.Current.MaskRightWidth - Width;
        }
        #endregion

        #region CanvasTop DependencyProperty
        public double CanvasTop
        {
            get { return (double)GetValue(CanvasTopProperty); }
            set { SetValue(CanvasTopProperty, value); }
        }
        public static readonly DependencyProperty CanvasTopProperty =
                DependencyProperty.Register("CanvasTop", typeof(double), typeof(ImageEditBar),
                new PropertyMetadata(0.0, new PropertyChangedCallback(ImageEditBar.OnCanvasTopPropertyChanged)));

        private static void OnCanvasTopPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is ImageEditBar)
            {
                (obj as ImageEditBar).OnCanvasTopValueChanged();
            }
        }

        protected void OnCanvasTopValueChanged()
        {

        }

        private void ResetCanvasTop()
        {
            var need = 30 + (SizeColorBar.Current.Selected == Tool.Null ? 0 : 37);
            CanvasTop = AppModel.Current.MaskBottomHeight >= need ? YanaImageEditor.ScreenHeight - AppModel.Current.MaskBottomHeight + 5
                : AppModel.Current.MaskTopHeight >= need ? AppModel.Current.MaskTopHeight - 30
                : AppModel.Current.MaskTopHeight;
        }

        private void ControlResetCanvasTop()
        {
            var need = 30 + (SizeColorBar.Current.Selected == Tool.Null ? 0 : 37);
            CanvasTop = AppModel.Current.MaskBottomHeight >= need ? YanaImageEditor.Current.MainCanvas.ActualHeight - AppModel.Current.MaskBottomHeight + 5
                : AppModel.Current.MaskTopHeight >= need ? AppModel.Current.MaskTopHeight - 30
                : AppModel.Current.MaskTopHeight;
        }
        #endregion

    }
}
