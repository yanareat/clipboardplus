using clipboard_;
using clipboardplus.Model;
using clipboardplus.Models;
using clipboardplus.Util;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Win32;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Condition = clipboardplus.Model.Condition;
using TabControl = System.Windows.Controls.TabControl;

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
                // Code runs "for real"
                
                // 移动窗口用,误删
                // NativeMethods.SendMessage(Hwnd, (int)WindowMessages.WM_NCLBUTTONDOWN, (IntPtr)HitTest.HTCAPTION, IntPtr.Zero);

                InitData();
            }
        }

        #region 属性
        /// <summary>
        /// 数据库工具类
        /// </summary>
        private SqlUtil<Record> _sqlutil;
        public SqlUtil<Record> Sqlutil
        {
            get => _sqlutil;
            set { _sqlutil = value; RaisePropertyChanged(() => Sqlutil); }
        }

        /// <summary>
        /// 数据库工具类
        /// </summary>
        private SqlUtil<User> _remoteSqlutil;
        public SqlUtil<User> RemoteSqlutil
        {
            get => _remoteSqlutil;
            set { _remoteSqlutil = value; RaisePropertyChanged(() => RemoteSqlutil); }
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
        /// 用户自身
        /// </summary>
        private User _myself;
        public User Myself
        {
            get => _myself;
            set { _myself = value; RaisePropertyChanged(() => Myself); }
        }

        /// <summary>
        /// 历史、笔记、搜索记录列表
        /// </summary>
        private ObservableCollection<Record> _recordList;
        public ObservableCollection<Record> RecordList
        {
            get => _recordList;
            set { _recordList = value; RaisePropertyChanged(() => RecordList); }
        }

        /// <summary>
        /// 分区树
        /// </summary>
        private ObservableCollection<Zone> _zoneTree;
        public ObservableCollection<Zone> ZoneTree
        {
            get => _zoneTree;
            set { _zoneTree = value; RaisePropertyChanged(() => ZoneTree); }
        }

        /// <summary>
        /// 分区树
        /// </summary>
        private ObservableCollection<Zone> _zoneList;
        public ObservableCollection<Zone> ZoneList
        {
            get => _zoneList;
            set { _zoneList = value; RaisePropertyChanged(() => ZoneList); }
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
        /// 剪贴的记录
        /// </summary>
        private Record _clipRecord;
        public Record ClipRecord
        {
            get => _clipRecord;
            set { _clipRecord = value; RaisePropertyChanged(() => ClipRecord); }
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
        /// 选择的Tab
        /// </summary>
        private int _selectedTab;
        public int SelectedTab
        {
            get => _selectedTab;
            set { _selectedTab = value; RaisePropertyChanged(() => SelectedTab); }
        }

        /// <summary>
        /// 选择的Tab
        /// </summary>
        private int _recordVersion;
        public int RecordVersion
        {
            get => _recordVersion;
            set { _recordVersion = value; RaisePropertyChanged(() => RecordVersion); }
        }

        /// <summary>
        /// 选择的Tab
        /// </summary>
        private int _selectedLeftTab;
        public int SelectedLeftTab
        {
            get => _selectedLeftTab;
            set { _selectedLeftTab = value; RaisePropertyChanged(() => SelectedLeftTab); }
        }

        /// <summary>
        /// 是否创建桌面快捷方式
        /// </summary>
        private bool _isCreateDesktopQuick;
        public bool IsCreateDesktopQuick
        {
            get => _isCreateDesktopQuick;
            set { _isCreateDesktopQuick = value; RaisePropertyChanged(() => IsCreateDesktopQuick); }
        }

        /// <summary>
        /// 是否开启自启动
        /// </summary>
        private bool _isSetMeAutoStart;
        public bool IsSetMeAutoStart
        {
            get => _isSetMeAutoStart;
            set { _isSetMeAutoStart = value; RaisePropertyChanged(() => IsSetMeAutoStart); }
        }

        /// <summary>
        /// 搜索条件
        /// </summary>
        private Condition _searchCondition;
        public Condition SearchCondition
        {
            get => _searchCondition;
            set { _searchCondition = value; RaisePropertyChanged(() => SearchCondition); }
        }
        
        /// <summary>
        /// 快捷键设置项集合
        /// </summary>
        private ObservableCollection<HotKeyModel> _hotKeyList = new ObservableCollection<HotKeyModel>();
        public ObservableCollection<HotKeyModel> HotKeyList
        {
            get => _hotKeyList; 
            set { _hotKeyList = value; RaisePropertyChanged(() => HotKeyList); }
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
                    ?? (_zoneSelectionChanged = new RelayCommand<RoutedPropertyChangedEventArgs<object>>(e => changedSelectedZone(e)));
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
        private RelayCommand<RoutedEventArgs> _recordMenuSelected;
        public RelayCommand<RoutedEventArgs> RecordMenuSelected
        {
            get
            {
                return _recordMenuSelected
                    ?? (_recordMenuSelected = new RelayCommand<RoutedEventArgs>(e => changeRecordZone(e)));
            }
        }

        private RelayCommand _toLoginClick;
        public RelayCommand ToLoginClick
        {
            get
            {
                return _toLoginClick
                    ?? (_toLoginClick = new RelayCommand(() => {
                        Myself.Name = "";
                        Myself.Password = "";
                        Myself.resetTip();
                        Myself.InfoVis = Visibility.Collapsed;
                        Myself.RegisterVis = Visibility.Collapsed;
                        Myself.LoginVis = Visibility.Visible;
                    }));
            }
        }

        private RelayCommand _loginClick;
        public RelayCommand LoginClick
        {
            get
            {
                return _loginClick
                    ?? (_loginClick = new RelayCommand(() => toLogin()));
            }
        }

        private RelayCommand _toRegisterClick;
        public RelayCommand ToRegisterClick
        {
            get
            {
                return _toRegisterClick
                    ?? (_toRegisterClick = new RelayCommand(() => {
                        Myself.Name = "";
                        Myself.Password = "";
                        Myself.resetTip();
                        Myself.InfoVis = Visibility.Collapsed;
                        Myself.LoginVis = Visibility.Collapsed;
                        Myself.RegisterVis = Visibility.Visible;
                    }));
            }
        }

        private RelayCommand _registerClick;
        public RelayCommand RegisterClick
        {
            get
            {
                return _registerClick
                    ?? (_registerClick = new RelayCommand(() => toRegister()));
            }
        }

        private RelayCommand _asyncRecord;
        public RelayCommand AsyncRecord
        {
            get
            {
                return _asyncRecord
                    ?? (_asyncRecord = new RelayCommand(() => {
                        Myself.IsAsync = Visibility.Visible;
                        ToolUtil.ToAsync(() => {

                            syncDB();

                            ToolUtil.ToSync(() => {
                                Myself.IsAsync = Visibility.Collapsed;
                            });
                        });
                    }));
            }
        }

        /// <summary>
        /// 连接服务器
        /// </summary>
        private RelayCommand _connectClick;
        public RelayCommand ConnectClick
        {
            get
            {
                return _connectClick
                    ?? (_connectClick = new RelayCommand(() => toConnect()));
            }
        }

        private RelayCommand _openClick;
        public RelayCommand OpenClick
        {
            get
            {
                return _openClick
                    ?? (_openClick = new RelayCommand(() => toOpen()));
            }
        }

        /// <summary>
        /// 删除记录或分区
        /// </summary>
        private RelayCommand<ObservableObject> _deleteRecordOrZone;
        public RelayCommand<ObservableObject> DeleteRecordOrZone
        {
            get
            {
                return _deleteRecordOrZone
                    ?? (_deleteRecordOrZone = new RelayCommand<ObservableObject>(e => deleteRecordOrZone(e)));
            }
        }

        /// <summary>
        /// 删除记录或分区
        /// </summary>
        private RelayCommand<ObservableObject> _recycleRecordOrZone;
        public RelayCommand<ObservableObject> RecycleRecordOrZone
        {
            get
            {
                return _recycleRecordOrZone
                    ?? (_recycleRecordOrZone = new RelayCommand<ObservableObject>(e => recycleRecordOrZone(e)));
            }
        }

        /// <summary>
        /// 删除记录或分区
        /// </summary>
        private RelayCommand<ObservableObject> _newRecordOrZone;
        public RelayCommand<ObservableObject> NewRecordOrZone
        {
            get
            {
                return _newRecordOrZone
                    ?? (_newRecordOrZone = new RelayCommand<ObservableObject>(e => newRecordOrZone(e)));
            }
        }

        /// <summary>
        /// 选择Tab触发
        /// </summary>
        private RelayCommand<MouseButtonEventArgs> _recordDoubleClick;
        public RelayCommand<MouseButtonEventArgs> RecordDoubleClick
        {
            get
            {
                return _recordDoubleClick
                    ?? (_recordDoubleClick = new RelayCommand<MouseButtonEventArgs>(e => SendRecordToClip(e)));
            }
        }

        /// <summary>
        /// 选择Tab触发
        /// </summary>
        private RelayCommand<MouseButtonEventArgs> _phoneEdit;
        public RelayCommand<MouseButtonEventArgs> PhoneEdit
        {
            get
            {
                return _phoneEdit
                    ?? (_phoneEdit = new RelayCommand<MouseButtonEventArgs>(e => {
                        switch (e.ClickCount)
                        {
                            case 1://单击
                                {
                                    break;
                                }
                            case 2://双击
                                {
                                    Myself.PhoneVis = Visibility.Visible;
                                    e.Handled = true;
                                    break;
                                }
                        }
                    }));
            }
        }
        /// <summary>
        /// 选择Tab触发
        /// </summary>
        private RelayCommand<MouseButtonEventArgs> _emailEdit;
        public RelayCommand<MouseButtonEventArgs> EmailEdit
        {
            get
            {
                return _emailEdit
                    ?? (_emailEdit = new RelayCommand<MouseButtonEventArgs>(e => {
                        switch (e.ClickCount)
                        {
                            case 1://单击
                                {
                                    break;
                                }
                            case 2://双击
                                {
                                    Myself.EmailVis = Visibility.Visible;
                                    e.Handled = true;
                                    break;
                                }
                        }
                    }));
            }
        }

        /// <summary>
        /// 搜索框输入回车
        /// </summary>
        private RelayCommand _searchBarEnter;
        public RelayCommand SearchBarEnter
        {
            get
            {
                return _searchBarEnter
                    ?? (_searchBarEnter = new RelayCommand(() => {
                        if (SelectedLeftTab == 4)
                        {
                            RecordList.Clear();
                            RecordList.Add(new Record() { Id = -1, Time = DateTime.Now });
                            //异步加载记录列表
                            var temp = GetRecordList();
                            if (temp.Count != 0)
                            {
                                ToolUtil.ToAsync((a) => LoadRecordList(a), temp);
                            }
                            else
                            {
                                RecordList.Remove(RecordList.FirstOrDefault(r => r.Id == -1));
                            }
                        }
                        SelectedLeftTab = 4;
                        //SearchRecord();
                    }));
            }
        }

        /// <summary>
        /// 搜索框输入回车
        /// </summary>
        private RelayCommand _renameBarEnter;
        public RelayCommand RenameBarEnter
        {
            get
            {
                return _renameBarEnter
                    ?? (_renameBarEnter = new RelayCommand(() => {
                        SelectedZone.IsRename = Visibility.Collapsed;
                        RenameZone(SelectedZone);
                        //SearchRecord();
                    }));
            }
        }

        /// <summary>
        /// 注册快捷键
        /// </summary>
        private RelayCommand<MouseButtonEventArgs> _zoneDoubleClick;
        public RelayCommand<MouseButtonEventArgs> ZoneDoubleClick
        {
            get
            {
                return _zoneDoubleClick
                    ?? (_zoneDoubleClick = new RelayCommand<MouseButtonEventArgs>((e) => {
                        if(e.ClickCount == 2)
                        {
                            SelectedZone.IsRename = Visibility.Visible;
                        }
                        //SelectedZone;
                    }));
            }
        }

        /// <summary>
        /// 注册快捷键
        /// </summary>
        private RelayCommand _btnSaveSetting;
        public RelayCommand BtnSaveSetting
        {
            get
            {
                return _btnSaveSetting
                    ?? (_btnSaveSetting = new RelayCommand(() => btnSaveSetting()));
            }
        }

        /// <summary>
        /// 注册快捷键
        /// </summary>
        private RelayCommand<KeyEventArgs> _sendToClip;
        public RelayCommand<KeyEventArgs> SendToClip
        {
            get
            {
                return _sendToClip
                    ?? (_sendToClip = new RelayCommand<KeyEventArgs>((e) => {
                        Console.WriteLine(e.OriginalSource.GetType()+"888"+ e.Key.ToString());
                        Console.WriteLine("<<<<<<<<<<<<<<<<<<<<<<<");
                        if(e.Key == Key.Enter)
                        {
                            sendToClip();
                        }
                        e.Handled = true;
                    }));
            }
        }

        /// <summary>
        /// 注册快捷键
        /// </summary>
        private RelayCommand<KeyEventArgs> _phoneEnter;
        public RelayCommand<KeyEventArgs> PhoneEnter
        {
            get
            {
                return _phoneEnter
                    ?? (_phoneEnter = new RelayCommand<KeyEventArgs>((e) => {
                        if (e.Key == Key.Enter)
                        {
                            Myself.PhoneVis = Visibility.Collapsed;
                            RemoteSqlutil.CurrentDb.Update(u => new User() { Phone = Myself.Phone }, u => u.Name == Myself.Name);
                            e.Handled = true;
                        }
                    }));
            }
        }
        /// <summary>
        /// 注册快捷键
        /// </summary>
        private RelayCommand<KeyEventArgs> _emailEnter;
        public RelayCommand<KeyEventArgs> EmailEnter
        {
            get
            {
                return _emailEnter
                    ?? (_emailEnter = new RelayCommand<KeyEventArgs>((e) => {
                        if (e.Key == Key.Enter)
                        {
                            Myself.EmailVis = Visibility.Collapsed;
                            RemoteSqlutil.CurrentDb.Update(u => new User() { Email = Myself.Email }, u => u.Name == Myself.Name);
                            e.Handled = true;
                        }
                    }));
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
        /// 初始化数据
        /// </summary>
        private void InitData()
        {
            try
            {               
                var list = HotKeySettingsManager.Instance.LoadDefaultHotKey();
                list.ToList().ForEach(x => HotKeyList.Add(x));
                IsCreateDesktopQuick = true;
                IsSetMeAutoStart = true;
                ToolUtil.CreateDesktopQuick(IsCreateDesktopQuick);
                ToolUtil.SetMeAutoStart(IsSetMeAutoStart);

                string AddressIP = "未获取到IP";
                foreach (IPAddress _IPAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
                {
                    if (_IPAddress.AddressFamily.ToString() == "InterNetwork")
                    {
                        AddressIP = _IPAddress.ToString();
                    }
                }

                Myself = new User()
                {
                    IsLogin = false,
                    LoginVis = Visibility.Visible,
                    RegisterVis = Visibility.Collapsed,
                    InfoVis = Visibility.Collapsed,
                    NameVis = Visibility.Collapsed,
                    PhoneVis = Visibility.Collapsed,
                    EmailVis = Visibility.Collapsed,
                    IsAsync = Visibility.Collapsed,
                    IsRun = Visibility.Collapsed,
                    IsShare = Visibility.Collapsed,
                    IP = AddressIP
                };
                Myself.resetTip();

                SelectedTab = 0;
                SelectedLeftTab = 0;
                SearchCondition = new Condition()
                {
                    StartTime = DateTime.Now,
                    EndTime = DateTime.Now,
                    Zone = 0,
                    HasContent = false,
                    HasDeleted = false,
                    Type = 0,
                    Advanced = false,
                    SearchText = "",
                    SearchPlaceholder = "请输入标题（左侧高级搜索）"
                };
                Console.WriteLine(" -----------------1-----------------\n");

                Sqlutil = new SqlUtil<Record>(new StringBuilder("DataSource=D:/5_desktop/clipboardplus.db"), DbType.Sqlite);
                RemoteSqlutil = new SqlUtil<User>(new StringBuilder("server=39.106.1.127;Database=clipboardplus;Uid=yanareat;Pwd=222430"), DbType.MySql);

                Console.WriteLine(" -----------------2-----------------\n");
                //异步加载记录列表
                RecordList = new ObservableCollection<Record>();
                RecordList.Add(new Record() { Id = -1, Time = DateTime.Now });
                var temp = GetRecordList();
                if (temp.Count != 0)
                {
                    ToolUtil.ToAsync((a) => LoadRecordList(a), temp);
                }
                else
                {
                    RecordList.Remove(RecordList.FirstOrDefault(r => r.Id == -1));
                }
                Console.WriteLine(" -----------------3-----------------\n");


                var tempZone = Sqlutil.ZoneDb.GetSingle(z => z.Parent == 0);
                if(tempZone == null)
                {
                    tempZone = new Zone() { Parent = 0, Name = "根分区", IsSelected = true, Nodes = new ObservableCollection<Zone>(), Version = 0 };
                    Sqlutil.ZoneDb.Insert(tempZone);
                }
                ZoneList = new ObservableCollection<Zone>(Sqlutil.ZoneDb.GetList(z => z.Parent != -1));
                ZoneTree = ToolUtil.getTrees(0, ZoneList);
                Console.WriteLine(" -----------------3.5-----------------\n");
                SelectedZone = Sqlutil.ZoneDb.GetSingle(r => r.IsSelected == true );

                var t = Sqlutil.CurrentDb.AsQueryable();
                if (t.Count() > 0)
                {
                    RecordVersion = int.Parse(ConfigurationManager.AppSettings["version"]);
                }
                else
                {
                    RecordVersion = 0;
                    Configuration cfa = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    cfa.AppSettings.Settings["version"].Value = RecordVersion.ToString();
                    cfa.Save();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(" -----------------ERROR-----------------\n" + e.Message);
            }
        }

        /// <summary>
        /// 剪贴监听函数
        /// </summary>
        public void ClipboardListener()
        {
            Console.WriteLine("\n\n\n\n\n -----------------%%%%%%%%%%%-----------------\n\n\n\n\n");
            Console.WriteLine(" -----------------5-----------------\n");
            ClipRecord = new Record();
            ClipRecord.Time = DateTime.Now;
            ClipRecord.Origin = ToolUtil.ClipFrom();
            ClipRecord.Zone = 1;
            ClipRecord.Version = RecordVersion;

            var clipData = Clipboard.GetDataObject();
            if (clipData.GetDataPresent(DataFormats.Text))
            {
                Console.WriteLine(" -----------------10-----------------\n");
                var text = (Clipboard.GetData(DataFormats.Text) as string);
                ClipRecord.Type = 1;
                ClipRecord.Title = text.Trim().Length > 16 ? text.Trim().Substring(0, 16) : text.Trim();
                ClipRecord.TextData = text;
                ClipRecord.HtmlData = text;
                ClipRecord.MD5 = ToolUtil.GetMD5Hash(ClipRecord.TextData, 0) + ToolUtil.GetMD5Hash(ClipRecord.Origin, 0);
                Console.WriteLine(" -----------------7-----------------\n");
                if (clipData.GetDataPresent(DataFormats.Html))
                {
                    Console.WriteLine(" -----------------10-----------------\n");
                    var doc = (Clipboard.GetData(DataFormats.Html) as string);
                    Console.WriteLine(doc);
                    var num = doc.IndexOf("<html");
                    var html = doc.Substring(num);
                    ClipRecord.HtmlData = html;
                    ClipRecord.MD5 = ToolUtil.GetMD5Hash(ClipRecord.HtmlData, 0) + ToolUtil.GetMD5Hash(ClipRecord.Origin, 0);
                    Console.WriteLine(" -----------------7-----------------\n");
                }
            }
            if (clipData.GetDataPresent(DataFormats.Bitmap))
            {
                Console.WriteLine(" -----------------9-----------------\n");
                ClipRecord.Type = 2;
                ClipRecord.Title = "图片";
                ClipRecord.ImageData = ToolUtil.ConvertToBytes(Clipboard.GetImage());
                ClipRecord.MD5 = ToolUtil.GetMD5Hash(ClipRecord.ImageData) + ToolUtil.GetMD5Hash(ClipRecord.Origin, 0);
                Console.WriteLine(" -----------------6-----------------\n");
            }

            Console.WriteLine(" -----------------10-----------------\n");
            //更新DB
            var tempToDB = Sqlutil.CurrentDb.GetSingle(r => r.MD5 == ClipRecord.MD5);
            if (tempToDB != null)
            {
                Console.WriteLine(" -----------------11-----------------\n");
                ClipRecord.Id = tempToDB.Id;
                Sqlutil.CurrentDb.Update(c => new Record() { Time = ClipRecord.Time },c => c.Id == ClipRecord.Id);
            }
            else
            {
                Console.WriteLine(" -----------------12-----------------\n");
                Sqlutil.CurrentDb.Insert(ClipRecord);
                ClipRecord.Id = Sqlutil.CurrentDb.GetSingle(r => r.MD5 == ClipRecord.MD5).Id;
            }

            Console.WriteLine(" -----------------13-----------------\n");

            int i;
            for (i = 0; i < RecordList.Count; i++)
            {
                if(RecordList[i].MD5 == ClipRecord.MD5)
                {
                    RecordList.RemoveAt(i);
                    break;
                }
            }
            if (SelectedLeftTab != 0)
            {
                if (i != RecordList.Count)
                {
                    RecordList.Insert(i, ClipRecord);
                }            
            }
            else
            {
                RecordList.Insert(0, ClipRecord);
            }

            Console.WriteLine(" -----------------8-----------------\n");
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
                var temp = ToolUtil.DeepCopy(SelectedRecord);
                if (temp.Type == 1)
                {
                    ToEditTextRecord = temp;
                }
                else
                {
                    ToEditImageRecord = temp;
                }
            }
            e.Handled = true;
        }

        /// <summary>
        /// 改变文本编辑记录的内容
        /// </summary>
        /// <param name="e"></param>
        private void changeBookRecordList()
        {
            RecordList.Clear();
            RecordList.Add(new Record() { Id = -1, Time = DateTime.Now });
            //异步加载记录列表
            var temp = GetRecordList();
            if (temp.Count != 0)
            {
                ToolUtil.ToAsync((a) => LoadRecordList(a), temp);
            }
            else
            {
                RecordList.Remove(RecordList.FirstOrDefault(r => r.Id == -1));
            }
            //ToolUtil.ToAsync(() => {
            //    List<Record> temp = new List<Record>();
            //    Thread.Sleep(500);
            //    temp = Sqlutil.CurrentDb.GetPageList(r => r.Deleted == false && r.Zone == SelectedZone.Id, new PageModel() { PageIndex = 1, PageSize = 10 });
            //    ToolUtil.ToSync((a) => a.ForEach(r => RecordList.Insert(0, r)), temp);
            //    //Thread.Sleep(500);
            //    ToolUtil.ToSync(() => {
            //        RecordList.Remove(RecordList.FirstOrDefault(r => r.Id == -1));
            //    });
            //});
        }

        private void changedSelectedZone(RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue != null)
            {
                SelectedZone = e.NewValue as Zone;
                var tempZone = e.OldValue as Zone;
                //ToolUtil.ToAsync(() => {
                if (tempZone != null)
                        Sqlutil.ZoneDb.Update(c => new Zone() { IsExpanded = tempZone.IsExpanded,IsSelected = tempZone.IsSelected }, c => c.Id == tempZone.Id);
                Sqlutil.ZoneDb.Update(c => new Zone() { IsExpanded = SelectedZone.IsExpanded,IsSelected = SelectedZone.IsSelected }, c => c.Id == SelectedZone.Id);
                //});
                changeBookRecordList();
            }
            e.Handled = true;
        }

        /// <summary>
        /// 改变记录列表的内容
        /// </summary>
        private void changeRecordList(SelectionChangedEventArgs e)
        {
            Console.WriteLine(e.OriginalSource.GetType().ToString() + "-------" + e.Source.GetType().ToString());
            RecordList.Clear();
            RecordList.Add(new Record() { Id = -1, Time = DateTime.Now });
            //SelectedLeftTab = (e.Source as TabControl).SelectedIndex;
            //异步加载记录列表
            var temp = GetRecordList();
            if(temp.Count != 0)
            {
                ToolUtil.ToAsync((a) => LoadRecordList(a), temp);
            }
            else
            {
                RecordList.Remove(RecordList.FirstOrDefault(r => r.Id == -1));
            }            

            e.Handled = true;
        }

        /// <summary>
        /// 加载记录列表
        /// </summary>
        /// <param name="index"></param>
        private void LoadRecordList(List<Record> temp)
        {
            Thread.Sleep(500);
            ToolUtil.ToSync((a) => a.ForEach(r => RecordList.Add(r)), temp);
            //Thread.Sleep(500);
            ToolUtil.ToSync(() => {
                RecordList.Remove(RecordList.FirstOrDefault(r => r.Id == -1));            
            });
        }

        /// <summary>
        /// 重新构建分区树
        /// </summary>
        /// <param name="e"></param>
        private void resetZoneTree(RoutedEventArgs e)
        {
            var mi = e.OriginalSource as MenuItem;
            var zone = mi.DataContext as Zone;
            Console.WriteLine(zone.Name);
            if (!SelectedZone.Contains(zone) && SelectedZone.Id != 0)
            {
                Sqlutil.ZoneDb.Update(z => new Zone() { Parent = zone.Id }, z => z.Id == SelectedZone.Id);
                ZoneList = new ObservableCollection<Zone>(Sqlutil.ZoneDb.GetList(z => z.Parent != -2));
                ZoneTree = ToolUtil.getTrees(-1, ZoneList);
            }
            e.Handled = true;
        }

        /// <summary>
        /// 改变记录的分区
        /// </summary>
        /// <param name="e"></param>
        private void changeRecordZone(RoutedEventArgs e)
        {
            var mi = e.OriginalSource as MenuItem;
            var zone = mi.DataContext as Zone;
            SelectedRecord.Zone = zone.Id;
            Sqlutil.CurrentDb.Update(c => new Record() { Zone = SelectedRecord.Zone }, c => c.Id == SelectedRecord.Id);
            e.Handled = true;
        }

        /// <summary>
        /// 删除记录或分区
        /// </summary>
        /// <param name="e"></param>
        private void deleteRecordOrZone(ObservableObject e)
        {
            //Console.WriteLine(e.GetType().Name.ToString());
            var type = e.GetType();
            if (type.Name.Equals("Record"))
            {
                RecordList.Remove(SelectedRecord);
                if (SelectedRecord.Deleted == false)
                {
                    SelectedRecord.Deleted = true;
                    Sqlutil.CurrentDb.Update(c => new Record() { Deleted = SelectedRecord.Deleted }, c => c.Id == SelectedRecord.Id);
                }
                else
                {                    
                    Sqlutil.CurrentDb.Delete(SelectedRecord);                    
                    SelectedRecord = null;
                }
            }
            else if(type.Name.Equals("Zone") && SelectedZone.Id != -1)
            {
                Sqlutil.ZoneDb.Delete(SelectedZone);
                Sqlutil.ZoneDb.Update(z => new Zone() { IsSelected = true }, z => z.Id == SelectedZone.Parent);
                ZoneList = new ObservableCollection<Zone>(Sqlutil.ZoneDb.GetList(z => z.Parent != -1));
                ZoneTree = ToolUtil.getTrees(0, ZoneList);
            }
        }

        /// <summary>
        /// 删除记录或分区
        /// </summary>
        /// <param name="e"></param>
        private void recycleRecordOrZone(ObservableObject e)
        {
            //Console.WriteLine(e.GetType().Name.ToString());
            var type = e.GetType();
            if (type.Name.Equals("Record"))
            {
                RecordList.Remove(SelectedRecord);
                if (SelectedRecord.Deleted == true)
                {
                    SelectedRecord.Deleted = false;
                    Sqlutil.CurrentDb.Update(c => new Record() { Deleted = SelectedRecord.Deleted }, c => c.Id == SelectedRecord.Id);
                }
                else
                {
                    //Sqlutil.CurrentDb.Delete(SelectedRecord);
                    //SelectedRecord = null;
                }
            }
            else if (type.Name.Equals("Zone") && SelectedZone.Id != -1)
            {
                //Sqlutil.ZoneDb.Delete(SelectedZone);
                //Sqlutil.ZoneDb.Update(z => new Zone() { IsSelected = true }, z => z.Id == SelectedZone.Parent);
                //ZoneList = new ObservableCollection<Zone>(Sqlutil.ZoneDb.GetList(z => z.Parent != -1));
                //ZoneTree = ToolUtil.getTrees(0, ZoneList);
            }
        }

        /// <summary>
        /// 新建记录或分区
        /// </summary>
        /// <param name="e"></param>
        private void newRecordOrZone(ObservableObject e)
        {
            var type = e.GetType();
            if (type.Name.Equals("Record"))
            {
                //RecordList.Remove(SelectedRecord);
                //if (SelectedRecord.Deleted == false)
                //{
                //    SelectedRecord.Deleted = true;
                //    Sqlutil.CurrentDb.Update(c => new Record() { Deleted = SelectedRecord.Deleted }, c => c.Id == SelectedRecord.Id);
                //}
                //else
                //{
                //    Sqlutil.CurrentDb.Delete(SelectedRecord);
                //    SelectedRecord = null;
                //}
            }
            else if (type.Name.Equals("Zone"))
            {
                Sqlutil.ZoneDb.Insert(new Zone()
                {
                    Name = "未命名",
                    Parent = SelectedZone.Id,
                    Version = Sqlutil.ZoneDb.AsQueryable().Count() > 0 ? Sqlutil.ZoneDb.AsQueryable().Max(z => z.Version) : 0
                });
                ZoneList = new ObservableCollection<Zone>(Sqlutil.ZoneDb.GetList(z => z.Parent != -1));
                ZoneTree = ToolUtil.getTrees(0, ZoneList);
            }
        }

        /// <summary>
        /// 发送记录到剪贴板
        /// </summary>
        /// <param name="e"></param>
        private void SendRecordToClip(MouseButtonEventArgs e)
        {
            switch (e.ClickCount)
            {
                case 1://单击
                    {
                        var temp = e.OriginalSource;
                        if (temp.GetType().Name.Equals("TextBlock"))
                        {
                            if(((temp as TextBlock).DataContext as Record).Type == 1)
                            {
                                SelectedTab = 0;
                            }
                            else
                            {
                                SelectedTab = 1;
                            }
                        }
                        else if (temp.GetType().Name.Equals("Image"))
                        {
                            SelectedTab = 1;
                        }
                        break;
                    }
                case 2://双击
                    {
                        if (SelectedRecord.Type == 1)
                        {
                            Clipboard.SetText(SelectedRecord.TextData);
                        }
                        else if (SelectedRecord.Type == 2)
                        {
                            Console.WriteLine(SelectedRecord.Type);
                            Clipboard.SetImage(ToolUtil.ConvertToBitmap(SelectedRecord.ImageData));
                        }
                        e.Handled = true;
                        break;
                    }
            }
        }

        /// <summary>
        /// 展示被删除的记录
        /// </summary>
        /// <param name="e"></param>
        private void showDeletedRecordList(MouseButtonEventArgs e)
        {
            RecordList.Clear();
            RecordList.Add(new Record() { Id = -1 });
            ToolUtil.ToAsync(() => {
                List<Record> temp = new List<Record>();
                Thread.Sleep(500);
                temp = Sqlutil.CurrentDb.GetPageList(r => r.Deleted == true, new PageModel() { PageIndex = 1, PageSize = 10 });
                ToolUtil.ToSync((a) => a.ForEach(r => RecordList.Insert(0, r)), temp);
                //Thread.Sleep(500);
                ToolUtil.ToSync(() => {
                    RecordList.Remove(RecordList.FirstOrDefault(r => r.Id == -1));
                });
            });
            e.Handled = true;
        }

        /// <summary>
        /// 搜索记录
        /// </summary>
        private List<Record> GetRecordList()
        {
            List<Record> temp = new List<Record>();
            switch (SelectedLeftTab)
            {
                case 0:
                    temp = Sqlutil.CurrentDb.GetPageList(r => r.Deleted == false && r.Time < RecordList[RecordList.Count() - 1].Time, new PageModel() { PageIndex = 1, PageSize = 10 }, r => r.Time, OrderByType.Desc);
                    break;
                case 1:
                    temp = Sqlutil.CurrentDb.GetPageList(r => r.Deleted == false && r.Zone == SelectedZone.Id && r.Time < RecordList[RecordList.Count() - 1].Time, new PageModel() { PageIndex = 1, PageSize = 10 }, r => r.Time, OrderByType.Desc);
                    break;
                case 4:
                    {
                        Console.WriteLine("搜索");
                        Console.WriteLine(SearchCondition.StartTimeString);
                        Console.WriteLine(SearchCondition.EndTimeString);
                        Console.WriteLine(SearchCondition.Zone);
                        Console.WriteLine(SearchCondition.HasContent);
                        Console.WriteLine(SearchCondition.HasDeleted);
                        Console.WriteLine(SearchCondition.Type);
                        Console.WriteLine(SearchCondition.Advanced);
                        Console.WriteLine(SearchCondition.SearchText);
                        Console.WriteLine(SearchCondition.SearchPlaceholder);
                        //1589009226000
                        //1589009226000
                        //0
                        //False
                        //False
                        //0
                        //False
                        //SelectedZone
                        //return Sqlutil.CurrentDb.GetPageList(r => r.Deleted == false && r.Type == 1, new PageModel() { PageIndex = 1, PageSize = 10 });
                        //SqlFunc

                        var queryable = Sqlutil.CurrentDb.AsQueryable();
                        if (SearchCondition.Advanced == false)
                        {
                            queryable = queryable.Where(r => r.Deleted == false);
                            if (SearchCondition.SearchText != "")
                            {
                                queryable = queryable.Where(r => r.Title.Contains(SearchCondition.SearchText));
                            }
                            temp = queryable.Where(r => r.Time < RecordList[RecordList.Count() - 1].Time).OrderBy(r => r.Time, OrderByType.Desc).ToPageList(1, 10);
                            break;
                        }

                        if (SearchCondition.StartTime < SearchCondition.EndTime)
                        {
                            queryable = queryable.Where(r => r.Time >= SearchCondition.StartTime && r.Time < SearchCondition.EndTime);
                        }
                        else if (SearchCondition.StartTime > SearchCondition.EndTime)
                        {
                            ToolUtil.ToSync(() => {
                                SearchCondition.SearchPlaceholder = "时间区间错误，重新输入！";
                            });
                            return new List<Record>();
                        }

                        if (SearchCondition.HasDeleted == false)
                        {
                            queryable = queryable.Where(r => r.Deleted == false);
                        }

                        if (SearchCondition.Type != 0)
                        {
                            queryable = queryable.Where(r => r.Type == SearchCondition.Type);
                        }

                        //有点问题，对分区处理不好
                        if (SearchCondition.Zone != 0)
                        {
                            queryable = queryable.Where(r => r.Zone == SearchCondition.Zone);
                        }

                        if (SearchCondition.SearchText != "")
                        {

                            if (SearchCondition.HasContent == true)
                            {
                                queryable = queryable.Where(r => r.Title.Contains(SearchCondition.SearchText) || r.TextData.Contains(SearchCondition.SearchText));
                            }
                            else
                            {
                                queryable = queryable.Where(r => r.Title.Contains(SearchCondition.SearchText));
                            }
                        }

                        temp = queryable.Where(r => r.Time < RecordList[RecordList.Count() - 1].Time).OrderBy(r => r.Time, OrderByType.Desc).ToPageList(1, 10);
                        break;
                    }                    
                default:
                    break;
            }
            return temp;
        }

        public void LoadNewRecordList()
        {
            var temp = GetRecordList();
            if (temp.Count != 0)
            {
                RecordList.Add(new Record() { Id = -1, Time = RecordList[RecordList.Count - 1].Time });
                ToolUtil.ToAsync((a) => LoadRecordList(a), temp);
            }
        }

        private void RenameZone(Zone zone)
        {
            Sqlutil.ZoneDb.Update(z => new Zone() { Name = zone.Name }, r => r.Id == zone.Id);
        }

        public void saveRecord()
        {

            Console.WriteLine("ToSave...");
            ToolUtil.ToAsync(() => {
                Thread.Sleep(1000);         
                if(SelectedTab == 0)
                {
                    ToEditTextRecord.MD5 = ToolUtil.GetMD5Hash(ToEditTextRecord.HtmlData, 0) + ToolUtil.GetMD5Hash(ToEditTextRecord.Origin, 0);
                    var temp = RecordList.AsQueryable().Where(r => r.Id == ToEditTextRecord.Id).ToList()[0];
                    temp.Title = ToEditTextRecord.Title;
                    temp.MD5 = ToEditTextRecord.MD5;
                    temp.TextData = ToEditTextRecord.TextData;
                    temp.HtmlData = ToEditTextRecord.HtmlData;
                    Sqlutil.CurrentDb.Update(r => new Record()
                    {
                        Title = ToEditTextRecord.Title,
                        MD5 = ToEditTextRecord.MD5,
                        TextData = ToEditTextRecord.TextData,
                        HtmlData = ToEditTextRecord.HtmlData
                    }, r => r.Id == ToEditTextRecord.Id);
                }else if(SelectedTab == 1 || SelectedTab ==2)
                {
                    ToEditImageRecord.MD5 = ToolUtil.GetMD5Hash(ToEditImageRecord.ImageData) + ToolUtil.GetMD5Hash(ToEditImageRecord.Origin, 0);
                    var temp = RecordList.AsQueryable().Where(r => r.Id == ToEditImageRecord.Id).ToList()[0];
                    temp.Title = ToEditImageRecord.Title;
                    temp.MD5 = ToEditImageRecord.MD5;
                    temp.ImageSource = ToEditImageRecord.ImageData;
                    Sqlutil.CurrentDb.Update(r => new Record()
                    {
                        Title = ToEditImageRecord.Title,
                        MD5 = ToEditImageRecord.MD5,
                        ImageData = ToEditImageRecord.ImageData
                    }, r => r.Id == ToEditImageRecord.Id);
                }           
            });
            Console.WriteLine("Saved...");
        }

        public void beOCR()
        {

            Console.WriteLine("ToOCR...");
            string temp = OCRUtils.Img2Tex(ToEditImageRecord.ImageData).ToString();
            Console.WriteLine(temp);
            ToEditImageRecord.TextData = temp;
        }

        private void toLogin()
        {
            Console.WriteLine(Myself.Name + "\n" + Myself.Password);
            var temp = RemoteSqlutil.CurrentDb.GetSingle(u => u.Name == Myself.Name);
            if(temp != null){
                if(temp.Password != Myself.Password)
                {
                    Myself.Password = "";
                    Myself.PasswordTip = "密码输入错误";
                }
                else
                {
                    Myself.Email = temp.Email;
                    Myself.Phone = temp.Phone;
                    Myself.CreateTime = temp.CreateTime;
                    Myself.Gravatar = temp.Gravatar;
                    Myself.Deleted = temp.Deleted;
                    Myself.InfoVis = Visibility.Visible;
                    Myself.LoginVis = Visibility.Collapsed;
                }
            }
            else
            {
                Myself.Name = "";
                Myself.NameTip = "账号输入错误";
            }
        }

        private void toRegister()
        {
            if(Myself.Password == Myself.Confirm)
            {
                Console.WriteLine(Myself.Name + "\n" + Myself.Password + "\n" + Myself.Email);
                var temp = RemoteSqlutil.CurrentDb.GetSingle(u => u.Name == Myself.Name);
                if (temp != null)
                {
                    Myself.Name = "";
                    Myself.NameTip = "账号已存在";
                }
                else
                {
                    Myself.CreateTime = DateTime.Now;
                    RemoteSqlutil.CurrentDb.Insert(Myself);
                    Myself.Name = "";
                    Myself.Password = "";
                    Myself.resetTip();
                    Myself.RegisterVis = Visibility.Collapsed;
                    Myself.LoginVis = Visibility.Visible;
                }
            }
            else
            {
                Myself.Confirm = "";
                Myself.ConfirmTip = "与密码不一致，请重新输入";
            }
        }

        private void syncDB()
        {
            var temp = Sqlutil.CurrentDb.AsQueryable();
            var tempRe = RemoteSqlutil.RecordDb.AsQueryable().Where(re => re.User == Myself.Name);

            Console.WriteLine(temp.Count() + ")))))))))))))))))))))))))");
            Console.WriteLine(tempRe.Count() + ")))))))))))))))))))))))))");

            int V;
            if (temp.Count() > 0)
            {
                V = temp.Max(r => r.Version);
                Console.WriteLine(V + ")))))))))))))))))))))))))");
            }
            else
            {
                V = 0;
            }

            int reV;
            if (tempRe.Count() > 0)
            {
                reV = tempRe.Max(re => re.Version);
                Console.WriteLine(reV + ")))))))))))))))))))))))))");
            }
            else
            {
                reV = 0;
            }
            
            if (V == 0)
            {
                Console.WriteLine(">>>>0------" + ")))))))))))))))))))))))))");
                if (temp.Count() > 0)
                {
                    Sqlutil.CurrentDb.Update(r => new Record() { Version = reV + 1 }, r => r.Version == 0);
                    RecordVersion = reV + 1;
                }
                
                var upList = RemoteSqlutil.RecordDb.GetList(r => r.User == Myself.Name);
                upList.ForEach(r => {
                    Console.WriteLine(r.Id);
                    Sqlutil.CurrentDb.Insert(r);
                });                
                syncDB();
            }
            else
            {
                if(V < reV)
                {
                    Console.WriteLine(">>>>  <------" + ")))))))))))))))))))))))))");
                    List<Record> upList = Sqlutil.CurrentDb.GetList(r => r.Version == V);
                    List<Record> upReList = RemoteSqlutil.RecordDb.GetList(r => r.Version >= V && r.Version <= reV);
                    upList.ForEach(r => {
                        r.User = Myself.Name;
                    });
                    RemoteSqlutil.RecordDb.InsertRange(upList);
                    Sqlutil.CurrentDb.InsertRange(upReList);
                    RecordVersion = reV;
                    syncDB();
                }
                else if (V > reV)
                {
                    Console.WriteLine(">>>>  >------" + ")))))))))))))))))))))))))");
                    List<Record> upList = Sqlutil.CurrentDb.GetList(r => r.Version > reV);
                    upList.ForEach(r => {
                        r.User = Myself.Name;
                    });
                    RemoteSqlutil.RecordDb.InsertRange(upList);
                    syncDB();
                }
                else if (V == reV)
                {
                    Console.WriteLine(">>>>  ==------" + ")))))))))))))))))))))))))");
                    RecordVersion = V + 1;
                    Configuration cfa = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);                    
                    cfa.AppSettings.Settings["version"].Value = RecordVersion.ToString();                   
                    cfa.Save();
                }
            }
        }

        /// <summary>
        /// 保存设置
        /// </summary>
        /// <param name="e"></param>
        private void btnSaveSetting()
        {
            ToolUtil.CreateDesktopQuick(IsCreateDesktopQuick);
            ToolUtil.SetMeAutoStart(IsSetMeAutoStart);
            if (!HotKeySettingsManager.Instance.RegisterGlobalHotKey(HotKeyList))
                return;
            //TODO 保存设置
        }

        /// <summary>
        /// 发送到剪贴板
        /// </summary>
        public void sendToClip()
        {
            try
            {
                Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>>");
                if (SelectedRecord.Type == 1)
                {
                    Console.WriteLine("666666666666666666666");
                    Clipboard.SetText(SelectedRecord.TextData);
                }
                else if (SelectedRecord.Type == 2)
                {
                    Console.WriteLine("44444444444444444444444");
                    Clipboard.SetImage(ToolUtil.ConvertToBitmap(SelectedRecord.ImageData));
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// 测试方法
        /// </summary>
        /// <param name="e"></param>
        private void test(MouseButtonEventArgs e)
        {
            //MessageBox.Show("1");
            //Console.WriteLine(e.OriginalSource.GetType().ToString());
            e.Handled = true;
        }
        #endregion

        #region 服务端
        //当开始监听时候，在服务器创建一个负责IP地址跟端口号的Socket
        Socket socketWatch;

        private void toOpen()
        {
            Myself.IsRun = Visibility.Visible;
            ToolUtil.ToAsync(() =>
            {
                try
                {
                    if (Myself.IP != "未获取到IP")
                    {
                        IPAddress ip = IPAddress.Any;
                        //当开始监听时候，在服务器创建一个负责IP地址跟端口号的Socket
                        socketWatch = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        IPEndPoint point = new IPEndPoint(ip, 9090);
                        //监听
                        socketWatch.Bind(point);
                        //ShowMsg("监听成功");              
                        socketWatch.Listen(10);

                        ListenClickConnect(socketWatch);

                        socketWatch.Close();
                    }
                    ToolUtil.ToSync(() =>
                    {
                        Myself.IsRun = Visibility.Collapsed;
                    });
                }
                catch (Exception error)
                {
                    Console.WriteLine(error.Message);
                }
            });
        }
        public void ListenClickConnect(object o)
        {
            Socket socketWatch = o as Socket;
            byte[] fileByte = new byte[1024];
            try
            {
                Socket socketSend = socketWatch.Accept();//等到客户端新的连接
                string path = "D:/5_desktop/clipboardplus.db";
                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                fileByte = new Byte[fs.Length];
                //将文件读到imgByte中。
                fs.Read(fileByte, 0, fileByte.Length);
                fs.Close();
                socketSend.Send(fileByte);
                socketSend.Shutdown(System.Net.Sockets.SocketShutdown.Send);
                socketSend.Close();
                socketSend.Dispose();
            }
            catch (Exception error)
            {
                Console.WriteLine(error.Message);
            }
        }
        #endregion

        #region 客户端
        //监听客户端发来的请求 socketWatch为监听 secketSend为通信
        Socket socketReceive;

        private void toConnect()
        {
            Myself.IsShare = Visibility.Visible;
            ToolUtil.ToAsync(() =>
            {
                try
                {
                    //负责通信的Socket
                    socketReceive = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    IPAddress ip = IPAddress.Parse(Myself.ServerIP);//拿到本机IP地址
                    IPEndPoint point = new IPEndPoint(ip, 9090);//连接的IP地址和端口号

                    //获得要连接的远程服务器应用程序的IP 地址和端口号
                    socketReceive.Connect(point);

                    Receive(socketReceive);

                    ToolUtil.ToSync(() =>
                    {
                        Myself.IsShare = Visibility.Collapsed;
                    });
                }
                catch (Exception error)
                {
                    Console.WriteLine(error.Message);
                }
            });
        }
        /// <summary>
        /// 不停的接收从服务器发来的消息
        /// </summary>
        void Receive(object o)
        {
            Socket hostSocket = o as Socket;
            //设置接收数据缓冲区的大小
            byte[] b = new byte[4096];
            try
            {
                //内存流fs的初始容量大小为0，随着数据增长而扩展。
                MemoryStream fs = new MemoryStream();
                int length = 0;
                //每接受一次，只能读取小于等于缓冲区的大小4096个字节
                while ((length = hostSocket.Receive(b)) > 0)
                {               
                    //将接受到的数据b，按长度length放到内存流中。
                    fs.Write(b, 0, length);
                }
                
                fs.Flush();
                fs.Seek(0, SeekOrigin.Begin);
                byte[] byteArray = new byte[fs.Length];
                int count = 0;
                while (count < fs.Length)
                {
                    byteArray[count] = Convert.ToByte(fs.ReadByte());
                    count++;
                }
                
                string Path = "D:/5_desktop/clipboardplus1.db";
                File.WriteAllBytes(Path, byteArray);//能用
                //关闭写文件流
                fs.Close();
                //关闭接收数据的Socket
                hostSocket.Shutdown(SocketShutdown.Receive);
                hostSocket.Close();
            }
            catch (Exception error)
            {
                Console.WriteLine(error.Message);
            }
        }

        #endregion
    }
}