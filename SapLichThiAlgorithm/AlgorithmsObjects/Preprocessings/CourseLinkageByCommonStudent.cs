using SapLichThiCore.DataObjects;

namespace SapLichThiAlgorithm.AlgorithmsObjects
{
    public class CourseLinkageByCommonStudent : BaseAlgorithmObject
    {
        public List<ExamClass> I_examClasses { get; set; }
        public Dictionary<ExamClass, HashSet<ExamClass>> O_examClass_linkages { get; set; }
        public Dictionary<ExamClass, Dictionary<ExamClass, int>> O_examClass_linkages_count { get; set; }
        protected override void InitializeAllOutput()
        {
            O_examClass_linkages = new();
            O_examClass_linkages_count = new();
        }

        protected override void ProcedureRun()
        {
            // I dont know whether this is the correct way to call
            // this function.
            LinkCourses();
        }

        protected override void ReceiveInput(AlgorithmContext context)
        {
            I_examClasses = context.I_examClasses;
        }

        protected override void SendOutput(AlgorithmContext context)
        {
            context.I_examClass_linkages = O_examClass_linkages;
            context.I_examClass_linkages_count = O_examClass_linkages_count;
        }

        private void LinkCourses()
        {
            foreach (var examClass1 in I_examClasses)
            {
                foreach (var examClass2 in I_examClasses)
                {
                    bool linkage = false;
                    if (examClass1 == examClass2)
                        continue;
                    int count = 0;
                    foreach (var student in examClass1.Students)
                    {
                        if (examClass2.Students.Contains(student))
                        {
                            linkage = true;
                            count++;
                        }
                    }
                    if (linkage)
                    {
                        O_examClass_linkages.TryAdd(examClass1, new());
                        O_examClass_linkages_count.TryAdd(examClass1, new());
                        O_examClass_linkages[examClass1].Add(examClass2);
                        O_examClass_linkages_count[examClass1][examClass2] = count;
                    }
                }
            }
        }

    }
}
