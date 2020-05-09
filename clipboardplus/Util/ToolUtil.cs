using clipboardplus.Model;
using HandyControl.Controls;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;

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
    }
}
