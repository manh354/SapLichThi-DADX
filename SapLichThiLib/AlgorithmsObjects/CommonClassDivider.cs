using SapLichThiLib.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiLib.AlgorithmsObjects
{
    public class CommonClassDivider : IAlgorithmObject
    {
        // INPUT
        public List<StudyClass>? I_allStudyClasses { get; set; }
        public List<ExamGroup>? I_allHardRails { get; set; } 
        public Dictionary<Course, HashSet<StudyClass>> I_courseGroup { get; set; }
        public int I_commonDivideThreshold { get; set; }
        // OUTPUT
        public Dictionary<StudyClass, List<ExamClass>>? O_studyClass_examClasses { get; set; }
        public List<Course> O_commonCourse { get; set; }
        public List<ExamClass> O_examClasses { get; set; }
        public int O_id { get; set; }

        public void Run()
        {
            CheckAllInput();
            InitializeAllOutput();
            
            ProcedureRun();
        }

        public void CheckAllInput()
        {
            if(I_allHardRails == null || I_allStudyClasses == null || I_courseGroup == null || I_commonDivideThreshold == 0)
            {
                throw new Exception(GetType().ToString() + " Not properly initialized");
            }
        }

        public void InitializeAllOutput() // following the real data_input
        {
            O_studyClass_examClasses = new();
            O_commonCourse = new();
            O_examClasses = new();
            O_id = 142363;
        }


        public void ProcedureRun()
        {
            
            List<Course> hardRailCourses = new List<Course>();
            I_allHardRails.ForEach(x => x.Courses.ForEach(y => hardRailCourses.Add(y)));
            List<StudyClass> allHardRailStudyClass = new();
            hardRailCourses.ForEach(x => 
            {
                allHardRailStudyClass = allHardRailStudyClass.Concat(I_courseGroup[x]).ToList();
                O_commonCourse.Add(x);
            }
            );
            allHardRailStudyClass.ForEach(x => DivideHardRailingClass(x));
        }
        public void DivideHardRailingClass(StudyClass thisStudyClass)
        {
            List<ExamClass> thisExamClasses = new List<ExamClass>();
            if (thisStudyClass.Count > I_commonDivideThreshold)
            {
                int numExamClasses = (int)MathF.Ceiling((float)thisStudyClass.Count / I_commonDivideThreshold);
                int numStudentPerClass = thisStudyClass.Count / numExamClasses;
                for (int i = 0; i < numExamClasses - 1; i++)
                {
                    thisExamClasses.Add(new ExamClass(thisStudyClass, O_id.ToString(), string.Format("Nhóm {0}", i + 1), numStudentPerClass));
                    O_id++;
                }
                thisExamClasses.Add(new ExamClass(thisStudyClass, O_id.ToString(), string.Format("Nhóm {0}", numExamClasses), thisStudyClass.Count - (numExamClasses - 1) * numStudentPerClass));
                O_id++;
            }
            else
            {
                thisExamClasses.Add(new ExamClass(thisStudyClass, O_id.ToString(), "TC", thisStudyClass.Count));
                O_id++;
            }
            O_studyClass_examClasses.Add(thisStudyClass, thisExamClasses);
            O_examClasses.AddRange(thisExamClasses);
        }

        /*private void DivideCommonClass(StudyClass thisStudyClass)
        {
            List<ExamClass> thisExamClasses = new List<ExamClass>();
            if (thisStudyClass.Count > I_commonDivideThreshold)
            {
                int numExamClasses = (int)MathF.Ceiling((float)thisStudyClass.Count / I_commonDivideThreshold);
                int numStudentPerClass = thisStudyClass.Count / numExamClasses;
                for (int i = 0; i < numExamClasses - 1; i++)
                {
                    thisExamClasses.Add(new ExamClass(thisStudyClass, Id.ToString(), string.Format("Nhóm {0}", i + 1), numStudentPerClass));
                    Id++;
                }
                thisExamClasses.Add(new ExamClass(thisStudyClass, Id.ToString(), string.Format("Nhóm {0}", numExamClasses), thisStudyClass.Count - (numExamClasses - 1) * numStudentPerClass));
                Id++;
            }
            else
            {
                thisExamClasses.Add(new ExamClass(thisStudyClass, Id.ToString(), "TC", thisStudyClass.Count));
                Id++;
            }
            O_studyClass_examClasses.Add(thisStudyClass, thisExamClasses);
        }
        private void DivideNonCommonClass(StudyClass thisStudyClass)
        {
            List<ExamClass> thisExamClasses = new List<ExamClass>();
            if (thisStudyClass.Count > I_commonDivideThreshold)
            {
                int numExamClasses = (int)MathF.Ceiling((float)thisStudyClass.Count / I_nonCommonDivideThreshold);
                int numStudentPerClass = thisStudyClass.Count / numExamClasses;
                for (int i = 0; i < numExamClasses - 1; i++)
                {
                    thisExamClasses.Add(new ExamClass(thisStudyClass, Id.ToString(), string.Format("Nhóm {0}", i + 1), numStudentPerClass));
                    Id++;
                }
                thisExamClasses.Add(new ExamClass(thisStudyClass, Id.ToString(), string.Format("Nhóm {0}", numExamClasses), thisStudyClass.Count - (numExamClasses - 1) * numStudentPerClass));
                Id++;
            }
            else
            {
                thisExamClasses.Add(new ExamClass(thisStudyClass, Id.ToString(), "TC", thisStudyClass.Count));
                Id++;
            }
            O_studyClass_examClasses.Add(thisStudyClass, thisExamClasses);
        }*/

    }
}
