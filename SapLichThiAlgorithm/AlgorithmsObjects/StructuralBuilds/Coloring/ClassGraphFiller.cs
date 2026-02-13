using SapLichThiCore.DataObjects;
using SapLichThiCore.DataStructures;

namespace SapLichThiAlgorithm.AlgorithmsObjects.StructuralBuilds.Coloring
{
    public class ClassGraphFiller : BaseAlgorithmObject
    {
        public List<ExamClass> I_examClasses { get; set; }
        public Dictionary<ExamClass, HashSet<ExamClass>> I_examClassLinkages { get; set; }
        public ClassGraph O_classGraph { get; set; }

        protected override void ReceiveInput(AlgorithmContext context)
        {
            I_examClassLinkages = context.I_examClass_linkages;
            I_examClasses = context.I_examClasses;
        }

        protected override void SendOutput(AlgorithmContext context)
        {
            context.I_classGraph = O_classGraph;
        }

        protected override void InitializeAllOutput()
        {
            O_classGraph = new(I_examClasses);
        }

        protected override void ProcedureRun()
        {
            foreach (var leftNode in O_classGraph.AdjacencyList)
            {
                foreach (var rightNode in O_classGraph.AdjacencyList)
                {
                    if (leftNode.Key != rightNode.Key)
                    {
                        if (CheckTwoClassesHaveAnyLinkage(leftNode.Key, rightNode.Key))
                        {
                            O_classGraph.AddEdge(new Tuple<ExamClass, ExamClass>(leftNode.Key, rightNode.Key));
                        }
                    }
                }
            }
        }

        // Kiểm tra 2 lớp có liên kết hay không tuỳ theo dữ liệu của lớp có sinh viên hay không có sinh viên
        public bool CheckTwoClassesHaveAnyLinkage(ExamClass firstClass, ExamClass secondClass)
        {
            if (true)
            {
                return CheckTwoClassHaveInternalLinkage(firstClass, secondClass);
            }
            else return CheckTwoClassesHaveAnyCommonStudents(firstClass, secondClass);
        }



        /// <summary>
        /// Hàm kiểm tra 2 lớp có chung sinh viên hay không.
        /// </summary>
        /// <param name="firstClass"></param>
        /// <param name="secondClass"></param>
        /// <returns></returns>
        private bool CheckTwoClassesHaveAnyCommonStudents(ExamClass firstClass, ExamClass secondClass)
        {
            int numberOfStudentsFirstClass = firstClass.Students.Count;
            int numberOfStudentsSecondClass = secondClass.Students.Count;
            for (int i = 0; i < numberOfStudentsFirstClass; i++)
            {
                for (int j = 0; j < numberOfStudentsSecondClass; j++)
                {
                    if (firstClass.Students[i] == secondClass.Students[j])
                        return true;
                }
            }
            return false;
        }


        private bool CheckTwoClassHaveInternalLinkage(ExamClass firstClass, ExamClass secondClass)
        {
            if (I_examClassLinkages.ContainsKey(firstClass))
            {
                if (I_examClassLinkages[firstClass].Contains(secondClass))
                    return true;
                return false;
            }
            return false;
        }

    }
}
