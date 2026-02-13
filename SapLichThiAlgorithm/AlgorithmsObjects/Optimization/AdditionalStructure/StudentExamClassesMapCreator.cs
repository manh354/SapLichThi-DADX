using SapLichThiCore.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiAlgorithm.AlgorithmsObjects.Optimization.AdditionalStructure
{
    public class StudentExamClassesMapCreator : BaseAlgorithmObject
    {
        public Dictionary<Student, HashSet<ExamClass>> O_student_RelevantExamClasses { get; set; }
        public Dictionary<Student, HashSet<ExamClass>> O_student_AllExamClasses { get; set; }
        public List<ExamClass> I_examClasses { get; set; }
        protected override void InitializeAllOutput()
        {
            O_student_RelevantExamClasses = new();
            O_student_AllExamClasses = new();
        }

        protected override void ProcedureRun()
        {
            foreach (var examClass in I_examClasses)
            {
                foreach (var student in examClass.Students)
                {
                    O_student_RelevantExamClasses.TryAdd(student, new HashSet<ExamClass>());
                    O_student_AllExamClasses.TryAdd(student, new HashSet<ExamClass>());
                    O_student_AllExamClasses[student].Add(examClass);
                    O_student_RelevantExamClasses[student].Add(examClass);

                }
            }
        }

        protected override void ReceiveInput(AlgorithmContext context)
        {
            Context = context;
            I_examClasses = context.I_examClasses;
        }

        protected override void SendOutput(AlgorithmContext context)
        {
            context.I_student_relevantExamClasses = O_student_RelevantExamClasses;
            context.I_student_allExamClasses = O_student_AllExamClasses;
        }
    }
}
