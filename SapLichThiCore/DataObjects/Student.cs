namespace SapLichThiCore.DataObjects
{
    public class Student
    {
        string id;
        public string ID => id;
        string name;
        int studyGroupId;
        public int StudyGroupId => studyGroupId;
        public Student(string id)
        {
            this.id = id;
            this.name = string.Empty;
            this.studyGroupId = -1;
        }
        public Student(string id, string name) : this(id)
        {
            this.name = name;
        }

        public Student(string id, string name, int studyGroupId) : this(id, name)
        {
            this.studyGroupId = studyGroupId;
        }
        public override string ToString()
        {
            return string.Format("Student: id:{0,-10}", id);
        }
    }
}
