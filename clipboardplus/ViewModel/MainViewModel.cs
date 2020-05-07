using clipboardplus.Model;
using clipboardplus.Util;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using SqlSugar;
using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
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
            if (IsInDesignMode)
            {
                // Code runs in Blend --> create design time data.
            }
            else
            {
                InitData();
                ClipboardRecorderOpen();
                // Code runs "for real"
            }
        }

        #region 属性
        /// <summary>
        /// 数据库工具类
        /// </summary>
        private SqlUtil<Record> _sqlutil = new SqlUtil<Record>(new StringBuilder("DataSource=D:/5_desktop"), DbType.Sqlite);
        public SqlUtil<Record> Sqlutil
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
        /// 历史记录列表
        /// </summary>
        private ObservableCollection<Record> _recordList;
        public ObservableCollection<Record> RecordList
        {
            get => _recordList;
            set { _recordList = value; RaisePropertyChanged(() => RecordList); }
        }

        /// <summary>
        /// 分区列表
        /// </summary>
        private ObservableCollection<Zone> _zoneTree;
        public ObservableCollection<Zone> ZoneTree
        {
            get => _zoneTree;
            set { _zoneTree = value; RaisePropertyChanged(() => ZoneTree); }
        }

        /// <summary>
        /// 选择的记录
        /// </summary>
        private Record _selectedRecord;
        public Record SelectedRecord
        {
            get => _selectedRecord;
            set { _selectedRecord = value; RaisePropertyChanged(() => SelectedRecord); }
        }

        /// <summary>
        /// 选择的分区
        /// </summary>
        private Zone _selectedZone;
        public Zone SelectedZone
        {
            get => _selectedZone;
            set { _selectedZone = value; RaisePropertyChanged(() => SelectedZone); }
        }

        /// <summary>
        /// 选择的分区
        /// </summary>
        private Zone _rootZone = new Zone() { Id = 0, Parent = 0, Name = "根目录", Nodes = new ObservableCollection<Zone>() };
        public Zone RootZone
        {
            get => _rootZone;
            set { _rootZone = value; RaisePropertyChanged(() => RootZone); }
        }

        /// <summary>
        /// Tab序号
        /// </summary>
        private int _tabIndex;
        public int TabIndex
        {
            get => _tabIndex;
            set { _tabIndex = value; RaisePropertyChanged(() => TabIndex); }
        }

        /// <summary>
        /// 列表序号
        /// </summary>
        private int _listIndex;
        public int ListIndex
        {
            get => _listIndex;
            set { _listIndex = value; RaisePropertyChanged(() => ListIndex); }
        }
        #endregion

        #region 命令
        /// <summary>
        /// 选择记录触发
        /// </summary>
        private RelayCommand<SelectionChangedEventArgs> _selectionChanged;
        public RelayCommand<SelectionChangedEventArgs> SelectionChanged
        {
            get
            {
                return _selectionChanged
                    ?? (_selectionChanged = new RelayCommand<SelectionChangedEventArgs>(e => changeEditRecord(e)));
            }
        }

        /// <summary>
        /// 选择Tab触发
        /// </summary>
        private RelayCommand<SelectionChangedEventArgs> _tabSelectionChanged;
        public RelayCommand<SelectionChangedEventArgs> TabSelectionChanged
        {
            get
            {
                return _tabSelectionChanged
                    ?? (_tabSelectionChanged = new RelayCommand<SelectionChangedEventArgs>(e => changeRecordList(e)));
            }
        }

        /// <summary>
        /// 选择Tab触发
        /// </summary>
        private RelayCommand<RoutedPropertyChangedEventArgs<object>> _zoneSelectionChanged;
        public RelayCommand<RoutedPropertyChangedEventArgs<object>> ZoneSelectionChanged
        {
            get
            {
                return _zoneSelectionChanged
                    ?? (_zoneSelectionChanged = new RelayCommand<RoutedPropertyChangedEventArgs<object>>(e => changeBookRecordList(e)));
            }
        }

        /// <summary>
        /// 选择Tab触发
        /// </summary>
        private RelayCommand<RoutedEventArgs> _zoneMenuSelected;
        public RelayCommand<RoutedEventArgs> ZoneMenuSelected
        {
            get
            {
                return _zoneMenuSelected
                    ?? (_zoneMenuSelected = new RelayCommand<RoutedEventArgs>(e => resetZoneTree(e)));
            }
        }

        /// <summary>
        /// 选择Tab触发
        /// </summary>
        private RelayCommand<MouseButtonEventArgs> _showRecycle;
        public RelayCommand<MouseButtonEventArgs> ShowRecycle
        {
            get
            {
                return _showRecycle
                    ?? (_showRecycle = new RelayCommand<MouseButtonEventArgs>(e => showDeletedRecordList(e)));
            }
        }

        /// <summary>
        /// 选择Tab触发
        /// </summary>
        private RelayCommand<MouseButtonEventArgs> _testCom;
        public RelayCommand<MouseButtonEventArgs> TestCom
        {
            get
            {
                return _testCom
                    ?? (_testCom = new RelayCommand<MouseButtonEventArgs>(e => test(e)));
            }
        }

        #endregion

        #region 方法

        /// <summary>
        /// 剪贴监听函数
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

            var res = Sqlutil.CurrentDb.GetList(r => r.MD5 == clipItem.MD5);
            if (res.Count == 0)
            {
                Console.WriteLine("777");
                Sqlutil.CurrentDb.Insert(clipItem);
            }
            else
            {
                Console.WriteLine("aaa" + res[0].Id);
                Console.WriteLine("MD5:"+clipItem.MD5);
                Sqlutil.CurrentDb.Update(r => new Record() { Time = clipItem.Time }, r => r.Id == res[0].Id);
            }

            Console.WriteLine("ClipboardMonitor");
        }
        
        /// <summary>
        /// 开启剪贴板记录器
        /// </summary>
        private void ClipboardRecorderOpen()
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
        }

        /// <summary>
        /// 初始化数据
        /// </summary>
        private void InitData()
        {
            RecordList = new ObservableCollection<Record>(Sqlutil.CurrentDb.GetPageList(r => r.Deleted == 0, new PageModel() { PageIndex = 1, PageSize = 10 }));
            ZoneTree = ToolUtil.getTrees(0, new ObservableCollection<Zone>(Sqlutil.ZoneDb.GetPageList(z => z.Parent != -1, new PageModel() { PageIndex = 1, PageSize = 10 })));
        }

        /// <summary>
        /// 改变文本编辑记录的内容
        /// </summary>
        /// <param name="e"></param>
        private void changeEditRecord(SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 0)
            {
                SelectedRecord = e.AddedItems[0] as Record;
                Record tempRecord = ToolUtil.DeepCopy(SelectedRecord);
                if (tempRecord.Type == 1)
                {
                    ToEditTextRecord = tempRecord;
                }
                else
                {
                    ToEditImageRecord = tempRecord;
                }
                Console.WriteLine(RecordList.Count + "%%%%%%%%%%%%%^^^^^^^^^^^^^^^^^^%%%%%%%%%%%%");
                //MessageBox.Show(((Record)e.AddedItems[0]).TextData);
            }
            e.Handled = true;
        }

        /// <summary>
        /// 改变文本编辑记录的内容
        /// </summary>
        /// <param name="e"></param>
        private void changeBookRecordList(RoutedPropertyChangedEventArgs<object> e)
        {
            //MessageBox.Show(((Record)e.AddedItems[0]).TextData);
            if (e.NewValue != null)
            {
                SelectedZone = e.NewValue as Zone;
                //Console.WriteLine(e.NewValue.GetType().ToString());
                Console.WriteLine(SelectedZone.Name);
                RecordList.Clear();
                Sqlutil.CurrentDb.GetPageList(r => r.Deleted == 0 && r.Zone == SelectedZone.Id, new PageModel() { PageIndex = 1, PageSize = 10 }).ForEach(r => RecordList.Add(r));
            }
            e.Handled = true;
        }

        /// <summary>
        /// 改变记录列表的内容
        /// </summary>
        private void changeRecordList(SelectionChangedEventArgs e)
        {
            //MessageBox.Show(selected.ToString());
            RecordList.Clear();
            switch ((e.OriginalSource as TabControl).SelectedIndex)
            {
                case 0:
                    Sqlutil.CurrentDb.GetPageList(r => r.Deleted == 0, new PageModel() { PageIndex = 1, PageSize = 10 }).ForEach(r => RecordList.Add(r));
                    break;
                case 1:
                    break;
                case 4:
                    Sqlutil.CurrentDb.GetPageList(r => r.Deleted == 0 && r.Type == 1, new PageModel() { PageIndex = 1, PageSize = 10 }).ForEach(r => RecordList.Add(r));
                    break;
                default:
                    break;
            }
            Console.WriteLine(RecordList.Count+ "%%%%%%%%%%%%%%%%%%%%%%%%%"  + "@@@@@@@@@@@@@@@@@@@@");
            //MessageBox.Show((e.OriginalSource as TabControl).SelectedIndex.ToString());
            e.Handled = true;
        }

        /// <summary>
        /// 重新构建分区树
        /// </summary>
        /// <param name="e"></param>
        private void resetZoneTree(RoutedEventArgs e)
        {
            //MessageBox.Show("1");
            //Console.WriteLine(e.OriginalSource.GetType().ToString());
            var mi = e.OriginalSource as MenuItem;
            var zone = mi.DataContext as Zone;
            Console.WriteLine(zone.Name);
            Sqlutil.ZoneDb.Update(z => new Zone() { Parent = zone.Id }, z => z.Id == SelectedZone.Id);
            ZoneTree = ToolUtil.getTrees(0, new ObservableCollection<Zone>(Sqlutil.ZoneDb.GetPageList(z => z.Parent != -1, new PageModel() { PageIndex = 1, PageSize = 10 })));
            e.Handled = true;
        }

        /// <summary>
        /// 展示被删除的记录
        /// </summary>
        /// <param name="e"></param>
        private void showDeletedRecordList(MouseButtonEventArgs e)
        {
            RecordList.Clear();
            Sqlutil.CurrentDb.GetPageList(r => r.Deleted == 1, new PageModel() { PageIndex = 1, PageSize = 10 }).ForEach(r => RecordList.Add(r));
        }

        /// <summary>
        /// 测试方法
        /// </summary>
        /// <param name="e"></param>
        private void test(MouseButtonEventArgs e)
        {
            //MessageBox.Show("1");
            //Console.WriteLine(e.OriginalSource.GetType().ToString());
            RecordList.Clear();
            Sqlutil.CurrentDb.GetPageList(r => r.Deleted == 1, new PageModel() { PageIndex = 1, PageSize = 10 }).ForEach(r => RecordList.Add(r));
        }
        #endregion
    }
}