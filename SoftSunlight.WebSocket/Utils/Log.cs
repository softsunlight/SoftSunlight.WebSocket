using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SoftSunlight.WebSocket.Utils
{
    /// <summary>
    /// 日志类
    /// </summary>
    public class Log
    {
        /// <summary>
        /// 日志目录
        /// </summary>
        private static string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log\\");

        /// <summary>
        /// 日志文件名称
        /// </summary>
        private static string fileName = DateTime.Today.ToString("yyyy-MM-dd") + "_log.txt";

        private static Queue<LogContent> contentList = null;

        static Log()
        {
            contentList = new Queue<LogContent>();
            Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        if (contentList.Count > 0)
                        {
                            var content = contentList.Peek();
                            WriteToFile(content);
                            contentList.Dequeue();
                        }
                        else
                        {
                            Thread.Sleep(5000);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            });
        }

        public static void Write(string message)
        {
            Write(message, null);
        }

        public static void Write(string message, Exception ex)
        {
            contentList.Enqueue(new LogContent() { Message = message, Ex = ex });
        }

        private static void WriteToFile(LogContent logContent)
        {
            if (logContent == null)
            {
                return;
            }
            string message = logContent.Message;
            Exception ex = logContent.Ex;
            try
            {
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                File.AppendAllText(Path.Combine(dir, fileName), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + Environment.NewLine + GetContent(message, ex));
            }
            catch (Exception exception)
            {
                string[] files = Directory.GetFiles(dir);
                Regex regex = new Regex(DateTime.Today.ToString("yyyy-MM-dd") + @"_log_?(?<number>_\d+)?\.txt");
                int maxIndex = 1;
                foreach (string file in files)
                {
                    Match m = regex.Match(file);
                    if (m.Success)
                    {
                        if (m.Groups["number"].Success)
                        {
                            maxIndex = Convert.ToInt32(m.Groups["number"].Value);
                        }
                    }
                }
                fileName = DateTime.Today.ToString("yyyy-MM-dd") + "_log_" + maxIndex + ".txt";
                Write(message, exception);
            }
        }

        private static StringBuilder GetContent(string message, Exception ex)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(message).Append(Environment.NewLine);
            if (ex != null)
            {
                stringBuilder.Append(ex.Message + "," + ex.Source + "," + ex.StackTrace).Append(Environment.NewLine);
                if (ex.InnerException != null)
                {
                    stringBuilder.Append(GetContent(string.Empty, ex.InnerException));
                }
            }
            return stringBuilder;
        }

        private class LogContent
        {
            public string Message { get; set; }
            public Exception Ex { get; set; }
        }

    }
}
