using clipboardplus.Helpers;
using clipboardplus.Models;
using clipboardplus.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace clipboardplus.Controls
{
    /// <summary>
    /// YanaImageEditor.xaml 的交互逻辑
    /// </summary>
    public partial class YanaImageEditor : UserControl
    {
        public YanaImageEditor()
        {
            InitializeComponent();
            _Current = this;
            DataContext = new AppModel();
            WpfHelper.MainDispatcher = Dispatcher;
            //SetImage();
            //计算Windows项目缩放比例
            ScreenHelper.ResetScreenScale();
        }

        public static double ScreenWidth = SystemParameters.PrimaryScreenWidth;
        public static double ScreenHeight = SystemParameters.PrimaryScreenHeight;
        public static double ScreenScale = 1;
        public static int MinSize = 10;

        //画图注册名称集合
        public List<NameAndLimit> list = new List<NameAndLimit>();
        //画图注册名称
        public int num = 1;

        //是否截图开始
        private bool _IsMouseDown = false;
        //是否截图完毕
        private bool _IsCapture = false;
        //是否编辑
        private bool _IsEdit = false;

        private double _X0 = 0;
        private double _Y0 = 0;

        #region 属性 Current
        private static YanaImageEditor _Current = null;
        public static YanaImageEditor Current
        {
            get
            {
                return _Current;
            }
        }
        #endregion

        #region 全屏+置顶
        private void MaxWindow()
        {
            //Left = 0;
            //Top = 0;
            //Width = ScreenWidth;
            //Height = ScreenHeight;
            //Activate();
        }
        #endregion

        #region 设置图片
        public void SetImage()
        {
            //var tempImage = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/content_0.jpg"));
            //Background = new ImageBrush(Helpers.ImageHelper.GetBitmapImage(ToolUtil.ConvertToBytes(tempImage)));
            //Background = new ImageBrush(Helpers.ImageHelper.GetFullBitmapSource());
        }
        #endregion

        #region 注册画图
        public static void Register(object control)
        {
            var name = "Draw" + _Current.num;
            _Current.MainCanvas.RegisterName(name, control);
            _Current.list.Add(new NameAndLimit(name));
            _Current.num++;
        }
        #endregion

        #region 截图区域添加画图
        public static void AddControl(UIElement e)
        {
            _Current.MainCanvas.Children.Add(e);
        }
        #endregion

        #region 截图区域撤回画图
        public static void RemoveControl(UIElement e)
        {
            _Current.MainCanvas.Children.Remove(e);
        }
        #endregion

        #region 撤销
        public void OnRevoke()
        {
            if (list.Count > 0)
            {
                var name = list[list.Count - 1].Name;
                var obj = MainCanvas.FindName(name);
                if (obj != null)
                {
                    MainCanvas.Children.Remove(obj as UIElement);
                    MainCanvas.UnregisterName(name);
                    list.RemoveAt(list.Count - 1);
                    MainImage.Limit = list.Count == 0 ? new Limit() : list[list.Count - 1].Limit;
                }
            }
        }
        #endregion

        #region 保存
        public void OnSave()
        {
            var sfd = new Microsoft.Win32.SaveFileDialog
            {
                FileName = "截图" + DateTime.Now.ToString("yyyyMMddhhmmss"),
                Filter = "png|*.png",
                AddExtension = true,
                RestoreDirectory = true
            };
            if (sfd.ShowDialog() == true)
            {
                Hidden();
                Thread t = new Thread(new ThreadStart(() =>
                {
                    Thread.Sleep(200);
                    WpfHelper.SafeRun(() =>
                    {
                        var source = GetCapture();
                        if (source != null)
                        {
                            Helpers.ImageHelper.SaveToPng(source, sfd.FileName);
                        }
                        //Close();
                    });
                }))
                {
                    IsBackground = true
                };
                t.Start();
            }
        }
        #endregion

        #region 获取截图
        private BitmapSource OriginGetCapture()
        {
            System.Windows.Point point = imageBrowser.PointToScreen(new System.Windows.Point(0, 0));
            return Helpers.ImageHelper.SetBitmapSource((int)(AppModel.Current.MaskLeftWidth + point.X + 1), (int)(AppModel.Current.MaskTopHeight + point.Y + 1), (int)MainImage.ActualWidth - 2, (int)MainImage.ActualHeight - 2);
        }

        private BitmapSource GetCapture()
        {
            return ToolUtil.GetPartImage(ToolUtil.ConvertToBitmapImage(MainCanvas), (int)(AppModel.Current.MaskLeftWidth+1), (int)(AppModel.Current.MaskTopHeight+1), (int)MainImage.ActualWidth - 2, (int)MainImage.ActualHeight - 2);
        }
        #endregion

        #region 退出截图
        public void OnCancel()
        {
            //Close();
        }
        #endregion

        #region 完成截图
        public void OnOK()
        {
            Hidden();
            Thread t = new Thread(new ThreadStart(() =>
            {
                Thread.Sleep(50);
                WpfHelper.SafeRun(() =>
                {
                    var source = GetCapture();
                    if (source != null)
                    {
                        Clipboard.SetImage(source);
                    }
                    //Close();
                });
            }))
            {
                IsBackground = true
            };
            t.Start();
        }
        #endregion

        #region 截图前隐藏窗口
        private void Hidden()
        {
            //隐藏尺寸RGB框
            if (AppModel.Current.MaskTopHeight < 40)
            {
                SizeRGB.Visibility = Visibility.Collapsed;
            }
            var need = SizeColorBar.Current.Selected == Tool.Null ? 30 : 67;
            if (AppModel.Current.MaskBottomHeight < need && AppModel.Current.MaskTopHeight < need)
            {
                ImageEditBar.Current.Visibility = Visibility.Collapsed;
                SizeColorBar.Current.Visibility = Visibility.Collapsed;
            }
            MainImage.ZoomThumbVisibility = Visibility.Collapsed;
        }
        #endregion

        #region 鼠标及键盘事件
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_IsCapture)
            {
                return;
            }
            var point = e.GetPosition(this);
            _X0 = point.X;
            _Y0 = point.Y;
            _IsMouseDown = true;
            Canvas.SetLeft(MainImage, _X0);
            Canvas.SetTop(MainImage, _Y0);
            AppModel.Current.MaskLeftWidth = _X0;
            AppModel.Current.MaskRightWidth = ScreenWidth - _X0;
            AppModel.Current.MaskTopHeight = _Y0;
            Show_Size.Visibility = Visibility.Visible;
        }

        private void Control_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_IsCapture)
            {
                return;
            }
            var point = e.GetPosition(MainCanvas);
            _X0 = point.X;
            _Y0 = point.Y;
            _IsMouseDown = true;
            Canvas.SetLeft(MainImage, _X0);
            Canvas.SetTop(MainImage, _Y0);
            AppModel.Current.MaskLeftWidth = _X0;
            AppModel.Current.MaskRightWidth = MainCanvas.ActualWidth - _X0;
            AppModel.Current.MaskTopHeight = _Y0;
            Show_Size.Visibility = Visibility.Visible;
        }

        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_IsMouseDown || _IsCapture)
            {
                return;
            }
            _IsMouseDown = false;
            if (MainImage.Width >= MinSize && MainImage.Height >= MinSize)
            {
                _IsCapture = true;
                ImageEditBar.Current.Visibility = Visibility.Visible;
                ImageEditBar.Current.ControlResetCanvas();
                Cursor = Cursors.Arrow;
            }
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            var point = e.GetPosition(this);
            var screenP = PointToScreen(point);
            AppModel.Current.ShowRGB = Helpers.ImageHelper.GetRGB((int)screenP.X, (int)screenP.Y);
            if (_IsCapture)
            {
                return;
            }

            if (Show_RGB.Visibility == Visibility.Collapsed)
            {
                Show_RGB.Visibility = Visibility.Visible;
            }

            if (_IsMouseDown)
            {
                var w = point.X - _X0;
                var h = point.Y - _Y0;
                if (w < MinSize || h < MinSize)
                {
                    return;
                }
                if (MainImage.Visibility == Visibility.Collapsed)
                {
                    MainImage.Visibility = Visibility.Visible;
                }
                AppModel.Current.MaskRightWidth = ScreenWidth - point.X;
                AppModel.Current.MaskTopWidth = w;
                AppModel.Current.MaskBottomHeight = ScreenHeight - point.Y;
                AppModel.Current.ChangeShowSize();
                MainImage.Width = w;
                MainImage.Height = h;
            }
            else
            {
                AppModel.Current.ShowSizeLeft = point.X;
                AppModel.Current.ShowSizeTop = ScreenHeight - point.Y < 30 ? point.Y - 30 : point.Y + 10;
            }
        }

        private void Control_MouseMove(object sender, MouseEventArgs e)
        {
            if (_IsEdit)
            {
                var point = e.GetPosition(MainCanvas);
                var screenP = point;
                AppModel.Current.ShowRGB = Helpers.ImageHelper.GetRGB((int)screenP.X, (int)screenP.Y);
                if (_IsCapture)
                {
                    return;
                }

                if (Show_RGB.Visibility == Visibility.Collapsed)
                {
                    Show_RGB.Visibility = Visibility.Visible;
                }

                if (_IsMouseDown)
                {
                    var w = point.X - _X0;
                    var h = point.Y - _Y0;
                    if (w < MinSize || h < MinSize)
                    {
                        return;
                    }
                    if (MainImage.Visibility == Visibility.Collapsed)
                    {
                        MainImage.Visibility = Visibility.Visible;
                    }
                    AppModel.Current.MaskRightWidth = MainCanvas.ActualWidth - point.X;
                    AppModel.Current.MaskTopWidth = w;
                    AppModel.Current.MaskBottomHeight = MainCanvas.ActualHeight - point.Y;
                    AppModel.Current.ChangeShowSize();
                    MainImage.Width = w;
                    MainImage.Height = h;
                }
                else
                {
                    AppModel.Current.ShowSizeLeft = point.X;
                    AppModel.Current.ShowSizeTop = MainCanvas.ActualHeight - point.Y < 30 ? point.Y - 30 : point.Y + 10;
                }
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                //Close();
            }
        }
        #endregion

        private void editImageClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Point point = imageBrowser.PointToScreen(new System.Windows.Point(0, 0));
            Helpers.ImageHelper.SetBitmapSource((int)point.X, (int)point.Y, (int)imageBrowser.ActualWidth, (int)imageBrowser.ActualHeight);
            _IsEdit = true;
        }
    }
}
