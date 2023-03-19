using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LurkbotV5
{
    internal class Log
    {
        public static void WriteLineColor(string msg, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(msg);
            Console.ResetColor();
        }
        public static void WriteSuccess(string msg)
        {
            if (!InternalConfig.EnableLogging)
            {
                return;
            }
            StackTrace stackTrace = new StackTrace();
            string clazz = stackTrace.GetFrame(1).GetMethod().DeclaringType.ToString().Split('.').Last();
            Console.ForegroundColor = ConsoleColor.Green;
            string date = DateTime.Now.ToString("dd-MM-yyyy");
            string time = DateTime.Now.ToString("hh\\:mm\\:ss");
            string file = InternalConfig.LogPath + date + ".log";
            if (InternalConfig.ShowLogsInConsole)
            {
                Console.WriteLine("[" + time + $" SUCCESS] [{clazz}]: " + msg);
            }
            StreamWriter sw = new StreamWriter(file, append: true);
            sw.Write("[" + time + $" SUCCESS] [{clazz}]: " + msg + "\n");
            sw.Close();
            Console.ResetColor();
        }
        public static void WriteError(string msg)
        {
            if (!InternalConfig.EnableLogging)
            {
                return;
            }
            StackTrace stackTrace = new StackTrace();
            string clazz = stackTrace.GetFrame(1).GetMethod().DeclaringType.ToString().Split('.').Last();
            Console.ForegroundColor = ConsoleColor.Red;
            string date = DateTime.Now.ToString("dd-MM-yyyy");
            string time = DateTime.Now.ToString("hh\\:mm\\:ss");
            string file = InternalConfig.LogPath + date + ".log";
            if (InternalConfig.ShowLogsInConsole)
            {
                Console.WriteLine($"[" + time + $" ERROR] [{clazz}]: " + msg);
            }
            StreamWriter sw = new StreamWriter(file, append: true);
            sw.Write("[" + time + $" ERROR] [{clazz}]: " + msg + "\n");
            sw.Close();
            Console.ResetColor();
        }
        public static void WriteFatal(string msg)
        {
            if (!InternalConfig.EnableLogging)
            {
                return;
            }
            StackTrace stackTrace = new StackTrace();
            string clazz = stackTrace.GetFrame(1).GetMethod().DeclaringType.ToString().Split('.').Last();
            Console.ForegroundColor = ConsoleColor.Red;
            string date = DateTime.Now.ToString("dd-MM-yyyy");
            string time = DateTime.Now.ToString("hh\\:mm\\:ss");
            string file = InternalConfig.LogPath + date + ".log";
            if (InternalConfig.ShowLogsInConsole)
            {
                Console.WriteLine("[" + time + $" FATAL ERROR] [{clazz}]: " + msg);
            }
            StreamWriter sw = new StreamWriter(file, append: true);
            sw.Write("[" + time + $" FATAL ERROR] [{clazz}]: " + msg + "\n");
            sw.Close();
            Console.ResetColor();
        }
        public static void WriteWarning(string msg)
        {
            if (!InternalConfig.EnableLogging)
            {
                return;
            }
            StackTrace stackTrace = new StackTrace();
            string clazz = stackTrace.GetFrame(1).GetMethod().DeclaringType.ToString().Split('.').Last();
            Console.ForegroundColor = ConsoleColor.Yellow;
            string date = DateTime.Now.ToString("dd-MM-yyyy");
            string time = DateTime.Now.ToString("hh\\:mm\\:ss");
            string file = InternalConfig.LogPath + date + ".log";
            if (InternalConfig.ShowLogsInConsole)
            {
                Console.WriteLine("[" + time + $" WARNING] [{clazz}]: " + msg);
            }
            StreamWriter sw = new StreamWriter(file, append: true);
            sw.Write("[" + time + $" WARNING] [{clazz}]: " + msg + "\n");
            sw.Close();
            Console.ResetColor();
        }
        public static void WriteInfo(string msg)
        {
            if (!InternalConfig.EnableLogging)
            {
                return;
            }
            StackTrace stackTrace = new StackTrace();
            string clazz = stackTrace.GetFrame(1).GetMethod().DeclaringType.ToString().Split('.').Last();
            Console.ForegroundColor = ConsoleColor.White;
            string date = DateTime.Now.ToString("dd-MM-yyyy");
            string time = DateTime.Now.ToString("hh\\:mm\\:ss");
            string file = InternalConfig.LogPath + date + ".log";
            if (InternalConfig.ShowLogsInConsole)
            {
                Console.WriteLine("[" + time + $" INFO] [{clazz}]: " + msg);
            }
            StreamWriter sw = new StreamWriter(file, append: true);
            sw.Write("[" + time + $" INFO] [{clazz}]: " + msg + "\n");
            sw.Close();
            Console.ResetColor();
        }
        public static void WriteDebug(string msg)
        {
            if (!InternalConfig.EnableLogging)
            {
                return;
            }
            StackTrace stackTrace = new StackTrace();
            string clazz = stackTrace.GetFrame(1).GetMethod().DeclaringType.ToString().Split('.').Last();
            Console.ForegroundColor = ConsoleColor.Gray;
            string date = DateTime.Now.ToString("dd-MM-yyyy");
            string time = DateTime.Now.ToString("hh\\:mm\\:ss");
            string file = InternalConfig.LogPath + date + ".log";
            if (InternalConfig.ShowLogsInConsole)
            {
                Console.WriteLine("[" + time + $" DEBUG] [{clazz}]: " + msg);
            }
            StreamWriter sw = new StreamWriter(file, append: true);
            sw.Write("[" + time + $" DEBUG] [{clazz}]: " + msg + "\n");
            sw.Close();
            Console.ResetColor();
        }
        public static void WriteVerbose(string msg)
        {
            if (!InternalConfig.EnableLogging)
            {
                return;
            }
            StackTrace stackTrace = new StackTrace();
            string clazz = stackTrace.GetFrame(1).GetMethod().DeclaringType.ToString().Split('.').Last();
            Console.ForegroundColor = ConsoleColor.Gray;
            string date = DateTime.Now.ToString("dd-MM-yyyy");
            string time = DateTime.Now.ToString("hh\\:mm\\:ss");
            string file = InternalConfig.LogPath + date + ".log";
            if (InternalConfig.ShowLogsInConsole)
            {
                Console.WriteLine("[" + time + $" VERBOSE] [{clazz}]: " + msg);
            }
            StreamWriter sw = new StreamWriter(file, append: true);
            sw.Write("[" + time + $" VERBOSE] [{clazz}]: " + msg + "\n");
            sw.Close();
            Console.ResetColor();
        }
        public static void WriteCritical(string msg)
        {
            if (!InternalConfig.EnableLogging)
            {
                return;
            }
            StackTrace stackTrace = new StackTrace();
            string clazz = stackTrace.GetFrame(1).GetMethod().DeclaringType.ToString().Split('.').Last();
            Console.ForegroundColor = ConsoleColor.DarkRed;
            string date = DateTime.Now.ToString("dd-MM-yyyy");
            string time = DateTime.Now.ToString("hh\\:mm\\:ss");
            string file = InternalConfig.LogPath + date + ".log";
            if (InternalConfig.ShowLogsInConsole)
            {
                Console.WriteLine("[" + time + $" CRITICAL] [{clazz}]: " + msg);
            }
            StreamWriter sw = new StreamWriter(file, append: true);
            sw.Write("[" + time + $" CRITICAL] [{clazz}]: " + msg + "\n");
            sw.Close();
            Console.ResetColor();
        }
    }
}
