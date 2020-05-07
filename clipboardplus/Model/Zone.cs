using GalaSoft.MvvmLight;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        #endregion
    }
}
