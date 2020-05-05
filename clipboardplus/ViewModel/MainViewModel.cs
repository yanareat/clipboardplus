using clipboardplus.Model;
using clipboardplus.Util;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.ObjectModel;
using System.Text;
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
                    Time = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString(),
                    Origin = "CentBrowser",
                    TextData = "// Code runs in Blend --> create design time data."
                });
                _recordList.Add(new Record()
                {
                    Id = 0,
                    Title = "题0目",
                    Type = 2,
                    Time = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString(),
                    Origin = "CentBrowser",
                    TextData = "// Code data."
                    //,ImageData = ToolUtil.ConvertToBytes(new BitmapImage(new Uri("pack://application:,,,/Resources/Images/content_0.jpg")))
                });

                ToEditTextRecord = new Record()
                {
                    Id = 0,
                    Title = "题0目",
                    Type = 1,
                    Time = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString(),
                    Origin = "CentBrowser",
                    TextData = "// Code data."
                    //,ImageData = ToolUtil.ConvertToBytes(new BitmapImage(new Uri("pack://application:,,,/Resources/Images/content_0.jpg")))
                };

                ToEditImageRecord = new Record()
                {
                    Id = 0,
                    Title = "题0目",
                    Type = 2,
                    Time = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString(),
                    Origin = "CentBrowser",
                    TextData = "// Picture"
                    //,ImageData = ToolUtil.ConvertToBytes(new BitmapImage(new Uri("pack://application:,,,/Resources/Images/content_0.jpg")))
                };
            }
            else
            {
                CM.ClipboardMonitor cm = new CM.ClipboardMonitor();
                cm.ClipboardData += (obj, args) =>
                {
                    try
                    {
                        ClipboardListener();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                };
                // Code runs "for real"
                _recordList.Add(new Record()
                {
                    Id = 4,
                    Title = "今天笔记",
                    Type = 1,
                    Time = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString(),
                    Origin = "OneNote",
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
                    Time = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString(),
                    Origin = "CentBrowser",
                    TextData = "// Code data.",
                    ImageData = ToolUtil.ConvertToBytes(new BitmapImage(new Uri("pack://application:,,,/Resources/Images/content_0.jpg")))
                });
            }
        }

        #region 属性
        /// <summary>
        /// 数据库工具类
        /// </summary>
        private SQLiteUtil<Record> _sqlutil = new SQLiteUtil<Record>(new StringBuilder("D:/5_desktop"));
        public SQLiteUtil<Record> Sqlutil
        {
            get => _sqlutil;
            set { _sqlutil = value; RaisePropertyChanged(() => Sqlutil); }
        }

        /// <summary>
        /// 文本编辑记录
        /// </summary>
        private Record _toEditTextRecord;
        public Record ToEditTextRecord
        {
            get => _toEditTextRecord;
            set { _toEditTextRecord = value; RaisePropertyChanged(() => ToEditTextRecord); }
        }

        /// <summary>
        /// 图片编辑记录
        /// </summary>
        private Record _toEditImageRecord;
        public Record ToEditImageRecord
        {
            get => _toEditImageRecord;
            set { _toEditImageRecord = value; RaisePropertyChanged(() => ToEditImageRecord); }
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
        /// 选择记录触发
        /// </summary>
        private RelayCommand<object> _selectionChanged;
        public RelayCommand<object> SelectionChanged
        {
            get
            {
                return _selectionChanged
                    ?? (_selectionChanged = new RelayCommand<object>(
                        (selected) =>
                        {
                            Record tempRecord = ToolUtil.DeepCopy((Record)selected);
                            Console.WriteLine("****************" + tempRecord.Type);
                            _ = tempRecord.Type == 1 ? ToEditTextRecord = tempRecord : ToEditImageRecord = tempRecord;
                            //Console.WriteLine("@@@@@@@@@@@@@@" + ToEditTextRecord.TextData+"\n###############"+ToEditImageRecord.TextData);
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

            Record clipItem = new Record()
            {
                Origin = ToolUtil.ClipFrom(),
                Time = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString(),
            };

            if (clipData.GetDataPresent(System.Windows.DataFormats.Bitmap))
            {
                byte[] tempData = ToolUtil.ConvertToBytes(System.Windows.Clipboard.GetImage());
                clipItem.Type = 2;
                clipItem.Title = "图片";
                clipItem.ImageData = tempData;
            }
            else
            {
                string temp = System.Windows.Clipboard.GetText();
                clipItem.Type = 1;
                clipItem.Title = temp;
                clipItem.TextData = temp;
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

            var res = Sqlutil.Query<Record>("select id from record where md5 = '" + clipItem.MD5 + "'");
            if (res.Count == 0)
            {
                Console.WriteLine("777");
                Sqlutil.Add(clipItem);
            }
            else
            {
                Console.WriteLine("aaa" + res[0].Id);
                clipItem.Id = res[0].Id;
                Sqlutil.Update(clipItem);
            }

            Console.WriteLine("ClipboardMonitor");
        }
        #endregion
    }
}