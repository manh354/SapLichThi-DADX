namespace SapLichThiLib.DataObjects
{
    public class StudyClass
    {
        string id;
        public string ID => id;
        string linkedClassID;
        public string LinkedClassID => linkedClassID;
        Course course;
        public Course Course => course;
        public string Description { get; set; }
        List<Student> students;
        public List<Student> Students
        {
            get { return students; }
            set { students = value; }
        }
        int count;
        public int Count
        {
            get
            {
                if (students != null)
                    return students.Count;
                else return count;
            }
        }
        bool isExercise;
        public bool IsExercise => isExercise;
        bool isElitech;
        public bool IsElitech => isElitech;
        // Khoá học hướng đến của môn học
        StudentYear studentYear;
        string term;
        string Term => term;
        public StudentYear StudentYear => studentYear;
        public StudyClass? LinkedClass { get; set; }

        public StudyClass(string id, string linkedClassId , Course course,bool isExercise, bool isElitech, string description, StudentYear studentYear,string term , int count = 0, StudyClass? linkedClass = null ,List<Student> students = null)
        {
            this.id = id;
            this.linkedClassID = linkedClassId;
            this.course = course;
            this.isElitech = isElitech;
            this.isExercise = isExercise;
            Description = description;
            this.studentYear = studentYear;
            this.term = term;
            this.count = count;
            this.students = students;
            this.LinkedClass = linkedClass;
        }
        public override string ToString()
        {
            string result = string.Format("Class id: {0,-10}, isCommon: {1,-6}, haveSchedual: {2,-6}", id, isElitech,true) + "\n";
            result += string.Format("Course: {0,-20}\n", course.ToString());
            result += "List of all the Students: \n";
            /*if(students != null)
            for (int i = 0; i < students.Count; i++)
            {
                result += students[i].ToString() + "\t";
            }*/
            return result;
        }
    }
}
