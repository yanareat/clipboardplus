﻿using clipboardplus.Util;
using GalaSoft.MvvmLight;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace clipboardplus.Model
{
    [Table("record")]
    public class Record : ObservableObject
    {
        #region 属性
        /// <summary>
        /// 记录的序号
        /// </summary>
        private int _id;
        [Column("id")]
        [PrimaryKey]
        [AutoIncrement]
        public int Id
        {
            get => _id; 
            set { _id = value; RaisePropertyChanged(() => Id); }
        }

        /// <summary>
        /// 记录的标题
        /// </summary>
        private string _title;
        [Column("title")]
        [NotNull]
        public string Title
        {
            get => _title;
            set { _title = value;RaisePropertyChanged(() => Title); }
        }

        /// <summary>
        /// 记录的内容的MD5码
        /// </summary>
        private string _md5;
        [Column("md5")]
        [NotNull]
        public string MD5
        {
            get { return _md5 == null ? (Type == 1 ? ToolUtil.GetMD5Hash(TextData, 0) : ToolUtil.GetMD5Hash(ImageData)) : _md5; }
            set { _md5 = value; RaisePropertyChanged(() => MD5); }
        }

        /// <summary>
        /// 记录的种类
        /// 0未知
        /// 1文本
        /// 2图片
        /// 3文件
        /// </summary>
        private int _type;
        [Column("type")]
        [NotNull]
        public int Type
        {
            get => _type;
            set { _type = value; RaisePropertyChanged(() => Type); }
        }

        /// <summary>
        /// 记录的时间
        /// 通过 DateTimeOffset.Now.ToUnixTimeMilliseconds() 获得
        /// </summary>
        private string _time;
        [Column("time")]
        [NotNull]
        public string Time
        {
            get => _time;
            set { _time = value; RaisePropertyChanged(() => Time); }
        }

        /// <summary>
        /// 记录的来源
        /// </summary>
        private string _origin;
        [Column("origin")]
        public string Origin
        {
            get => _origin;
            set { _origin = value; RaisePropertyChanged(() => Origin); }
        }

        /// <summary>
        /// 记录的分区
        /// </summary>
        private int _zone;
        [Column("zone")]
        [NotNull]
        public int Zone
        {
            get => _zone;
            set { _zone = value; RaisePropertyChanged(() => Zone); }
        }

        /// <summary>
        /// 记录是否删除
        /// </summary>
        private int _deleted;
        [Column("deleted")]
        [NotNull]
        public int Deleted
        {
            get => _deleted;
            set { _deleted = value; RaisePropertyChanged(() => Deleted); }
        }

        /// <summary>
        /// 记录的文本数据
        /// </summary>
        private string _textData;
        [Column("text_data")]
        public string TextData
        {
            get => _textData;
            set { _textData = value; RaisePropertyChanged(() => TextData); }
        }

        /// <summary>
        /// 记录的HTML数据
        /// </summary>
        private string _htmlData;
        [Column("html_data")]
        public string HtmlData
        {
            get => _htmlData;
            set { _htmlData = value; RaisePropertyChanged(() => HtmlData); }
        }

        /// <summary>
        /// 记录的image数据
        /// 二进制
        /// </summary>
        private byte[] _imageData;
        [Column("image_data")]
        public byte[] ImageData
        {
            get => _imageData;
            set { _imageData = value; RaisePropertyChanged(() => ImageData); }
        }

        /// <summary>
        /// 记录的文件数据
        /// 二进制
        /// </summary>
        private byte[] _fileData;
        [Column("file_data")]
        public byte[] FileData
        {
            get => _fileData;
            set { _fileData = value; RaisePropertyChanged(() => FileData); }
        }
        #endregion

        #region 只读属性
        /// <summary>
        /// 记录的文本大小
        /// </summary>
        public int TextSize
        {
            get => _textData == null ? 0 : _textData.Length;
        }

        /// <summary>
        /// 记录的HTML大小
        /// </summary>
        public int HtmlSize
        {
            get => _htmlData == null ? 0: _htmlData.Length;
        }

        /// <summary>
        /// 记录的image大小
        /// </summary>
        public int ImageSize
        {
            get => _imageData == null ? 0 : _imageData.Length;
        }

        /// <summary>
        /// 记录的文件大小
        /// </summary>
        public int FileSize
        {
            get => _fileData == null ? 0 : _fileData.Length;
        }

        /// <summary>
        /// 记录的时间格式化
        /// </summary>
        public string TimeFormat
        {
            get => DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(_time)).ToLocalTime().ToString("yyyy.MM.dd HH:mm:ss");
        }

        /// <summary>
        /// 记录的时间格式化
        /// </summary>
        public Visibility TextShow
        {
            get => Type == 1 ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// 记录的种类图标
        /// </summary>
        public string TypeIcon
        {
            //get => Type != 1 ? @"F1 M 20 2.5 L 20 17.5 L 0 17.5 L 0 2.5 Z M 1.25 3.75 L 1.25 10.361328 L 5.625 5.996094 L 11.875 12.246094 L 14.375 9.746094 L 18.75 14.111328 L 18.75 3.75 Z M 1.25 16.25 L 14.111328 16.25 L 5.625 7.753906 L 1.25 12.138672 Z M 18.75 16.25 L 18.75 15.888672 L 14.375 11.503906 L 12.753906 13.125 L 15.888672 16.25 Z M 15.625 7.5 C 15.455729 7.5 15.309244 7.438151 15.185547 7.314453 C 15.061848 7.190756 14.999999 7.044271 15 6.875 C 14.999999 6.705729 15.061848 6.559245 15.185547 6.435547 C 15.309244 6.31185 15.455729 6.25 15.625 6.25 C 15.79427 6.25 15.940754 6.31185 16.064453 6.435547 C 16.18815 6.559245 16.25 6.705729 16.25 6.875 C 16.25 7.044271 16.18815 7.190756 16.064453 7.314453 C 15.940754 7.438151 15.79427 7.5 15.625 7.5 Z " 
            //    : @"F1 M 6.357422 8.75 L 9.6875 18.75 L 8.330078 18.75 L 7.5 16.25 L 3.75 16.25 L 2.919922 18.75 L 1.5625 18.75 L 4.892578 8.75 Z M 7.080078 15 L 5.625 10.625 L 4.169922 15 Z M 7.871094 5.3125 C 7.877604 4.863281 7.882486 4.417318 7.885742 3.974609 C 7.888997 3.531902 7.884114 3.085938 7.871094 2.636719 C 7.871094 2.604168 7.877604 2.568359 7.890625 2.529297 C 7.942708 2.516277 7.985025 2.509766 8.017578 2.509766 C 8.291016 2.516277 8.562825 2.522787 8.833008 2.529297 C 9.103189 2.535809 9.375 2.539063 9.648438 2.539063 L 12.333984 2.539063 C 12.333983 2.33073 12.332355 2.115887 12.329102 1.894531 C 12.325846 1.673178 12.298177 1.458334 12.246094 1.25 C 12.695312 1.25 13.141275 1.269531 13.583984 1.308594 C 13.623047 1.315105 13.663736 1.324871 13.706055 1.337891 C 13.748371 1.350912 13.76953 1.383465 13.769531 1.435547 C 13.76953 1.474609 13.759765 1.513672 13.740234 1.552734 C 13.720702 1.591797 13.704426 1.630859 13.691406 1.669922 C 13.678385 1.702475 13.666992 1.75293 13.657227 1.821289 C 13.647461 1.889648 13.64095 1.961264 13.637695 2.036133 C 13.634439 2.111004 13.632813 2.182617 13.632813 2.250977 C 13.632813 2.319336 13.632813 2.373047 13.632813 2.412109 L 13.632813 2.539063 L 16.523438 2.539063 C 16.796875 2.539063 17.068684 2.535809 17.338867 2.529297 C 17.609049 2.522787 17.880859 2.516277 18.154297 2.509766 C 18.199869 2.509766 18.242188 2.519531 18.28125 2.539063 C 18.300781 2.571615 18.310547 2.604168 18.310547 2.636719 C 18.304035 2.747396 18.297525 2.862957 18.291016 2.983398 C 18.284504 3.103842 18.28125 3.219402 18.28125 3.330078 L 18.28125 3.896484 C 18.28125 4.130859 18.284504 4.366862 18.291016 4.604492 C 18.297525 4.842123 18.304035 5.078125 18.310547 5.3125 C 18.310547 5.332031 18.308918 5.354818 18.305664 5.380859 C 18.302408 5.406901 18.29427 5.426433 18.28125 5.439453 C 18.229166 5.452475 18.186848 5.458984 18.154297 5.458984 C 18.121744 5.458984 18.046875 5.460612 17.929688 5.463867 C 17.8125 5.467123 17.688801 5.468751 17.558594 5.46875 C 17.428385 5.468751 17.30957 5.467123 17.202148 5.463867 C 17.094727 5.460612 17.03125 5.449219 17.011719 5.429688 C 16.998697 5.358074 16.990559 5.242514 16.987305 5.083008 C 16.984049 4.923503 16.984049 4.754232 16.987305 4.575195 C 16.990559 4.396159 16.993814 4.223633 16.99707 4.057617 C 17.000324 3.891602 17.001953 3.763021 17.001953 3.671875 L 9.169922 3.671875 C 9.169922 3.704428 9.171549 3.769531 9.174805 3.867188 C 9.17806 3.964844 9.179688 4.077148 9.179688 4.204102 C 9.179688 4.331055 9.179688 4.466146 9.179688 4.609375 C 9.179688 4.752605 9.179688 4.884441 9.179688 5.004883 C 9.179688 5.125326 9.176432 5.226237 9.169922 5.307617 C 9.163411 5.388998 9.153646 5.436198 9.140625 5.449219 C 9.088541 5.46224 9.046224 5.468751 9.013672 5.46875 L 8.017578 5.46875 C 7.945963 5.468751 7.903646 5.458985 7.890625 5.439453 C 7.877604 5.419923 7.871094 5.377605 7.871094 5.3125 Z M 18.574219 7.832031 C 18.587238 7.884115 18.59375 7.926433 18.59375 7.958984 L 18.59375 8.896484 C 18.59375 8.922526 18.587238 8.961589 18.574219 9.013672 C 18.522135 9.026693 18.479816 9.033203 18.447266 9.033203 C 18.186848 9.026693 17.929688 9.020183 17.675781 9.013672 C 17.421875 9.007162 17.164713 9.003906 16.904297 9.003906 L 13.769531 9.003906 C 13.763021 9.394531 13.764648 9.786784 13.774414 10.180664 C 13.78418 10.574545 13.795572 10.966797 13.808594 11.357422 C 13.815104 11.520183 13.789063 11.682943 13.730469 11.845703 C 13.671875 12.008464 13.574219 12.138672 13.4375 12.236328 C 13.346354 12.301433 13.217772 12.351889 13.051758 12.387695 C 12.885741 12.423503 12.708332 12.449545 12.519531 12.46582 C 12.330729 12.482097 12.14681 12.491862 11.967773 12.495117 C 11.788736 12.498373 11.647135 12.5 11.542969 12.5 C 11.503906 12.5 11.442057 12.498373 11.357422 12.495117 C 11.272786 12.491862 11.214192 12.473959 11.181641 12.441406 C 11.162109 12.421875 11.139322 12.369792 11.113281 12.285156 C 11.087239 12.200521 11.067708 12.141928 11.054688 12.109375 C 11.009114 11.940104 10.955403 11.782227 10.893555 11.635742 C 10.831705 11.489258 10.755208 11.341146 10.664063 11.191406 C 10.885416 11.217448 11.106771 11.235352 11.328125 11.245117 C 11.549479 11.254883 11.770833 11.259766 11.992188 11.259766 C 12.148438 11.259766 12.267252 11.235352 12.348633 11.186523 C 12.430013 11.137695 12.470703 11.028646 12.470703 10.859375 L 12.470703 9.003906 L 9.355469 9.003906 C 9.095052 9.003906 8.834635 9.007162 8.574219 9.013672 C 8.313802 9.020183 8.053385 9.026693 7.792969 9.033203 C 7.740885 9.033203 7.701823 9.023438 7.675781 9.003906 C 7.66276 8.964844 7.65625 8.929037 7.65625 8.896484 L 7.65625 7.958984 C 7.65625 7.939454 7.657877 7.91504 7.661133 7.885742 C 7.664388 7.856446 7.672526 7.835287 7.685547 7.822266 C 7.724609 7.809245 7.760417 7.802734 7.792969 7.802734 C 8.053385 7.809246 8.313802 7.815756 8.574219 7.822266 C 8.834635 7.828776 9.095052 7.832031 9.355469 7.832031 L 12.470703 7.832031 C 12.457682 7.662762 12.449543 7.491863 12.446289 7.319336 C 12.443033 7.146811 12.42513 6.975912 12.392578 6.806641 C 12.535807 6.819662 12.675781 6.832683 12.8125 6.845703 C 12.949219 6.858725 13.089192 6.871745 13.232422 6.884766 C 13.460286 6.722006 13.678385 6.54948 13.886719 6.367188 C 14.095052 6.184896 14.303385 5.99935 14.511719 5.810547 L 11.503906 5.810547 C 11.230469 5.810547 10.958658 5.813803 10.688477 5.820313 C 10.418294 5.826823 10.146484 5.833334 9.873047 5.839844 C 9.820963 5.839844 9.7819 5.830078 9.755859 5.810547 C 9.742838 5.771484 9.736328 5.735678 9.736328 5.703125 L 9.736328 4.775391 C 9.736328 4.74935 9.742838 4.710287 9.755859 4.658203 C 9.807942 4.645183 9.847005 4.638672 9.873047 4.638672 C 10.146484 4.645184 10.418294 4.651693 10.688477 4.658203 C 10.958658 4.664714 11.230469 4.667969 11.503906 4.667969 L 15.146484 4.667969 C 15.25065 4.667969 15.348307 4.65332 15.439453 4.624023 C 15.530599 4.594727 15.611979 4.580078 15.683594 4.580078 C 15.735676 4.580078 15.810546 4.619141 15.908203 4.697266 C 16.005859 4.775391 16.103516 4.866537 16.201172 4.970703 C 16.298828 5.07487 16.38509 5.180664 16.459961 5.288086 C 16.53483 5.395508 16.572266 5.475261 16.572266 5.527344 C 16.572266 5.592448 16.551105 5.642904 16.508789 5.678711 C 16.466471 5.71452 16.41927 5.745443 16.367188 5.771484 C 16.276041 5.817058 16.18815 5.875651 16.103516 5.947266 C 16.018879 6.018881 15.9375 6.08724 15.859375 6.152344 C 15.520832 6.432292 15.180663 6.704102 14.838867 6.967773 C 14.497069 7.231445 14.140624 7.483725 13.769531 7.724609 L 13.769531 7.832031 L 16.904297 7.832031 C 17.164713 7.832031 17.421875 7.828776 17.675781 7.822266 C 17.929688 7.815756 18.186848 7.809246 18.447266 7.802734 C 18.492838 7.802734 18.535156 7.8125 18.574219 7.832031 Z ";
            get => Type != 1 ? "\uEB9F" : "\uE164";
        }
        #endregion
    }
}