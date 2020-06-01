using clipboardplus.Helpers;
using clipboardplus.Models;
using clipboardplus.Util;
using HandyControl.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MessageBox = System.Windows.MessageBox;

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
            imageEditToggle.IsChecked = true;
            //计算Windows项目缩放比例
            ScreenHelper.ResetScreenScale();
        }

        public static double ScreenWidth = SystemParameters.PrimaryScreenWidth;
        public static double ScreenHeight = SystemParameters.PrimaryScreenHeight;
        public static double ScreenScale = 1;
        public static int MinSize = 10;

        bool updateUI = true;

        public byte[] ImageSource
        {
            get { return (byte[])GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ImageSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource", typeof(byte[]), typeof(YanaImageEditor), new PropertyMetadata(null, new PropertyChangedCallback(OnImageSource)));

        private static void OnImageSource(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((d as YanaImageEditor).updateUI)
            {
                Console.WriteLine("\n\n\n\n\n\nImageSource\n" + "\n更新控件\n\n\n\n\n\n");
            }
            else
            {
                Console.WriteLine("\n\n\n\n\n\nImageSource\n" + "\n更新源\n\n\n\n\n\n");
            }
        }

        public string OCRText
        {
            get { return (string)GetValue(OCRTextProperty); }
            set { SetValue(OCRTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ImageSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OCRTextProperty =
            DependencyProperty.Register("OCRText", typeof(string), typeof(YanaImageEditor), new PropertyMetadata("", new PropertyChangedCallback(OnOCRText)));

        private static void OnOCRText(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((d as YanaImageEditor).updateUI)
            {
                Console.WriteLine("\n\n\n\n\n\nOCRText\n" + "\n更新控件\n\n\n\n\n\n" + e.NewValue);
                var yie = d as YanaImageEditor;
                if(e.NewValue != null)
                {
                    yie.temprich.Text = e.NewValue.ToString();
                }                
            }
            else
            {
                Console.WriteLine("\n\n\n\n\n\nOCRText\n" + "\n更新源\n\n\n\n\n\n");
            }
        }

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

        public Button SaveBtn
        {
            get { return this.saveBtn; }
        }

        public ToggleButton OCRBtn
        {
            get { return this.btnOCR; }
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
        public BitmapSource SetImage()
        {
            System.Windows.Point point = imageBrowser.PointToScreen(new System.Windows.Point(0, 0));
            return Helpers.ImageHelper.SetBitmapSource((int)point.X, (int)point.Y, (int)imageBrowser.ActualWidth, (int)imageBrowser.ActualHeight);
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
        public void OriginOnSave()
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
                        OnCancel();
                    });
                }))
                {
                    IsBackground = true
                };
                t.Start();
            }
        }
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
                        Show();
                        OnCancel();
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
        private BitmapSource GetCapture()
        {
            System.Windows.Point point = imageBrowser.PointToScreen(new System.Windows.Point(0, 0));
            return Helpers.ImageHelper.SetBitmapSource((int)(AppModel.Current.MaskLeftWidth + point.X + 2), (int)(AppModel.Current.MaskTopHeight + point.Y + 2), (int)MainImage.ActualWidth - 3, (int)MainImage.ActualHeight - 3);
        }
        private BitmapSource OriginGetCapture()
        {
            return ToolUtil.GetPartImage(ToolUtil.ConvertToBitmapImage(MainCanvas), (int)(AppModel.Current.MaskLeftWidth+1), (int)(AppModel.Current.MaskTopHeight+1), (int)MainImage.ActualWidth - 2, (int)MainImage.ActualHeight - 2);
        }
        #endregion

        #region 退出截图
        public void OriginOnCancel()
        {
            //Close();
        }
        public void OnCancel()
        {
            Cursor = Cursors.Cross;
            Show_Size.Visibility = Visibility.Collapsed;
            if(ImageEditBar.CurrentTool != null)
            {
                ImageEditBar.CurrentTool.ToOnClick();
            }
            ImageEditBar.Current.Visibility = Visibility.Collapsed;
            //SizeColorBar.Current.Visibility = Visibility.Collapsed;
            //MainImage.ZoomThumbVisibility = Visibility.Collapsed;
            MainImage.Current.Visibility = Visibility.Collapsed;
            for (int i = list.Count - 1; i > -1; i--)
            {
                var name = list[i].Name;
                var obj = MainCanvas.FindName(name);
                MainCanvas.Children.Remove(obj as UIElement);
                MainCanvas.UnregisterName(name);
                list.RemoveAt(i);
                MainImage.Limit = list.Count == 0 ? new Limit() : list[list.Count - 1].Limit;
            }
            num = 1;
            _IsMouseDown = false;
            _IsCapture = false;
        }
        #endregion

        #region 完成截图
        public void OriginOnOK()
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
                    OnCancel();
                });
            }))
            {
                IsBackground = true
            };
            t.Start();
        }
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
                        ImageSource = ToolUtil.ConvertToBytes(source);                        
                    }
                    Show();
                    OnCancel();
                    SetImage();
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
        private void Show()
        {
            //隐藏尺寸RGB框
            if (AppModel.Current.MaskTopHeight < 40)
            {
                SizeRGB.Visibility = Visibility.Visible;
            }
            var need = SizeColorBar.Current.Selected == Tool.Null ? 30 : 67;
            if (AppModel.Current.MaskBottomHeight < need && AppModel.Current.MaskTopHeight < need)
            {
                ImageEditBar.Current.Visibility = Visibility.Visible;
                SizeColorBar.Current.Visibility = Visibility.Visible;
            }
            MainImage.ZoomThumbVisibility = Visibility.Visible;
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
            if (_IsEdit)
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
            e.Handled = true;
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

        private void Control_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_IsEdit)
            {
                if (!_IsMouseDown || _IsCapture)
                {
                    return;
                }
                ToMouseUp();
            }
            e.Handled = true;
        }

        private void ToMouseUp()
        {
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
                if(point.X > MainCanvas.ActualWidth || point.Y > MainCanvas.ActualHeight)
                {
                    ToMouseUp();
                    return;
                }

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
            e.Handled = true;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                //Close();
            }
        }
        #endregion



        private void imageEditToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            editPanel.Visibility = Visibility.Visible;
            Panel.SetZIndex(imageViewer, 0);
        }

        private void imageEditToggle_Checked(object sender, RoutedEventArgs e)
        {
            editPanel.Visibility = Visibility.Collapsed;
            Panel.SetZIndex(imageViewer, 1);
        }

        private void ImageAnticlockwise(object sender, RoutedEventArgs e)
        {
            OnCancel();
            Cursor = Cursors.Arrow;
            _IsEdit = false;
            TransformedBitmap tb = new TransformedBitmap();
            tb.BeginInit();//表示开始 TransformedBitmap 初始化。
            tb.Source = ToolUtil.ConvertToBitmap(ImageSource);
            RotateTransform transform = new RotateTransform(-90);//旋转角度
            tb.Transform = transform;
            tb.EndInit();//结束的信号 BitmapImage 初始化。
            ImageSource = ToolUtil.ConvertToBytes(tb);//设置image控件source
        }

        private void ImageClockwise(object sender, RoutedEventArgs e)
        {
            OnCancel();
            Cursor = Cursors.Arrow;
            _IsEdit = false;
            TransformedBitmap tb = new TransformedBitmap();
            tb.BeginInit();//表示开始 TransformedBitmap 初始化。
            tb.Source = ToolUtil.ConvertToBitmap(ImageSource);
            RotateTransform transform = new RotateTransform(90);//旋转角度
            tb.Transform = transform;
            tb.EndInit();//结束的信号 BitmapImage 初始化。
            ImageSource = ToolUtil.ConvertToBytes(tb);//设置image控件source
        }

        private void ImageFlipHorizontal(object sender, RoutedEventArgs e)
        {
            OnCancel();
            Cursor = Cursors.Arrow;
            _IsEdit = false;
            TransformedBitmap tb = new TransformedBitmap();
            tb.BeginInit();//表示开始 TransformedBitmap 初始化。
            tb.Source = ToolUtil.ConvertToBitmap(ImageSource);
            ScaleTransform transform = new ScaleTransform(-1, 1);//旋转角度
            tb.Transform = transform;
            tb.EndInit();//结束的信号 BitmapImage 初始化。
            ImageSource = ToolUtil.ConvertToBytes(tb);//设置image控件source
        }

        private void ImageFlipVertical(object sender, RoutedEventArgs e)
        {
            OnCancel();
            Cursor = Cursors.Arrow;
            _IsEdit = false;
            TransformedBitmap tb = new TransformedBitmap();
            tb.BeginInit();//表示开始 TransformedBitmap 初始化。
            tb.Source = ToolUtil.ConvertToBitmap(ImageSource);
            ScaleTransform transform = new ScaleTransform(1, -1);//旋转角度
            tb.Transform = transform;
            tb.EndInit();//结束的信号 BitmapImage 初始化。
            ImageSource = ToolUtil.ConvertToBytes(tb);//设置image控件source
        }

        private void ImageEditOpen(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(imageBrowser.ActualWidth + "--" + imageBrowser.ActualHeight + "--" + MainCanvas.ActualWidth + "--" + MainCanvas.ActualHeight);
            if (SetImage() != null)
            {
                Cursor = Cursors.Cross;
                _IsEdit = true;
            }
        }

        private void ImageEditClose(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Arrow;
            _IsEdit = false;
        }
    }
}
