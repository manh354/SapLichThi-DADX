using SapLichThiCore.DataObjects;
using System.ComponentModel.DataAnnotations;

namespace SapLichThiAlgorithm.AlgorithmsObjects
{
    public class ClassesGrouper : BaseAlgorithmObject
    {
        public List<ExamClass> I_allExamClasses { get; set; }
        public List<ExamClass> O_examClasses { get; set; }

        protected override void ProcedureRun()
        {
            O_examClasses = I_allExamClasses.ToList();
        }

        protected override void InitializeAllOutput()
        {
            O_examClasses = new();
        }

        protected override void ReceiveInput(AlgorithmContext context)
        {
            I_allExamClasses = context.I_allExamClasses;

        }

        protected override void SendOutput(AlgorithmContext context)
        {
            context.I_examClasses = O_examClasses;
        }
    }
}
