using System;
using System.IO;

namespace ImageConversion.Helpers
{
    public static class Logger
    {
        public static void Log(string message)
        {
            using (StreamWriter writer = File.AppendText("log.txt"))
            {
                DateTime dt = new DateTime();
                dt = DateTime.Now;
                string dateNow = "[" + dt.ToString("yyyy-MM-dd HH:mm:ss.ffff") + "] ";
                writer.WriteLine(dateNow + message);
            }
        }

        public static void SaveError(string message)
        {
            using (StreamWriter writer = File.AppendText("save_error.txt"))
            {
                DateTime dt = new DateTime();
                dt = DateTime.Now;
                string dateNow = "[" + dt.ToString("yyyy-MM-dd HH:mm:ss.ffff") + "] ";
                writer.WriteLine(dateNow + message);
            }
        }
    }
}
