using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace clipboardplus
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            SizeChanged += MainWindow_SizeChanged;
            StateChanged += MainWindow_StateChanged;
            htmlBoxToggle.IsChecked = true;
            imageEditToggle.IsChecked = true;
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
        }

        /// <summary>
        /// 双击最大化、还原
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void windowDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        /// <summary>
        /// 窗口最小化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void windowMin(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// 展示分区
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
        }

        /// <summary>
        /// 隐藏高级搜索
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void advancedSearchHide(object sender, RoutedEventArgs e)
        {
            Grid.SetRowSpan(tabGrid, 2);
        }

        /// <summary>
        /// 测试
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void showSearchTabKey(object sender, KeyboardFocusChangedEventArgs e)
        {
            MessageBox.Show("key");
        }

        /// <summary>
        /// 隐藏html编辑
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void htmlBoxHide(object sender, RoutedEventArgs e)
        {
            Grid.SetColumnSpan(richTextBox, 2);
        }

        /// <summary>
        /// 展示htmlbmji
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void htmlBoxShow(object sender, RoutedEventArgs e)
        {
            Grid.SetColumnSpan(richTextBox, 1);
        }

        /// <summary>
        /// 隐藏图片编辑
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void imageEdiHide(object sender, RoutedEventArgs e)
        {
            Panel.SetZIndex(imageViewer, 1);
        }

        /// <summary>
        /// 展示图片编辑
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void imageEdiShow(object sender, RoutedEventArgs e)
        {
            Panel.SetZIndex(imageViewer, 0);
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

        private void test(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("select");
        }

        private void test(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            //MessageBox.Show(e.OriginalSource.GetType().ToString()+sender.GetType().ToString()+testtree.SelectedItem.GetType().ToString()+e.NewValue.GetType().ToString());
        }
    }
}
