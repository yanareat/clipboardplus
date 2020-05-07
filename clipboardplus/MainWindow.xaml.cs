﻿using System;
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

        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                double clipWidth = SystemParameters.MaximumWindowTrackWidth;
                double clipHeight = SystemParameters.MaximumWindowTrackHeight;
                double gridWidth = SystemParameters.PrimaryScreenWidth;
                double gridHeight = SystemParameters.PrimaryScreenHeight;
                //MessageBox.Show(gridWidth + "    " + gridHeight + "    " + clipWidth + "     " + clipHeight);
                windowClip.Rect = new Rect(0, 0, clipWidth, clipHeight);
                globalGrid.Width = gridWidth;
                globalGrid.Height = gridHeight;
            }
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
            {
                windowClip.Rect = new Rect(0, 0, this.Width, this.Height);
                globalGrid.Width = this.Width;
                globalGrid.Height = this.Height;
            }
        }

        private void windowMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void windowDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void windowMin(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

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

        private void showPersonalCenter(object sender, MouseButtonEventArgs e)
        {
            personalCenterTab.IsSelected = true;
        }

        private void showSearchTab(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("&&&&&&&&&&&----------------&&&&&&&&&");
            searchTab.IsSelected = true;
            e.Handled = true;
        }

        private void advancedSearchShow(object sender, RoutedEventArgs e)
        {
            Grid.SetRowSpan(tabGrid, 1);
        }

        private void advancedSearchHide(object sender, RoutedEventArgs e)
        {
            Grid.SetRowSpan(tabGrid, 2);
        }

        private void showSearchTabKey(object sender, KeyboardFocusChangedEventArgs e)
        {
            MessageBox.Show("key");
        }

        private void htmlBoxHide(object sender, RoutedEventArgs e)
        {
            Grid.SetColumnSpan(richTextBox, 2);
        }

        private void htmlBoxShow(object sender, RoutedEventArgs e)
        {
            Grid.SetColumnSpan(richTextBox, 1);
        }

        private void imageEdiHide(object sender, RoutedEventArgs e)
        {
            Panel.SetZIndex(imageViewer, 1);
        }

        private void imageEdiShow(object sender, RoutedEventArgs e)
        {
            Panel.SetZIndex(imageViewer, 0);
        }

        private void beHandled(object sender, SelectionChangedEventArgs e)
        {
            //MessageBox.Show("test1");
            //MessageBox.Show(e.AddedItems.GetType().ToString());
            Console.WriteLine("&&&&&&&&&&&++++++++++++++++&&&&&&&&&");
            //e.Handled = true;
        }

        private void test(object sender, MouseButtonEventArgs e)
        {
            //MessageBox.Show("test1");
            //MessageBox.Show(sender.GetType().ToString());
            Console.WriteLine("分区:"+sender.GetType());
            var treeViewItem = VisualUpwardSearch<TreeViewItem>(e.OriginalSource as DependencyObject) as TreeViewItem;
            if (treeViewItem != null)
            {
                treeViewItem.Focus();
                e.Handled = true;
            }
        }

        private void test(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("select");
        }

        private void test(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            //MessageBox.Show(e.OriginalSource.GetType().ToString()+sender.GetType().ToString()+testtree.SelectedItem.GetType().ToString()+e.NewValue.GetType().ToString());
        }

        private DependencyObject VisualUpwardSearch<T>(DependencyObject source)
        {
            while (source != null && source.GetType() != typeof(T))
                source = VisualTreeHelper.GetParent(source);
            return source;
        }
    }
}
