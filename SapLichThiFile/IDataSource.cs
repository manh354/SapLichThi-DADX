using SapLichThiCore.DataObjects;
using SapLichThiCore.DataType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiStream
{
    public interface IDataSource
    {
        public List<ExamClass> GetAllExamClasses();
        public List<Room> GetRooms();
        public List<Room> GetSpareRooms();
        public List<Student> GetStudents();
        public InputDataType GetInputDataType();
    }
}
