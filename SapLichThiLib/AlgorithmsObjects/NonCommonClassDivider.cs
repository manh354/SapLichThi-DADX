using SapLichThiLib.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiLib.AlgorithmsObjects
{
    internal class NonCommonClassDivider
    {
        public int StartId { get; set; }
        public float NonCommonDivideThreshold { get; set; }
        public List<StudyClass> AllNonCommonStudyClasses { get; set; }
        public Dictionary<StudyClass, List<ExamClass>> O_studyClasses_examClasses { get; set; }
        public List<ExamClass> O_examClasses { get; set; }  
        public void Run()
        {
            O_studyClasses_examClasses = new();
            O_examClasses = new();
            foreach (var studyClass in AllNonCommonStudyClasses)
            {
                DivideNonCommonClass(studyClass, out List<ExamClass> examClasses);
                O_examClasses.AddRange(examClasses);
            }
        }
        public void DivideNonCommonClass(StudyClass thisStudyClass, out List<ExamClass> examClassesOfThisStudyClass)
        {
            List<ExamClass> thisExamClasses = new List<ExamClass>();
            if (thisStudyClass.Count > NonCommonDivideThreshold)
            {
                int numExamClasses = (int)MathF.Ceiling((float)thisStudyClass.Count / NonCommonDivideThreshold);
                int numStudentPerClass = thisStudyClass.Count / numExamClasses;
                for (int i = 0; i < numExamClasses - 1; i++)
                {
                    thisExamClasses.Add(new ExamClass(thisStudyClass, StartId.ToString(), string.Format("Nhóm {0}", i + 1), numStudentPerClass));
                    StartId++;
                }
                thisExamClasses.Add(new ExamClass(thisStudyClass, StartId.ToString(), string.Format("Nhóm {0}", numExamClasses), thisStudyClass.Count - (numExamClasses - 1) * numStudentPerClass));
                StartId++;
            }
            else
            {
                thisExamClasses.Add(new ExamClass(thisStudyClass, StartId.ToString(), "TC", thisStudyClass.Count));
                StartId++;
            }
            O_studyClasses_examClasses.Add(thisStudyClass, thisExamClasses);
            examClassesOfThisStudyClass = thisExamClasses;
        }
    }
}
