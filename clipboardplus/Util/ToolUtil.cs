using HandyControl.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
                if (ImageData.Length != 0)
                {
                    bi = new BitmapImage();
                    bi.BeginInit();
                    bi.StreamSource = new MemoryStream(ImageData);
                    bi.EndInit();
                }
            return bi;
        }

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
        public static extern int GetWindowThreadProcessId(IntPtr handle,
        out int processId);
        [DllImport("kernel32.dll")]
        public static extern bool CloseHandle(IntPtr handle);
        #endregion

        public static string ClipFrom()
        {
            IntPtr vOwner = GetOpenClipboardWindow();
            if (vOwner == IntPtr.Zero) return "";
            int vProcessId;
            GetWindowThreadProcessId(vOwner, out vProcessId);
            Process vProcess = Process.GetProcessById(vProcessId);
            return vProcess.MainModule.FileName;
        }
    }
}
