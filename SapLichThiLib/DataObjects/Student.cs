namespace SapLichThiLib.DataObjects
{
    public class Student
    {
        string id;
        public string Id => id;
        string name;
        public Student(string id)
        {
            this.id = id;
        }
        public Student(string id, string name) : this(id)
        {
            this.name = name;
        }

        public override string ToString()
        {
            return string.Format("Student: id:{0,-10}", id);
        }
    }
}
