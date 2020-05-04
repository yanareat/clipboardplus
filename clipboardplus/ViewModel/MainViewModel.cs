using clipboardplus.Model;
using clipboardplus.Util;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using CM = ClipboardMonitor;

namespace clipboardplus.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            _recordList = new ObservableCollection<Record>();
            if (IsInDesignMode)
            {
                // Code runs in Blend --> create design time data.
                _recordList.Add(new Record()
                {
                    Id = 0,
                    Title = "题0目",
                    Type = 1,
                    Time = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                    From = "CentBrowser",
                    TextData = "// Code runs in Blend --> create design time data."
                });
                _recordList.Add(new Record()
                {
                    Id = 0,
                    Title = "题0目",
                    Type = 2,
                    Time = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                    From = "CentBrowser",
                    TextData = "// Code data."
                    //,ImageData = ToolUtil.ConvertToBytes(new BitmapImage(new Uri("pack://application:,,,/Resources/Images/content_0.jpg")))
                });

                ToEditTextRecord = new Record()
                {
                    Id = 0,
                    Title = "题0目",
                    Type = 2,
                    Time = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                    From = "CentBrowser",
                    TextData = "// Code data."
                    //,ImageData = ToolUtil.ConvertToBytes(new BitmapImage(new Uri("pack://application:,,,/Resources/Images/content_0.jpg")))
                };
            }
            else
            {
                //CM.ClipboardMonitor cm = new CM.ClipboardMonitor();
                //cm.ClipboardData += (obj, args) =>
                //{
                //    try
                //    {
                //        ClipboardListener();
                //    }
                //    catch (Exception e)
                //    {
                //        Console.WriteLine(e.ToString());
                //    }
                //};
                // Code runs "for real"
                _recordList.Add(new Record()
                {
                    Id = 3,
                    Title = "今天笔记",
                    Type = 1,
                    Time = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                    From = "OneNote",
                    TextData = @"BitmapImage bi = null;
                                if (ImageSize != 0)
                                {
                                    bi = new BitmapImage();
                                    bi.BeginInit();
                                    bi.StreamSource = new MemoryStream(ImageData);
                                    bi.EndInit();
                                }"
                });
                _recordList.Add(new Record()
                {
                    Id = 0,
                    Title = "题0目",
                    Type = 2,
                    Time = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                    From = "CentBrowser",
                    TextData = "// Code data.",
                    ImageData = ToolUtil.ConvertToBytes(new BitmapImage(new Uri("pack://application:,,,/Resources/Images/content_0.jpg")))
                });
            }
        }

        #region 属性
        /// <summary>
        /// 剪贴板记录
        /// </summary>
        private Record _toEditTextRecord;
        public Record ToEditTextRecord
        {
            get => _toEditTextRecord;
            set { _toEditTextRecord = value; RaisePropertyChanged(() => ToEditTextRecord); }
        }

        /// <summary>
        /// 剪贴板记录列表
        /// </summary>
        private ObservableCollection<Record> _recordList;
        public ObservableCollection<Record> RecordList
        {
            get => _recordList;
            set { _recordList = value; RaisePropertyChanged(() => RecordList); }
        }

        #endregion

        #region 命令
        /// <summary>
        /// 命令模板
        /// </summary>
        private RelayCommand _changTitle;
        public RelayCommand ChangTitle
        {
            get
            {
                return _changTitle
                    ?? (_changTitle = new RelayCommand(
                        () =>
                        {
                            _toEditTextRecord.Title = "标题改变";
                        }));
            }
        }

        /// <summary>
        /// 命令模板
        /// </summary>
        private RelayCommand<object> _searchSelectionChanged;
        public RelayCommand<object> SearchSelectionChanged
        {
            get
            {
                return _searchSelectionChanged
                    ?? (_searchSelectionChanged = new RelayCommand<object>(
                        (selected) =>
                        {
                            ToEditTextRecord = (Record)selected;
                            Console.WriteLine("@@@@@@@@@@@@@@" + ToEditTextRecord.TextData);
                        }));
            }
        }
        #endregion

        #region 方法

        /// <summary>
        /// 剪贴触发函数
        /// </summary>
        private void ClipboardListener()
        {
            var clipData = System.Windows.Clipboard.GetDataObject();
            foreach (var format in clipData.GetFormats())
            {
                Console.WriteLine(format);
            }
            Console.WriteLine(clipData.GetDataPresent(System.Windows.DataFormats.Bitmap));
            Record clipItem;
            if (clipData.GetDataPresent(System.Windows.DataFormats.Bitmap))
            {
                Console.WriteLine("GetDataPresent");
                byte[] tempData = ToolUtil.ConvertToBytes(System.Windows.Clipboard.GetImage());
                clipItem = new Record()
                {
                    Id = 1,
                    Type = 2,
                    From = ToolUtil.ClipFrom(),
                    MD5 = ToolUtil.GetMD5Hash(tempData),
                    Time = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                    Title = "图片",
                    ImageData = tempData
                };
                //sqlu.Insert(clipItem);
            }
            else
            {
                string temp = System.Windows.Clipboard.GetText();
                clipItem = new Record()
                {
                    Id = 5,
                    Type = 1,
                    From = ToolUtil.ClipFrom(),
                    MD5 = ToolUtil.GetMD5Hash(System.Windows.Clipboard.GetText(), 0),
                    Time = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                    Title = temp,
                    TextData = temp
                };
                //sqlu.Insert(clipItem);
            }
            foreach (var item in RecordList)
            {
                if (item.MD5 == clipItem.MD5)
                {
                    RecordList.Remove(item);
                    break;
                }
            }
            RecordList.Insert(0, clipItem);
            Console.WriteLine("ClipboardMonitor");
        }
        #endregion
    }
}