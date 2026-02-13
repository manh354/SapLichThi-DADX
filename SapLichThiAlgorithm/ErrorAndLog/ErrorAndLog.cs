using System.Globalization;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace SapLichThiAlgorithm.ErrorAndLog
{
    public enum LogType
    {
        Info,
        Warning,
        Error,
        Debug
    }

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

    public class FormatError : ErrorAndLog
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
        private static readonly object _lock = new object();
        private static readonly object _lock_data = new object();
        private static Logger logger = new();
        public static string Folder = "Outputs";
        public static string FilePath = "LOG.txt";
        public static string DataFilePath = "DATA.txt";
        public static string _combineFilePath = Path.Combine(Folder, FilePath);
        public static string _combineDataFilePath = Path.Combine(Folder, DataFilePath);

        private readonly Channel<LogItem> _logChannel;
        private abstract record LogItem;
        private record MessageLogItem(string LogEntry, LogType LogType) : LogItem;
        private record DataLogItem(string Output) : LogItem;

        Logger()
        {
            Folder = "Outputs";
            FilePath = "LOG.txt";
            DataFilePath = "DATA.txt";
            _combineFilePath = Path.Combine(Folder, FilePath);
            _combineDataFilePath = Path.Combine(Folder, DataFilePath);
            if (File.Exists(_combineFilePath))
                File.Delete(_combineFilePath);
            if (File.Exists(_combineDataFilePath))
                File.Delete(_combineDataFilePath);
            Directory.CreateDirectory(Folder);
            File.Create(FilePath).Dispose();
            File.Create(DataFilePath).Dispose();
            Console.OutputEncoding = Encoding.UTF8;

            _logChannel = Channel.CreateUnbounded<LogItem>();
            _ = Task.Run(ProcessLogQueue);
        }

        private async Task ProcessLogQueue()
        {
            await foreach (var item in _logChannel.Reader.ReadAllAsync())
            {
                try
                {
                    if (item is MessageLogItem msg)
                    {
                        lock (_lock)
                        {
                            File.AppendAllText(_combineFilePath, msg.LogEntry + Environment.NewLine);
                        }
                        PrintToConsole(msg.LogEntry, msg.LogType);
                    }
                    else if (item is DataLogItem data)
                    {
                        lock (_lock_data)
                        {
                            File.AppendAllText(_combineDataFilePath, data.Output + Environment.NewLine);
                        }
                    }
                }
                catch
                {
                    // Fail silently to avoid crashing the background loop
                }
            }
        }

        private void _LogMessage(string message, LogType logType)
        {
            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{logType}] {message}";
            _logChannel.Writer.TryWrite(new MessageLogItem(logEntry, logType));
        }

        public void _LogData(double[] val, string[] name)
        {
            string dataEntry = string.Join(", ", val.Zip(name, (v, n) => $"{n}: {v.ToString(CultureInfo.InvariantCulture)}"));
            string output = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {dataEntry}";
            _logChannel.Writer.TryWrite(new DataLogItem(output));
        }

        private void PrintToConsole(string message, LogType logType)
        {
            ConsoleColor originalColor = Console.ForegroundColor;

            switch (logType)
            {
                case LogType.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogType.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogType.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogType.Debug:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;
            }

            Console.WriteLine(message);
            Console.ForegroundColor = originalColor;
        }

        /// <summary>
        /// Set the data file path used by the logger. Accepts a full or relative path.
        /// If the target file or directory does not exist it will be created.
        /// The actual file name will be prefixed with a timestamp (yyyyMMdd_HHmmss) followed by an underscore and the original name.
        /// </summary>
        public static void SetDataFilePath(string fullPath)
        {
            if (string.IsNullOrWhiteSpace(fullPath))
                throw new ArgumentException("Path must not be null or empty.", nameof(fullPath));

            lock (_lock_data)
            {
                // Determine directory and original file name
                var dir = Path.GetDirectoryName(fullPath);
                var originalName = Path.GetFileName(fullPath);

                // Create timestamp prefix
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var newFileName = string.IsNullOrEmpty(originalName) ? $"{timestamp}" : $"{timestamp}_{originalName}";

                // If no directory supplied, use current directory (or Folder)
                string targetDir = string.IsNullOrEmpty(dir) ? Folder : dir;

                // Ensure directory exists
                Directory.CreateDirectory(targetDir);

                // Build combined path and update static fields
                _combineDataFilePath = Path.Combine(targetDir, newFileName);
                DataFilePath = newFileName;

                if (!File.Exists(_combineDataFilePath))
                    File.Create(_combineDataFilePath).Dispose();
            }
        }

        /// <summary>
        /// Immediately save (copy) the current data file to the provided target path.
        /// If the target is a directory, the file will be written there. If the target is a file path,
        /// its directory will be used. The saved file name will be prefixed with a timestamp (yyyyMMdd_HHmmss)
        /// followed by an underscore and the current data file name.
        /// The target directory will be created if missing. Existing files will be overwritten.
        /// </summary>
        public static void SaveDataFile(string targetPath)
        {
            if (string.IsNullOrWhiteSpace(targetPath))
                throw new ArgumentException("Target path must not be null or empty.", nameof(targetPath));

            lock (_lock_data)
            {
                if (!File.Exists(_combineDataFilePath))
                    throw new FileNotFoundException("Current data file not found.", _combineDataFilePath);

                // Determine if targetPath is a directory (existing directory, ends with separator, or has no extension)
                bool treatAsDirectory = Directory.Exists(targetPath)
                    || targetPath.EndsWith(Path.DirectorySeparatorChar)
                    || targetPath.EndsWith(Path.AltDirectorySeparatorChar)
                    || string.IsNullOrEmpty(Path.GetExtension(targetPath));

                string targetDir;
                if (treatAsDirectory)
                {
                    targetDir = targetPath;
                }
                else
                {
                    targetDir = Path.GetDirectoryName(targetPath);
                }

                if (string.IsNullOrEmpty(targetDir))
                    targetDir = Folder;

                Directory.CreateDirectory(targetDir);

                // Decide base name: prefer DataFilePath if available, otherwise use file name from current path
                var baseName = !string.IsNullOrEmpty(DataFilePath) ? DataFilePath : Path.GetFileName(_combineDataFilePath);
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var newFileName = $"{timestamp}_{baseName}";
                var finalPath = Path.Combine(targetDir, newFileName);

                File.Copy(_combineDataFilePath, finalPath, true);

                // Clear the original data file so it can start fresh after the export copy.
                File.WriteAllText(_combineDataFilePath, string.Empty);
            }
        }

        public static void LogMessage(string message, LogType logType = LogType.Info)
        {
            logger._LogMessage(message, logType);
        }

        public static void LogMessage(string message, LogType logType, params object[] args)
        {
            logger._LogMessage(string.Format(message, args), logType);
        }

        public static void LogData(double[] val, string[] name)
        {
            logger._LogData(val, name);
        }

    }
}
