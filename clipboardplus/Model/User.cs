using GalaSoft.MvvmLight;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace clipboardplus.Model
{
    public class User : ObservableObject
    {
        #region 属性
        /// <summary>
        /// 用户的账号
        /// </summary>
        private string _name;
        [SugarColumn(IsPrimaryKey = true)]
        public string Name
        {
            get => _name;
            set { _name = value; RaisePropertyChanged(() => Name); }
        }

        /// <summary>
        /// 用户的密码
        /// </summary>
        private string _password;
        public string Password
        {
            get => _password;
            set { _password = value; RaisePropertyChanged(() => Password); }
        }

        /// <summary>
        /// 用户的重复密码
        /// </summary>
        private string _confirm;
        [SugarColumn(IsIgnore = true)]
        public string Confirm
        {
            get => _confirm;
            set { _confirm = value; RaisePropertyChanged(() => Confirm); }
        }

        /// <summary>
        /// 用户的注册时间
        /// </summary>
        private DateTime _createTime;
        public DateTime CreateTime
        {
            get => _createTime;
            set { _createTime = value; RaisePropertyChanged(() => CreateTime); }
        }

        /// <summary>
        /// 用户的邮箱
        /// </summary>
        private string _email;
        public string Email
        {
            get => _email;
            set { _email = value; RaisePropertyChanged(() => Email); }
        }

        /// <summary>
        /// 用户的手机
        /// </summary>
        private string _phone;
        [SugarColumn(IsNullable = true)]
        public string Phone
        {
            get => _phone;
            set { _phone = value; RaisePropertyChanged(() => Phone); }
        }

        /// <summary>
        /// 用户是否删除
        /// </summary>
        private bool _deleted;
        [SugarColumn(IsNullable = true)]
        public bool Deleted
        {
            get => _deleted;
            set { _deleted = value; RaisePropertyChanged(() => Deleted); }
        }

        /// <summary>
        /// 账号错误提示
        /// </summary>
        private string _nameTip;
        [SugarColumn(IsIgnore =true)]
        public string NameTip
        {
            get => _nameTip;
            set { _nameTip = value; RaisePropertyChanged(() => NameTip); }
        }

        /// <summary>
        /// 密码错误提示
        /// </summary>
        private string _passwordTip;
        [SugarColumn(IsIgnore = true)]
        public string PasswordTip
        {
            get => _passwordTip;
            set { _passwordTip = value; RaisePropertyChanged(() => PasswordTip); }
        }

        /// <summary>
        /// 密码错误提示
        /// </summary>
        private string _confirmTip;
        [SugarColumn(IsIgnore = true)]
        public string ConfirmTip
        {
            get => _confirmTip;
            set { _confirmTip = value; RaisePropertyChanged(() => ConfirmTip); }
        }

        /// <summary>
        /// 邮箱错误提示
        /// </summary>
        private string _emailTip;
        [SugarColumn(IsIgnore = true)]
        public string EmailTip
        {
            get => _emailTip;
            set { _emailTip = value; RaisePropertyChanged(() => EmailTip); }
        }

        /// <summary>
        /// 手机错误提示
        /// </summary>
        private string _phoneTip;
        [SugarColumn(IsIgnore = true)]
        public string PhoneTip
        {
            get => _phoneTip;
            set { _phoneTip = value; RaisePropertyChanged(() => PhoneTip); }
        }

        /// <summary>
        /// 手机错误提示
        /// </summary>
        private string _iP;
        [SugarColumn(IsIgnore = true)]
        public string IP
        {
            get => _iP;
            set { _iP = value; RaisePropertyChanged(() => IP); }
        }

        /// <summary>
        /// 手机错误提示
        /// </summary>
        private string _serverIP;
        [SugarColumn(IsIgnore = true)]
        public string ServerIP
        {
            get => _serverIP;
            set { _serverIP = value; RaisePropertyChanged(() => ServerIP); }
        }

        /// <summary>
        /// 手机错误提示
        /// </summary>
        private Visibility _loginVis;
        [SugarColumn(IsIgnore = true)]
        public Visibility LoginVis
        {
            get => _loginVis;
            set { _loginVis = value; RaisePropertyChanged(() => LoginVis); }
        }
        /// <summary>
        /// 手机错误提示
        /// </summary>
        private Visibility _registerVis;
        [SugarColumn(IsIgnore = true)]
        public Visibility RegisterVis
        {
            get => _registerVis;
            set { _registerVis = value; RaisePropertyChanged(() => RegisterVis); }
        }
        /// <summary>
        /// 手机错误提示
        /// </summary>
        private Visibility _infoVis;
        [SugarColumn(IsIgnore = true)]
        public Visibility InfoVis
        {
            get => _infoVis;
            set { _infoVis = value; RaisePropertyChanged(() => InfoVis); }
        }

        /// <summary>
        /// 手机错误提示
        /// </summary>
        private Visibility _nameVis;
        [SugarColumn(IsIgnore = true)]
        public Visibility NameVis
        {
            get => _nameVis;
            set { _nameVis = value; RaisePropertyChanged(() => NameVis); }
        }
        /// <summary>
        /// 手机错误提示
        /// </summary>
        private Visibility _phoneVis;
        [SugarColumn(IsIgnore = true)]
        public Visibility PhoneVis
        {
            get => _phoneVis;
            set { _phoneVis = value; RaisePropertyChanged(() => PhoneVis); }
        }
        /// <summary>
        /// 手机错误提示
        /// </summary>
        private Visibility _emailVis;
        [SugarColumn(IsIgnore = true)]
        public Visibility EmailVis
        {
            get => _emailVis;
            set { _emailVis = value; RaisePropertyChanged(() => EmailVis); }
        }
        /// <summary>
        /// 是否开启自启动
        /// </summary>
        private Visibility _isAsync;
        [SugarColumn(IsIgnore = true)]
        public Visibility IsAsync
        {
            get => _isAsync;
            set { _isAsync = value; RaisePropertyChanged(() => IsAsync); }
        }

        /// <summary>
        /// 是否开启自启动
        /// </summary>
        private Visibility _isShare;
        [SugarColumn(IsIgnore = true)]
        public Visibility IsShare
        {
            get => _isShare;
            set { _isShare = value; RaisePropertyChanged(() => IsShare); }
        }
        /// <summary>
        /// 是否开启自启动
        /// </summary>
        private Visibility _isRun;
        [SugarColumn(IsIgnore = true)]
        public Visibility IsRun
        {
            get => _isRun;
            set { _isRun = value; RaisePropertyChanged(() => IsRun); }
        }

        /// <summary>
        /// 是否登录
        /// </summary>
        private bool _isLogin;
        [SugarColumn(IsIgnore = true)]       
        public bool IsLogin
        {
            get => _isLogin;
            set { _isLogin = value; RaisePropertyChanged(() => IsLogin); }
        }

        /// <summary>
        /// 用户的头像数据
        /// 二进制
        /// </summary>
        private byte[] _gravatar;
        [SugarColumn(IsNullable = true)]
        public byte[] Gravatar
        {
            get => _gravatar;
            set { _gravatar = value; RaisePropertyChanged(() => Gravatar); }
        }

        #endregion

        #region 只读属性
        
        #endregion

        public void resetTip()
        {
            NameTip = "请输入账号";
            PasswordTip = "请输入密码";
            ConfirmTip = "请再次输入密码";
            PhoneTip = "请输入手机号";
            EmailTip = "请输入邮箱";
        }
    }
}