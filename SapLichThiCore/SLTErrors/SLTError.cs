using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiCore.SLTErrors
{
    public record SLTError
    {
        public SLTError(string message, SLTErrorLevel level)
        {
            Message = message;
            Level = level;  
        }
        public string Message { get; private set; }
        public SLTErrorLevel Level { get; private set; }
    }
    public enum SLTErrorLevel
    {
        Info,
        Warning,
        Error
    }
}
