using clipboardplus.Util;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace clipboardplus.Model
{
    public class Record : ObservableObject
    {
        #region 属性
        /// <summary>
        /// 记录的序号
        /// </summary>
        private int _id;
        public int Id
        {
            get => _id; 
            set { _id = value; RaisePropertyChanged(() => Id); }
        }

        /// <summary>
        /// 记录的标题
        /// </summary>
        private string _title;
        public string Title
        {
            get => _title;
            set { _title = value;RaisePropertyChanged(() => Title); }
        }

        /// <summary>
        /// 记录的内容的MD5码
        /// </summary>
        private string _md5;
        public string MD5
        {
            get => _md5;
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
        public int Type
        {
            get => _type;
            set { _type = value; RaisePropertyChanged(() => Type); }
        }

        /// <summary>
        /// 记录的时间
        /// 通过 DateTimeOffset.Now.ToUnixTimeMilliseconds() 获得
        /// </summary>
        private long _time;
        public long Time
        {
            get => _time;
            set { _time = value; RaisePropertyChanged(() => Time); }
        }

        /// <summary>
        /// 记录的来源
        /// </summary>
        private string _from;
        public string From
        {
            get => _from;
            set { _from = value; RaisePropertyChanged(() => From); }
        }

        /// <summary>
        /// 记录的分区
        /// </summary>
        private int _zone;
        public int Zone
        {
            get => _zone;
            set { _zone = value; RaisePropertyChanged(() => Zone); }
        }

        /// <summary>
        /// 记录是否删除
        /// </summary>
        private int _deleted;
        public int Deleted
        {
            get => _deleted;
            set { _deleted = value; RaisePropertyChanged(() => Deleted); }
        }

        /// <summary>
        /// 记录的文本数据
        /// </summary>
        private string _textData;
        public string TextData
        {
            get => _textData;
            set { _textData = value; RaisePropertyChanged(() => TextData); }
        }

        /// <summary>
        /// 记录的HTML数据
        /// </summary>
        private string _htmlData;
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
            get => _textData.Length;
        }

        /// <summary>
        /// 记录的HTML大小
        /// </summary>
        public int HtmlSize
        {
            get => _htmlData.Length;
        }

        /// <summary>
        /// 记录的image大小
        /// </summary>
        public int ImageSize
        {
            get => _imageData.Length;
        }

        /// <summary>
        /// 记录的文件大小
        /// </summary>
        public int FileSize
        {
            get => _fileData.Length;
        }

        /// <summary>
        /// 记录的二进制还原成图片
        /// </summary>
        public BitmapImage ImageSource
        {
            get => ToolUtil.ConvertToBitmap(_imageData);
        }

        /// <summary>
        /// 记录的时间格式化
        /// </summary>
        public string TimeFormat
        {
            get => DateTimeOffset.FromUnixTimeMilliseconds(_time).ToLocalTime().ToString("yyyy.MM.dd HH:mm:ss");
        }

        /// <summary>
        /// 记录的时间格式化
        /// </summary>
        public Visibility TextShow
        {
            get => Type == 1 ? Visibility.Visible : Visibility.Collapsed;
        }
        #endregion
    }
}