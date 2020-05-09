using clipboardplus.Model;
using clipboardplus.Util;
using clipboardplus.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace clipboardplus
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        //MainViewModel main = new MainViewModel();
        public MainWindow()
        {
            InitializeComponent();
            wsl = WindowState;
            //DataContext = main;
            HotKeySettingsManager.Instance.RegisterGlobalHotKeyEvent += Instance_RegisterGlobalHotKeyEvent;
            SizeChanged += MainWindow_SizeChanged;
            StateChanged += MainWindow_StateChanged;
            htmlBoxToggle.IsChecked = true;
            imageEditToggle.IsChecked = true;
        }
        #region 属性

        WindowState wsl;
        #endregion

        #region clipboard参数
        private HwndSource source = null;
        private IntPtr nextClipboardViewer;
        private IntPtr handle
        {
            get
            {
                return new WindowInteropHelper(this).Handle;
            }
        }
        /// <summary>
        /// 记录快捷键注册项的唯一标识符
        /// </summary>
        private Dictionary<EHotKeySetting, int> m_HotKeySettings = new Dictionary<EHotKeySetting, int>();
        #endregion

        #region overrides
        /// <summary>
        /// 关闭程序，从观察链移除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            nextClipboardViewer = (IntPtr)SetClipboardViewer((int)this.handle);
            source = PresentationSource.FromVisual(this) as HwndSource;
            source.AddHook(WndProc);
        }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            ChangeClipboardChain(this.handle, nextClipboardViewer);
            if (null != source)
                source.RemoveHook(WndProc);
        }

        /// <summary>
        /// 所有控件初始化完成后调用
        /// </summary>
        /// <param name="e"></param>
        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            // 注册热键
            InitHotKey();
        }
        #endregion

        #region ClipboardPlus
        /// <summary>
        /// 要处理的 WindowsSystem.Windows.Forms.Message。
        /// </summary>
        /// <param name="m"></param>
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            var hotkeySetting = new EHotKeySetting();
            switch (msg)
            {
                case WM_DRAWCLIPBOARD:
                    ClipboardPlus();
                    SendMessage(nextClipboardViewer, msg, wParam, lParam);
                    break;
                case WM_CHANGECBCHAIN:
                    if (wParam == nextClipboardViewer)
                        nextClipboardViewer = lParam;
                    else
                        SendMessage(nextClipboardViewer, msg, wParam, lParam);
                    break;
                case ToolUtil.WM_HOTKEY:
                    int sid = wParam.ToInt32();
                    if (sid == m_HotKeySettings[EHotKeySetting.显示])
                    {
                        hotkeySetting = EHotKeySetting.显示;
                        winNor();
                        //TODO 执行显示操作
                    }
                    else if (sid == m_HotKeySettings[EHotKeySetting.隐藏])
                    {
                        hotkeySetting = EHotKeySetting.隐藏;
                        winMin();
                        //TODO 执行隐藏操作
                    }
                    else if (sid == m_HotKeySettings[EHotKeySetting.截图])
                    {
                        hotkeySetting = EHotKeySetting.截图;
                        //TODO 截图......
                    }

                    //MessageBox.Show(string.Format("触发【{0}】快捷键", hotkeySetting.ToString()));
                    handled = true;
                    break;
            }
            return IntPtr.Zero;
        }
        /// <summary>
        /// 显示剪贴板内容
        /// </summary>
        public void ClipboardPlus()
        {
            try
            {
                (DataContext as MainViewModel).ClipboardListener();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                //MessageBox.Show(e.ToString());
            }
        }
        #endregion

        #region WindowsAPI
        private const int WM_DRAWCLIPBOARD = 0x308;
        private const int WM_CHANGECBCHAIN = 0x030D;
        /// <summary>
        /// 将CWnd加入一个窗口链，每当剪贴板的内容发生变化时，就会通知这些窗口
        /// </summary>
        /// <param name="hWndNewViewer">句柄</param>
        /// <returns>返回剪贴板观察器链中下一个窗口的句柄</returns>
        [DllImport("User32.dll")]
        protected static extern int SetClipboardViewer(int hWndNewViewer);

        /// <summary>
        /// 从剪贴板链中移出的窗口句柄
        /// </summary>
        /// <param name="hWndRemove">从剪贴板链中移出的窗口句柄</param>
        /// <param name="hWndNewNext">hWndRemove的下一个在剪贴板链中的窗口句柄</param>
        /// <returns>如果成功，非零;否则为0。</returns>
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern bool ChangeClipboardChain(IntPtr hWndRemove, IntPtr hWndNewNext);

        /// <summary>
        /// 将指定的消息发送到一个或多个窗口
        /// </summary>
        /// <param name="hwnd">其窗口程序将接收消息的窗口的句柄</param>
        /// <param name="wMsg">指定被发送的消息</param>
        /// <param name="wParam">指定附加的消息特定信息</param>
        /// <param name="lParam">指定附加的消息特定信息</param>
        /// <returns>消息处理的结果</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);
        #endregion

        /// <summary>
        /// 通知注册系统快捷键事件处理函数
        /// </summary>
        /// <param name="hotKeyModelList"></param>
        /// <returns></returns>
        private bool Instance_RegisterGlobalHotKeyEvent(ObservableCollection<HotKeyModel> hotKeyModelList)
        {
            return InitHotKey(hotKeyModelList);
        }

        /// <summary>
        /// 初始化注册快捷键
        /// </summary>
        /// <param name="hotKeyModelList">待注册热键的项</param>
        /// <returns>true:保存快捷键的值；false:弹出设置窗体</returns>
        private bool InitHotKey(ObservableCollection<HotKeyModel> hotKeyModelList = null)
        {
            var list = hotKeyModelList ?? HotKeySettingsManager.Instance.LoadDefaultHotKey();
            // 注册全局快捷键
            string failList = ToolUtil.RegisterGlobalHotKey(list, handle, out m_HotKeySettings);
            if (string.IsNullOrEmpty(failList))
                return true;
            MessageBoxResult mbResult = MessageBox.Show(string.Format("无法注册下列快捷键\n\r{0}是否要改变这些快捷键？", failList), "提示", MessageBoxButton.YesNo);
            // 弹出热键设置窗体
            if (mbResult == MessageBoxResult.Yes)
            {
                Activate();
                return false;
            }
            return true;
        }

        /// <summary>
        /// 窗口状态改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                double clipWidth = SystemParameters.MaximumWindowTrackWidth;
                double clipHeight = SystemParameters.MaximumWindowTrackHeight;
                double gridWidth = SystemParameters.PrimaryScreenWidth;
                double gridHeight = SystemParameters.PrimaryScreenHeight;
                windowClip.Rect = new Rect(0, 0, clipWidth, clipHeight);
                globalGrid.Width = gridWidth;
                globalGrid.Height = gridHeight;
            }
        }

        /// <summary>
        /// 窗口尺寸改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
            {
                windowClip.Rect = new Rect(0, 0, this.Width, this.Height);
                globalGrid.Width = this.Width;
                globalGrid.Height = this.Height;
            }
            e.Handled = true;
        }

        /// <summary>
        /// 窗口移动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void windowMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
            e.Handled = true;
        }

        /// <summary>
        /// 双击最大化、还原
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void windowDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            e.Handled = true;
        }

        /// <summary>
        /// 窗口最小化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void windowMin(object sender, MouseButtonEventArgs e)
        {
            winMin();
            e.Handled = true;
        }

        /// <summary>
        /// 窗口最小化
        /// </summary>
        private void winMin()
        {
            Hide();
            this.WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// 展示编辑分区
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void showEditZone(object sender, MouseButtonEventArgs e)
        {
            if (this.Width == 300)
            {
                this.Width = 800;
                this.Height = 450;
            }
            else if (this.Width == 800)
            {
                this.Width = 300;
                this.Height = 450;
            }
            e.Handled = true;
        }

        /// <summary>
        /// 展示编辑Tab
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void showEditTab(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine(e.OriginalSource.GetType().ToString()+e.Source.GetType().ToString()+(sender as Grid).DataContext.GetType().ToString());
            e.Handled = true;
        }

        /// <summary>
        /// 展示个人中心
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void showPersonalCenter(object sender, MouseButtonEventArgs e)
        {
            personalCenterTab.IsSelected = true;
            e.Handled = true;
        }

        /// <summary>
        /// 切换搜索Tab
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void showSearchTab(object sender, RoutedEventArgs e)
        {
            searchTab.IsSelected = true;
            e.Handled = true;
        }

        /// <summary>
        /// 展示高级搜索
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void advancedSearchShow(object sender, RoutedEventArgs e)
        {
            Grid.SetRowSpan(tabGrid, 1);
            e.Handled = true;
        }

        /// <summary>
        /// 隐藏高级搜索
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void advancedSearchHide(object sender, RoutedEventArgs e)
        {
            Grid.SetRowSpan(tabGrid, 2);
            e.Handled = true;
        }

        /// <summary>
        /// 测试
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void showSearchTabKey(object sender, KeyboardFocusChangedEventArgs e)
        {
            MessageBox.Show("key");
            e.Handled = true;
        }

        /// <summary>
        /// 隐藏html编辑
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void htmlBoxHide(object sender, RoutedEventArgs e)
        {
            Grid.SetColumnSpan(richTextBox, 2);
            e.Handled = true;
        }

        /// <summary>
        /// 展示htmlbmji
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void htmlBoxShow(object sender, RoutedEventArgs e)
        {
            Grid.SetColumnSpan(richTextBox, 1);
            e.Handled = true;
        }

        /// <summary>
        /// 隐藏图片编辑
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void imageEdiHide(object sender, RoutedEventArgs e)
        {
            Panel.SetZIndex(imageViewer, 1);
            e.Handled = true;
        }

        /// <summary>
        /// 展示图片编辑
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void imageEdiShow(object sender, RoutedEventArgs e)
        {
            Panel.SetZIndex(imageViewer, 0);
            e.Handled = true;
        }

        /// <summary>
        /// 右键选择分区
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MouseRightButtonSelectZone(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine("分区:"+sender.GetType());
            var treeViewItem = VisualUpwardSearch<TreeViewItem>(e.OriginalSource as DependencyObject) as TreeViewItem;
            if (treeViewItem != null)
            {
                treeViewItem.Focus();
                e.Handled = true;
            }
        }

        /// <summary>
        /// 可视树搜索
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        private DependencyObject VisualUpwardSearch<T>(DependencyObject source)
        {
            while (source != null && source.GetType() != typeof(T))
                source = VisualTreeHelper.GetParent(source);
            return source;
        }

        /// <summary>
        /// 窗口正常
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void winNormal(object sender, RoutedEventArgs e)
        {
            winNor();
            e.Handled = true;
        }

        /// <summary>
        /// 窗口正常
        /// </summary>
        private void winNor()
        {
            Show();
            this.WindowState = wsl;
        }

        /// <summary>
        /// 程序退出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void winExit(object sender, RoutedEventArgs e)
        {
            notifyIcon.Visibility = Visibility.Collapsed;
            Environment.Exit(0);
            e.Handled = true;
        }

        private void test(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
            //loadingCircle.Visibility = Visibility.Visible;
            //Thread.Sleep(1000);
            //loadingCircle.Visibility = Visibility.Collapsed;
        }

        private void test(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            //MessageBox.Show("Expand");
        }

        private void test(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            e.Handled = true;
            //MessageBox.Show("e");
            //Console.WriteLine("e");
            //MessageBox.Show(e.OriginalSource.GetType().ToString()+sender.GetType().ToString()+testtree.SelectedItem.GetType().ToString()+e.NewValue.GetType().ToString());
        }

        private void test(object sender, DependencyPropertyChangedEventArgs e)
        {
        }

        private void test(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }
    }
}
