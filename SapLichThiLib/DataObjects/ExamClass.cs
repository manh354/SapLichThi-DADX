using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiLib.DataObjects
{
    public class ExamClass
    {
        StudyClass studyClass;
        public StudyClass StudyClass => studyClass;
        string id;
        public string ID => id;
        string description;
        public string Description => description;
        int count;
        public int Count => count;
        public ExamClass(StudyClass studyClass, string id,string description , int count)
        {
            this.studyClass = studyClass;
            this.id = id;
            this.description = description;
            this.count = count;
        }
        public override string ToString()
        {
            return $"{ID}, {StudyClass.ID}, {StudyClass.Course.ID}";
        }
    }
}
