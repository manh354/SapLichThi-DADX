using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SapLichThiLib.ErrorAndLog
{
    public abstract class ErrorAndLog
    {
        protected int id;
        protected string message;
        public ErrorAndLog()
        {
            ThrowMessage();
        }
        protected abstract void ThrowMessage();
    }

    public class FormatError: ErrorAndLog
    {
        public FormatError()
        {
            id = 1;
            message = "ERROR 01: format is wrong";
            ThrowMessage();
        }
        protected override void ThrowMessage()
        {
            Console.WriteLine(message);
        }
    }

    public class StringToBoolError : ErrorAndLog
    {
        public StringToBoolError()
        {
            id = 2;
            message = "ERROR 02: Common/not common is wrong";
            ThrowMessage();
        }
        protected override void ThrowMessage()
        {
            Console.WriteLine(message);
        }
    }

    public class Logger
    {
        public static Logger logger = new();
        public string Path = "DEBUG_LOG/LOG.txt";
        public StreamWriter sw;
        Logger()
        {
            Directory.CreateDirectory("DEBUG_LOG");
            if(File.Exists(Path))
                File.Delete(Path);
            sw = new StreamWriter(File.Open(Path, FileMode.OpenOrCreate),Encoding.Unicode);
            sw.WriteLine("Khởi tạo logger");
        }
        ~Logger()
        {
            sw.Close();
        }

        public void LogDateTime()
        {
            sw.Write(DateTime.Now);
        }
        public void LogMessage(string message)
        {
            sw.WriteLine(message);
        }
        public void LogMessage(string message, params object[] args)
        {
            sw.WriteLine(message, args);
        }
    }
}
