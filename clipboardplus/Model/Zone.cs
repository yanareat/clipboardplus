using GalaSoft.MvvmLight;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace clipboardplus.Model
{
    public class Zone : ObservableObject
    {
        public Zone()
        {
            this.Nodes = new ObservableCollection<Zone>();
        }

        #region 属性

        /// <summary>
        /// 分区树节点
        /// </summary>
        private ObservableCollection<Zone> _nodes;
        [SugarColumn(IsIgnore = true)]
        public ObservableCollection<Zone> Nodes
        {
            get => _nodes;
            set { _nodes = value; RaisePropertyChanged(() => Nodes); }
        }

        /// <summary>
        /// 分区序号
        /// </summary>
        private int _id;
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id
        {
            get => _id;
            set { _id = value; RaisePropertyChanged(() => Id); }
        }

        /// <summary>
        /// 分区名称
        /// </summary>
        private string _name;
        [SugarColumn(IsNullable = true)]
        public string Name
        {
            get => _name;
            set { _name = value; RaisePropertyChanged(() => Name); }
        }

        /// <summary>
        /// 分区父级
        /// </summary>
        private int _parent;
        public int Parent
        {
            get => _parent;
            set { _parent = value; RaisePropertyChanged(() => Parent); }
        }

        /// <summary>
        /// 是否展开
        /// </summary>
        private bool _isExpanded;
        [SugarColumn(IsNullable = true)]
        public bool IsExpanded
        {
            get => _isExpanded;
            set { _isExpanded = value; RaisePropertyChanged(() => IsExpanded); }
        }

        /// <summary>
        /// 是否选中
        /// </summary>
        private bool _isSelected;
        [SugarColumn(IsNullable = true)]
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; RaisePropertyChanged(() => IsSelected); }
        }

        /// <summary>
        /// 是否重命名
        /// </summary>
        private Visibility _isRename = Visibility.Collapsed;
        [SugarColumn(IsIgnore = true)]
        public Visibility IsRename
        {
            get => _isRename;
            set { _isRename = value; RaisePropertyChanged(() => IsRename); }
        }

        #endregion

        #region 方法
        /// <summary>
        /// 是否包含
        /// </summary>
        /// <param name="zone"></param>
        /// <returns></returns>
        public bool Contains(Zone zone)
        {
            bool isContain = (Id == zone.Id);
            if (isContain) return isContain;
            foreach(var n in Nodes)
            {
                isContain = n.Contains(zone);
                if (isContain) return isContain;
            }
            return isContain;
        }

        #endregion
    }

    public class Condition : ObservableObject
    {
        /// <summary>
        /// 开始时间
        /// </summary>
        private DateTime _startTime;
        public DateTime StartTime
        {
            get => _startTime;
            set { _startTime = value; RaisePropertyChanged(() => StartTime); }
        }

        /// <summary>
        /// 结束时间
        /// </summary>
        private DateTime _endTime;
        public DateTime EndTime
        {
            get => _endTime;
            set { _endTime = value; RaisePropertyChanged(() => EndTime); }
        }

        /// <summary>
        /// 分区
        /// </summary>
        private int _zone;
        public int Zone
        {
            get => _zone;
            set { _zone = value; RaisePropertyChanged(() => Zone); }
        }

        /// <summary>
        /// 包含内容
        /// </summary>
        private bool _hasContent;
        public bool HasContent
        {
            get => _hasContent;
            set { _hasContent = value; RaisePropertyChanged(() => HasContent); }
        }

        /// <summary>
        /// 包含回收站
        /// </summary>
        private bool _hasDeleted;
        public bool HasDeleted
        {
            get => _hasDeleted;
            set { _hasDeleted = value; RaisePropertyChanged(() => HasDeleted); }
        }

        /// <summary>
        /// 包含回收站
        /// </summary>
        private int _type;
        public int Type
        {
            get => _type;
            set { _type = value; RaisePropertyChanged(() => Type); }
        }

        /// <summary>
        /// 高级搜索
        /// </summary>
        private bool _advanced;
        public bool Advanced
        {
            get => _advanced;
            set { _advanced = value; RaisePropertyChanged(() => Advanced); }
        }

        /// <summary>
        /// 搜索文本
        /// </summary>
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; RaisePropertyChanged(() => SearchText); }
        }

        /// <summary>
        /// 提示文本
        /// </summary>
        private string _searchPlaceholder;
        public string SearchPlaceholder
        {
            get => _searchPlaceholder;
            set { _searchPlaceholder = value; RaisePropertyChanged(() => SearchPlaceholder); }
        }

        /// <summary>
        /// 存库开始时间
        /// </summary>
        public string StartTimeString
        {
            get => ((DateTimeOffset)StartTime).ToUnixTimeMilliseconds().ToString();
        }

        /// <summary>
        /// 存库结束时间
        /// </summary>
        public string EndTimeString
        {
            get => ((DateTimeOffset)EndTime).ToUnixTimeMilliseconds().ToString();
        }
    };

    /// <summary>
    /// 快捷键模型
    /// </summary>
    public class HotKeyModel
    {
        /// <summary>
        /// 设置项名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 设置项快捷键是否可用
        /// </summary>
        public bool IsUsable { get; set; }

        /// <summary>
        /// 是否勾选Ctrl按键
        /// </summary>
        public bool IsSelectCtrl { get; set; }

        /// <summary>
        /// 是否勾选Shift按键
        /// </summary>
        public bool IsSelectShift { get; set; }

        /// <summary>
        /// 是否勾选Alt按键
        /// </summary>
        public bool IsSelectAlt { get; set; }

        /// <summary>
        /// 选中的按键
        /// </summary>
        public Keys SelectKey { get; set; }

        /// <summary>
        /// 快捷键按键集合
        /// </summary>
        public static List<Keys> MyKeys
        {
            get
            {
                List<Keys> keys = new List<Keys>();
                var temp = Enum.GetValues(typeof(Keys));
                foreach(var i in temp)
                {
                    keys.Add((Keys)i);
                }
                return keys.Where(k => k.CompareTo(Keys.A) >= 0 && k.CompareTo(Keys.Z) <= 0).ToList();
            }
        }
    }

    /// <summary>
    /// 快捷键设置项枚举
    /// </summary>
    public enum EHotKeySetting
    {
        显示 = 0,
        隐藏 = 1,
        截图 = 2,
    }
}