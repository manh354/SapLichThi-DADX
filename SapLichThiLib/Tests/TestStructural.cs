using SapLichThiLib.DataStructures;
using SapLichThiLib.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiLib.Tests
{
    public interface ITest<T>
    {
        public void Test();
        public IEnumerable<T> GiveTestResult();
    }
}
