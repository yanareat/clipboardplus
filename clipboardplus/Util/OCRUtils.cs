using Baidu.Aip.Ocr;
using System;
using System.Collections.Generic;
using System.Text;
using static System.Diagnostics.Debug;

namespace clipboard_
{
    class OCRUtils
    {
        // 设置APPID/AK/SK
        static string APP_ID = "19005685";
        static string API_KEY = "m0pmGjobQTt1QMTCjNtMYy9M";
        static string SECRET_KEY = "R7UlLQb4WFKByxReQFjlN2rQIlOZCA6p";

        static Ocr client = new Ocr(API_KEY, SECRET_KEY);
        
        public static StringBuilder Img2Tex(byte[] img)
        {
            client.Timeout = 60000;  // 修改超时时间
            StringBuilder sb = new StringBuilder();
            try
            {
                var image = img;
                // 调用通用文字识别, 图片参数为本地图片，可能会抛出网络等异常，请使用try/catch捕获
                var result = client.GeneralBasic(image);
                var words = result.GetValue("words_result");
                foreach (var word in words.Children())
                {
                    sb.AppendLine(word["words"].ToString());
                }
            }
            catch(Exception e)
            {
                WriteLine(e.ToString());
            }
            return sb;
        }
    }
}
