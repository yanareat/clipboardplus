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
using System.Threading.Tasks;
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
            BitmapImage bi = null;
            if (ImageData != null)
            {
                bi = new BitmapImage();
                bi.BeginInit();
                bi.StreamSource = new MemoryStream(ImageData);
                bi.EndInit();
            }
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
        #endregion

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
            if(vProcess != null)
            {
                Console.WriteLine(vProcess.MainWindowTitle + "-----" + vProcess.ProcessName);
            }

            return vProcess.ProcessName;
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
