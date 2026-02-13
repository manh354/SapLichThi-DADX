using SapLichThiLib.DataStructures;
using SapLichThiLib.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiLib.AlgorithmsObjects
{
    public class ClassGraphFiller
    {
        public List<StudyClass> I_classList { get; set; }
        public Dictionary<StudyClass, string> O_studyClass_processedDescription { get; set; }
        public Dictionary<Course, HashSet<Course>> I_courseLinkages { get; set; }
        public ClassGraph O_classGraph { get; set; }
        
        public ClassGraphFiller (List<StudyClass> classList, Dictionary<StudyClass, string> studyClass_ProcessedDescription_Dict)
        {
            I_classList = classList;
            O_studyClass_processedDescription = studyClass_ProcessedDescription_Dict;
            O_classGraph = new ClassGraph(classList);
        }
        public void FillClassGraph()
        {
            foreach (var leftNode in O_classGraph.AdjacencyList)
            {
                foreach (var rightNode in O_classGraph.AdjacencyList)
                {
                    if (leftNode.Key != rightNode.Key)
                    {
                        if (CheckTwoClassesHaveAnyLinkage(leftNode.Key, rightNode.Key))
                        {
                            O_classGraph.AddEdge(new Tuple<StudyClass, StudyClass>(leftNode.Key, rightNode.Key));
                        }
                    }
                }
            }
        }

        // Kiểm tra 2 lớp có liên kết hay không tuỳ theo dữ liệu của lớp có sinh viên hay không có sinh viên
        public bool CheckTwoClassesHaveAnyLinkage(StudyClass firstClass, StudyClass secondClass)
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
        private bool CheckTwoClassesHaveAnyCommonStudents(StudyClass firstClass, StudyClass secondClass)
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

        /// <summary>
        /// Hàm dùng một vài mẹo đặc biệt để kiểm tra 2 lớp có nên trùng nhau hay không
        /// </summary>
        /// <param name="firstClass"></param>
        /// <param name="secondClass"></param>
        /// <returns></returns>
        /*private bool CheckTwoClassHaveInternalLinkage(StudyClass firstClass, StudyClass secondClass)
        {
            // Nếu 2 lớp cùng mã => sinh viên không thể học 2 lớp cùng mã, nên chúng ko có liên kết
            dif (firstClass.Course == secondClass.Course)
                return false;
            // Nếu 2 lớp ko cùng mã môn nhưng cùng tên, VD: "MI1121 và MI1126 có cùng tên Giải tích 2" => Sinh viên cũng không thể học 2 lớp này, nên chúng ko có liên kết
            if (firstClass.Course.Name == secondClass.Course.Name)
            {
                return false;
            }
            // Mếu 2 lớp khác mã, khác tên mã, nếu chúng chung trường, nếu chúng có cùng khoá học thì chúng có liên kết, nếu không thì chúng không có liên kết.
            if (firstClass.Course.School == secondClass.Course.School)
            {
                *//*if (firstClass.StudentYear.Name != secondClass.StudentYear.Name)
                    return false;*//*
                return true;
            }
            // Nếu 2 lớp khác mã, khác tên mã, khác trường nhưng cùng khoá học, cùng là môn chung thì chúng có liên kết
            *//*if(firstClass.Course.IsCommon && secondClass.Course.IsCommon)
            {
                if (firstClass.StudentYear.Name == secondClass.StudentYear.Name)
                    return true;
                return false;
            }*//*
            // Nếu 2 lớp có Mô tả gần khớp nhau
            string firstClassDesc = StudyClass_ProcessedDescription_Dict[firstClass];
            string secondClassDesc = StudyClass_ProcessedDescription_Dict[secondClass];
            if(firstClassDesc.Contains(secondClassDesc)||secondClassDesc.Contains(firstClassDesc))
            {
                return true;

            }    
            return false;
        }*/

        /// <summary>
        /// Hàm dùng một vài mẹo đặc biệt để kiểm tra 2 lớp có nên trùng nhau hay không
        /// </summary>
        /// <param name="firstClass"></param>
        /// <param name="secondClass"></param>
        /// <returns></returns>
        private bool CheckTwoClassHaveInternalLinkage(StudyClass firstClass, StudyClass secondClass)
        {
            if (firstClass.Course == secondClass.Course)
                return false;
            // Nếu 2 lớp ko cùng mã môn nhưng cùng tên, VD: "MI1121 và MI1126 có cùng tên Giải tích 2" => Sinh viên cũng không thể học 2 lớp này, nên chúng ko có liên kết
            if (firstClass.Course.Name.Contains(secondClass.Course.Name) && secondClass.Course.Name.Contains(firstClass.Course.Name))
            {
                return false;
            }
            Course course1 = firstClass.Course;
            Course course2 = secondClass.Course;
            if (I_courseLinkages.ContainsKey(course1))
            {
                if (I_courseLinkages[course1].Contains(course2))
                    return true;
                return false;
            }
            return false;
            // Mếu 2 lớp khác mã, khác tên mã, nếu chúng chung trường, nếu chúng có cùng khoá học thì chúng có liên kết, nếu không thì chúng không có liên kết.
            if (firstClass.Course.School == secondClass.Course.School)
            {
                /*if (firstClass.StudentYear.Name != secondClass.StudentYear.Name)
                    return false;*/
                return true;
            }
            // Nếu 2 lớp khác mã, khác tên mã, khác trường nhưng cùng khoá học, cùng là môn chung thì chúng có liên kết
            /*if(firstClass.Course.IsCommon && secondClass.Course.IsCommon)
            {
                if (firstClass.StudentYear.Name == secondClass.StudentYear.Name)
                    return true;
                return false;
            }*/
            // Nếu 2 lớp có Mô tả gần khớp nhau
            /*string firstClassDesc = StudyClass_ProcessedDescription_Dict[firstClass];
            string secondClassDesc = StudyClass_ProcessedDescription_Dict[secondClass];
            if (firstClassDesc.Contains(secondClassDesc) || secondClassDesc.Contains(firstClassDesc))
            {
                return true;

            }*/
            return false;
        }
    }
}
