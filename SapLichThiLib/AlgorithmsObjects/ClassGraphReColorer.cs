using SapLichThiLib.DataStructures;
using SapLichThiLib.FileWriter;
using SapLichThiLib.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiLib.AlgorithmsObjects
{
    // Tô màu lại là không cần thiết nữa, giờ chỉ cần 1 biện pháp tô màu mà thôi (ClassGraphColorer)
    internal class ClassGraphReColorer
    {
        public StudentConflictTest StudentConflictTest { get; set; }
        public ClassGraph ClassGraph { get; set; }
        public void RecolorClassGraph()
        {
            foreach (var elem in StudentConflictTest.Student_ConflictedPairOfClasses_Dict)
            {
                foreach(var elem2 in elem.Value)
                {
                    
                }
            }
        }
    }
}
