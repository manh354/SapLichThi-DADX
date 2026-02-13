using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiLib.AlgorithmsObjects
{
    public interface IAlgorithmObject
    {
        public void Run();
        protected void CheckAllInput();
        protected void InitializeAllOutput();
        protected void ProcedureRun();
    }
}
