using clipboardplus.Model;
//using HandyControl.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using IWshRuntimeLibrary;
using System.Xml.Serialization;
using Path = System.IO.Path;
using System.Drawing;
using Point = System.Windows.Point;
using System.Windows.Controls;
using System.Windows.Media;

namespace clipboardplus.Util
{
    class ToolUtil
    {
        public static bool IsVerticalScrollBarAtButtom(ScrollViewer s, double dVer)
        {
            bool isAtButtom = false;
            double dViewport = s.ViewportHeight;
            double dExtent = s.ExtentHeight;
            if (dVer != 0 && dVer + dViewport == dExtent)
            {
                isAtButtom = true;
            }
            return isAtButtom;
        }

        public static byte[] ConvertToBytes(BitmapSource bitmapSource)
        {
            byte[] buffer = null;
            BmpBitmapEncoder encoder = new BmpBitmapEncoder();
            //JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            MemoryStream memoryStream = new MemoryStream();
            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
            encoder.Save(memoryStream);
            memoryStream.Position = 0;
            if (memoryStream.Length > 0)
            {
                using (BinaryReader br = new BinaryReader(memoryStream))
                {
                    buffer = br.ReadBytes((int)memoryStream.Length);
                }
            }
            memoryStream.Close();
            return buffer;
        }

        public static Bitmap ConvertToBitmapImage(byte[] ImageData)
        {
            Bitmap img = null;
            try
            {
                if (ImageData != null && ImageData.Length != 0)
                {
                    MemoryStream ms = new MemoryStream(ImageData);
                    img = new Bitmap(ms);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return img;
        }

        public static BitmapImage ConvertToBitmap(byte[] ImageData)
        {
            Console.WriteLine(ImageData.Length);
            BitmapImage bi = null;
            if (ImageData != null)
            {
                bi = new BitmapImage();
                Console.WriteLine("开始转化");
                bi.BeginInit();
                bi.StreamSource = new MemoryStream(ImageData);
                bi.EndInit();
                Console.WriteLine("转化完成");
            }
            Console.WriteLine(bi == null);
            return bi;
        }

        public static BitmapImage ConvertToBitmapImage(Canvas canvas)
        {
            int width = (int)canvas.ActualWidth;
            int height = (int)canvas.ActualHeight;
            RenderTargetBitmap bmp = new RenderTargetBitmap(width, height, 96d, 96d, PixelFormats.Pbgra32);
            bmp.Render(canvas);
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bmp));
            BitmapImage bitmapImage = new BitmapImage();
            var memoryStream = new MemoryStream();
            encoder.Save(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = memoryStream;
            bitmapImage.EndInit();
            return bitmapImage;
        }

        public static BitmapSource GetPartImage(BitmapImage bitmap, int XCoordinate, int YCoordinate, int Width, int Height)
        {
            return new CroppedBitmap(bitmap, new Int32Rect(XCoordinate, YCoordinate, Width, Height));
        }

        /// <summary>
        /// 得到文件,字符串的MD5码
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetMD5Hash(string fileName, int type)
        {
            try
            {
                byte[] retVal = null;
                MD5 md5 = new MD5CryptoServiceProvider();
                if (type == 0)
                {
                    byte[] data = Encoding.GetEncoding("utf-8").GetBytes(fileName);
                    retVal = md5.ComputeHash(data);
                }
                else
                {
                    FileStream file = new FileStream(fileName, FileMode.Open);
                    retVal = md5.ComputeHash(file);
                    file.Close();
                }
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("GetMD5Hash() fail,error:" + ex.Message);
            }
        }

        public static string GetMD5Hash(byte[] bytedata)
        {
            try
            {
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(bytedata);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("GetMD5Hash() fail,error:" + ex.Message);
            }
        }

        #region Win32 API
        [DllImport("user32.dll")]
        public static extern IntPtr GetClipboardOwner();

        [DllImport("user32.dll")]
        public static extern IntPtr GetOpenClipboardWindow();

        [DllImport("user32.dll")]
        public static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);

        [DllImport("kernel32.dll")]
        public static extern bool CloseHandle(IntPtr handle);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        /// <summary>   
        /// 获取鼠标的坐标   
        /// </summary>   
        /// <param name="lpPoint">传址参数，坐标point类型</param>   
        /// <returns>获取成功返回真</returns>   
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool GetCursorPos(out Point pt);
        #endregion

        #region 热键管理

        /// <summary>
        /// 热键消息
        /// </summary>
        public const int WM_HOTKEY = 0x312;

        /// <summary>
        /// 注册热键
        /// </summary>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, ModifierKeys fsModifuers, int vk);

        /// <summary>
        /// 注销热键
        /// </summary>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        /// <summary>
        /// 向原子表中添加全局原子
        /// </summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern short GlobalAddAtom(string lpString);

        /// <summary>
        /// 在表中搜索全局原子
        /// </summary>
        /// <param name="lpString">字符串，这个字符串的长度最大为255字节</param>
        /// <returns>成功：返回原子；失败：返回值为0</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern short GlobalFindAtom(string lpString);

        /// <summary>
        /// 在表中删除全局原子
        /// </summary>
        /// <param name="nAtom">全局原子</param>
        /// <returns>成功：返回原子；失败：返回值为0</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern short GlobalDeleteAtom(short nAtom);

        /// <summary>
        /// 记录快捷键注册项的唯一标识符
        /// </summary>
        private static Dictionary<EHotKeySetting, int> m_HotKeySettingsDic = new Dictionary<EHotKeySetting, int>();

        /// <summary>
        /// 注册全局快捷键
        /// </summary>
        /// <param name="hotKeyModelList">待注册快捷键项</param>
        /// <param name="hwnd">窗口句柄</param>
        /// <param name="hotKeySettingsDic">快捷键注册项的唯一标识符字典</param>
        /// <returns>返回注册失败项的拼接字符串</returns>
        public static string RegisterGlobalHotKey(IEnumerable<HotKeyModel> hotKeyModelList, IntPtr hwnd, out Dictionary<EHotKeySetting, int> hotKeySettingsDic)
        {
            string failList = string.Empty;
            foreach (var item in hotKeyModelList)
            {
                if (!RegisterHotKey(item, hwnd))
                {
                    string str = string.Empty;
                    if (item.IsSelectCtrl && !item.IsSelectShift && !item.IsSelectAlt)
                    {
                        str = ModifierKeys.Control.ToString();
                    }
                    else if (!item.IsSelectCtrl && item.IsSelectShift && !item.IsSelectAlt)
                    {
                        str = ModifierKeys.Shift.ToString();
                    }
                    else if (!item.IsSelectCtrl && !item.IsSelectShift && item.IsSelectAlt)
                    {
                        str = ModifierKeys.Alt.ToString();
                    }
                    else if (item.IsSelectCtrl && item.IsSelectShift && !item.IsSelectAlt)
                    {
                        str = string.Format("{0}+{1}", ModifierKeys.Control.ToString(), ModifierKeys.Shift);
                    }
                    else if (item.IsSelectCtrl && !item.IsSelectShift && item.IsSelectAlt)
                    {
                        str = string.Format("{0}+{1}", ModifierKeys.Control.ToString(), ModifierKeys.Alt);
                    }
                    else if (!item.IsSelectCtrl && item.IsSelectShift && item.IsSelectAlt)
                    {
                        str = string.Format("{0}+{1}", ModifierKeys.Shift.ToString(), ModifierKeys.Alt);
                    }
                    else if (item.IsSelectCtrl && item.IsSelectShift && item.IsSelectAlt)
                    {
                        str = string.Format("{0}+{1}+{2}", ModifierKeys.Control.ToString(), ModifierKeys.Shift.ToString(), ModifierKeys.Alt);
                    }
                    if (string.IsNullOrEmpty(str))
                    {
                        str += item.SelectKey;
                    }
                    else
                    {
                        str += string.Format("+{0}", item.SelectKey);
                    }
                    str = string.Format("{0} ({1})\n\r", item.Name, str);
                    failList += str;
                }
            }
            hotKeySettingsDic = m_HotKeySettingsDic;
            return failList;
        }

        /// <summary>
        /// 注册热键
        /// </summary>
        /// <param name="hotKeyModel">热键待注册项</param>
        /// <param name="hWnd">窗口句柄</param>
        /// <returns>成功返回true，失败返回false</returns>
        private static bool RegisterHotKey(HotKeyModel hotKeyModel, IntPtr hWnd)
        {
            var fsModifierKey = new ModifierKeys();
            var hotKeySetting = (EHotKeySetting)Enum.Parse(typeof(EHotKeySetting), hotKeyModel.Name);

            if (!m_HotKeySettingsDic.ContainsKey(hotKeySetting))
            {
                // 全局原子不会在应用程序终止时自动删除。每次调用GlobalAddAtom函数，必须相应的调用GlobalDeleteAtom函数删除原子。
                if (GlobalFindAtom(hotKeySetting.ToString()) != 0)
                {
                    GlobalDeleteAtom(GlobalFindAtom(hotKeySetting.ToString()));
                }
                // 获取唯一标识符
                m_HotKeySettingsDic[hotKeySetting] = GlobalAddAtom(hotKeySetting.ToString());
            }
            else
            {
                // 注销旧的热键
                UnregisterHotKey(hWnd, m_HotKeySettingsDic[hotKeySetting]);
            }
            if (!hotKeyModel.IsUsable)
                return true;

            // 注册热键
            if (hotKeyModel.IsSelectCtrl && !hotKeyModel.IsSelectShift && !hotKeyModel.IsSelectAlt)
            {
                fsModifierKey = ModifierKeys.Control;
            }
            else if (!hotKeyModel.IsSelectCtrl && hotKeyModel.IsSelectShift && !hotKeyModel.IsSelectAlt)
            {
                fsModifierKey = ModifierKeys.Shift;
            }
            else if (!hotKeyModel.IsSelectCtrl && !hotKeyModel.IsSelectShift && hotKeyModel.IsSelectAlt)
            {
                fsModifierKey = ModifierKeys.Alt;
            }
            else if (hotKeyModel.IsSelectCtrl && hotKeyModel.IsSelectShift && !hotKeyModel.IsSelectAlt)
            {
                fsModifierKey = ModifierKeys.Control | ModifierKeys.Shift;
            }
            else if (hotKeyModel.IsSelectCtrl && !hotKeyModel.IsSelectShift && hotKeyModel.IsSelectAlt)
            {
                fsModifierKey = ModifierKeys.Control | ModifierKeys.Alt;
            }
            else if (!hotKeyModel.IsSelectCtrl && hotKeyModel.IsSelectShift && hotKeyModel.IsSelectAlt)
            {
                fsModifierKey = ModifierKeys.Shift | ModifierKeys.Alt;
            }
            else if (hotKeyModel.IsSelectCtrl && hotKeyModel.IsSelectShift && hotKeyModel.IsSelectAlt)
            {
                fsModifierKey = ModifierKeys.Control | ModifierKeys.Shift | ModifierKeys.Alt;
            }

            return RegisterHotKey(hWnd, m_HotKeySettingsDic[hotKeySetting], fsModifierKey, (int)hotKeyModel.SelectKey);
        }

        #endregion


        /// <summary>
        /// 获取剪贴板来源
        /// </summary>
        /// <returns></returns>
        public static string ClipFrom()
        {
            IntPtr vOwner = GetClipboardOwner();
            Console.WriteLine("******************* 1 ****** " + vOwner + " ************************");

            Console.WriteLine("******************* 2 ****** " + (vOwner == IntPtr.Zero) + " ************************");
            if (vOwner == IntPtr.Zero) return "";            

            int vProcessId;
            GetWindowThreadProcessId(vOwner, out vProcessId);
            Console.WriteLine("******************* 3 ****** " + vProcessId + " ************************");

            Process vProcess = Process.GetProcessById(vProcessId);
            Console.WriteLine("******************* 4 ****** " + vProcess.ProcessName + " ************************");

            return vProcess.ProcessName;
        }

        /// <summary>
        /// 方法转异步
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ac"></param>
        /// <param name="obj"></param>
        public static void ToAsync<T>(Action<T> ac, T obj)
        {
            Task.Run(() => {
                try
                {
                    ac(obj);
                }
                catch (Exception error)
                {
                    Console.WriteLine(error.Message);
                }
            });
        }

        /// <summary>
        /// 方法转异步
        /// </summary>
        /// <param name="ac"></param>
        public static void ToAsync(Action ac)
        {
            Task.Run(() => {
                try
                {
                    ac();
                }
                catch (Exception error)
                {
                    Console.WriteLine(error.Message);
                }
            });
        }

        /// <summary>
        /// 方法转同步
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ac"></param>
        /// <param name="obj"></param>
        public static void ToSync<T>(Action<T> ac, T obj)
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                SynchronizationContext.SetSynchronizationContext(new System.Windows.Threading.DispatcherSynchronizationContext(Application.Current.Dispatcher));
                SynchronizationContext.Current.Send(pl =>
                {
                    try
                    {
                        ac(obj);
                    }
                    catch (Exception error)
                    {
                        Console.WriteLine(error.Message);
                    }

                }, null);
            });
        }

        /// <summary>
        /// 方法转同步
        /// </summary>
        /// <param name="ac"></param>
        public static void ToSync(Action ac)
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                SynchronizationContext.SetSynchronizationContext(new System.Windows.Threading.DispatcherSynchronizationContext(Application.Current.Dispatcher));
                SynchronizationContext.Current.Send(pl =>
                {
                    try
                    {
                        ac();
                    }
                    catch (Exception error)
                    {
                        Console.WriteLine(error.Message);
                    }

                }, null);
            });
        }

        #region 四种深拷贝方法
        public static T DeepCopyByReflect<T>(T obj)
        {
            //如果是字符串或值类型则直接返回
            if (obj is string || obj.GetType().IsValueType) return obj;

            object retval = Activator.CreateInstance(obj.GetType());
            FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            foreach (FieldInfo field in fields)
            {
                try { field.SetValue(retval, DeepCopyByReflect(field.GetValue(obj))); }
                catch { }
            }
            return (T)retval;
        }

        public static T DeepCopyByXml<T>(T obj)
        {
            object retval;
            using (MemoryStream ms = new MemoryStream())
            {
                XmlSerializer xml = new XmlSerializer(typeof(T));
                xml.Serialize(ms, obj);
                ms.Seek(0, SeekOrigin.Begin);
                retval = xml.Deserialize(ms);
                ms.Close();
            }
            return (T)retval;
        }

        public static T DeepCopyByBin<T>(T obj)
        {
            object retval;
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                //序列化成流
                bf.Serialize(ms, obj);
                ms.Seek(0, SeekOrigin.Begin);
                //反序列化成对象
                retval = bf.Deserialize(ms);
                ms.Close();
            }
            return (T)retval;
        }

        //需要silverlight支持
        public static T DeepCopy<T>(T obj)
        {
            object retval;
            using (MemoryStream ms = new MemoryStream())
            {
                DataContractSerializer ser = new DataContractSerializer(typeof(T));
                ser.WriteObject(ms, obj);
                ms.Seek(0, SeekOrigin.Begin);
                retval = ser.ReadObject(ms);
                ms.Close();
            }
            return (T)retval;
        }
        #endregion

        /// <summary>
        /// 递归生成树形数据
        /// </summary>
        /// <param name="delst"></param>
        /// <returns></returns>
        public static ObservableCollection<Zone> getTrees(int parent, ObservableCollection<Zone> nodes)
        {
            ObservableCollection<Zone> mainNodes = new ObservableCollection<Zone>(nodes.Where(x => x.Parent == parent).ToList());
            ObservableCollection<Zone> otherNodes = new ObservableCollection<Zone>(nodes.Where(x => x.Parent != parent).ToList());
            foreach (Zone node in mainNodes)
            {
                node.Nodes = getTrees(node.Id, otherNodes);
            }
            return mainNodes;
        }

        #region 开机自启

        /// <summary>
        /// 快捷方式名称-任意自定义
        /// </summary>
        private const string QuickName = "clipboardplus";

        /// <summary>
        /// 自动获取系统自动启动目录
        /// </summary>
        private static string systemStartPath { get { return Environment.GetFolderPath(Environment.SpecialFolder.Startup); } }

        /// <summary>
        /// 自动获取程序完整路径
        /// </summary>
        public static string appAllPath { get { return Process.GetCurrentProcess().MainModule.FileName; } }

        /// <summary>
        /// 自动获取桌面目录
        /// </summary>
        private static string desktopPath { get { return Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory); } }

        /// <summary>
        /// 设置开机自动启动-只需要调用改方法就可以了参数里面的bool变量是控制开机启动的开关的，默认为开启自启启动
        /// </summary>
        /// <param name="onOff">自启开关</param>
        public static void SetMeAutoStart(bool onOff = true)
        {
            if (onOff)//开机启动
            {
                //获取启动路径应用程序快捷方式的路径集合
                List<string> shortcutPaths = GetQuickFromFolder(systemStartPath, appAllPath);
                //存在2个以快捷方式则保留一个快捷方式-避免重复多于
                if (shortcutPaths.Count >= 2)
                {
                    for (int i = 1; i < shortcutPaths.Count; i++)
                    {
                        DeleteFile(shortcutPaths[i]);
                    }
                }
                else if (shortcutPaths.Count < 1)//不存在则创建快捷方式
                {
                    CreateShortcut(systemStartPath, QuickName, appAllPath, "clipboardplus --Yanareat");
                }
            }
            else//开机不启动
            {
                //获取启动路径应用程序快捷方式的路径集合
                List<string> shortcutPaths = GetQuickFromFolder(systemStartPath, appAllPath);
                //存在快捷方式则遍历全部删除
                if (shortcutPaths.Count > 0)
                {
                    for (int i = 0; i < shortcutPaths.Count; i++)
                    {
                        DeleteFile(shortcutPaths[i]);
                    }
                }
            }
            //创建桌面快捷方式-如果需要可以取消注释
            //CreateDesktopQuick(desktopPath, QuickName, appAllPath);
        }

        /// <summary>
        ///  向目标路径创建指定文件的快捷方式
        /// </summary>
        /// <param name="directory">目标目录</param>
        /// <param name="shortcutName">快捷方式名字</param>
        /// <param name="targetPath">文件完全路径</param>
        /// <param name="description">描述</param>
        /// <param name="iconLocation">图标地址</param>
        /// <returns>成功或失败</returns>
        private static bool CreateShortcut(string directory, string shortcutName, string targetPath, string description = null, string iconLocation = null)
        {
            try
            {
                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);                         //目录不存在则创建
                //添加引用 Com 中搜索 Windows Script Host Object Model
                string shortcutPath = Path.Combine(directory, string.Format("{0}.lnk", shortcutName));          //合成路径
                WshShell shell = new IWshRuntimeLibrary.WshShell();
                IWshShortcut shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(shortcutPath);    //创建快捷方式对象
                shortcut.TargetPath = targetPath;                                                               //指定目标路径
                shortcut.WorkingDirectory = Path.GetDirectoryName(targetPath);                                  //设置起始位置
                shortcut.WindowStyle = 1;                                                                       //设置运行方式，默认为常规窗口
                shortcut.Description = description;                                                             //设置备注
                shortcut.IconLocation = string.IsNullOrWhiteSpace(iconLocation) ? targetPath : iconLocation;    //设置图标路径
                shortcut.Save();                                                                                //保存快捷方式
                return true;
            }
            catch (Exception ex)
            {
                string temp = ex.Message;
                temp = "";
            }
            return false;
        }

        /// <summary>
        /// 获取指定文件夹下指定应用程序的快捷方式路径集合
        /// </summary>
        /// <param name="directory">文件夹</param>
        /// <param name="targetPath">目标应用程序路径</param>
        /// <returns>目标应用程序的快捷方式</returns>
        private static List<string> GetQuickFromFolder(string directory, string targetPath)
        {
            List<string> tempStrs = new List<string>();
            tempStrs.Clear();
            string tempStr = null;
            string[] files = Directory.GetFiles(directory, "*.lnk");
            if (files == null || files.Length < 1)
            {
                return tempStrs;
            }
            for (int i = 0; i < files.Length; i++)
            {
                //files[i] = string.Format("{0}\\{1}", directory, files[i]);
                tempStr = GetAppPathFromQuick(files[i]);
                if (tempStr == targetPath)
                {
                    tempStrs.Add(files[i]);
                }
            }
            return tempStrs;
        }

        /// <summary>
        /// 获取快捷方式的目标文件路径-用于判断是否已经开启了自动启动
        /// </summary>
        /// <param name="shortcutPath"></param>
        /// <returns></returns>
        private static string GetAppPathFromQuick(string shortcutPath)
        {
            //快捷方式文件的路径 = @"d:\Test.lnk";
            if (System.IO.File.Exists(shortcutPath))
            {
                WshShell shell = new WshShell();
                IWshShortcut shortct = (IWshShortcut)shell.CreateShortcut(shortcutPath);
                //快捷方式文件指向的路径.Text = 当前快捷方式文件IWshShortcut类.TargetPath;
                //快捷方式文件指向的目标目录.Text = 当前快捷方式文件IWshShortcut类.WorkingDirectory;
                return shortct.TargetPath;
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// 根据路径删除文件-用于取消自启时从计算机自启目录删除程序的快捷方式
        /// </summary>
        /// <param name="path">路径</param>
        private static void DeleteFile(string path)
        {
            FileAttributes attr = System.IO.File.GetAttributes(path);
            if (attr == FileAttributes.Directory)
            {
                Directory.Delete(path, true);
            }
            else
            {
                System.IO.File.Delete(path);
            }
        }

        /// <summary>
        /// 在桌面上创建快捷方式-如果需要可以调用
        /// </summary>
        /// <param name="desktopPath">桌面地址</param>
        /// <param name="appPath">应用路径</param>
        public static void CreateDesktopQuick(bool onOff = true)
        {
            if (onOff)//开机启动
            {
                //获取启动路径应用程序快捷方式的路径集合
                List<string> shortcutPaths = GetQuickFromFolder(desktopPath, appAllPath);
                //存在2个以快捷方式则保留一个快捷方式-避免重复多于
                if (shortcutPaths.Count >= 2)
                {
                    for (int i = 1; i < shortcutPaths.Count; i++)
                    {
                        DeleteFile(shortcutPaths[i]);
                    }
                }
                else if (shortcutPaths.Count < 1)//不存在则创建快捷方式
                {
                    CreateShortcut(desktopPath, QuickName, appAllPath, "clipboardplus --Yanareat");
                }
            }
            else//开机不启动
            {
                //获取启动路径应用程序快捷方式的路径集合
                List<string> shortcutPaths = GetQuickFromFolder(desktopPath, appAllPath);
                //存在快捷方式则遍历全部删除
                if (shortcutPaths.Count > 0)
                {
                    for (int i = 0; i < shortcutPaths.Count; i++)
                    {
                        DeleteFile(shortcutPaths[i]);
                    }
                }
            }
        }

        #endregion
    }
}
