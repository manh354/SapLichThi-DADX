namespace SapLichThiCore.DataObjects
{
    public class ExamClass
    {
        string id;
        public string Id => id;
        int count;
        public int Count => Students == null ? count : Students.Count;
        public int duration;
        public int Duration => duration;
        public List<Student> Students { get; set; }
        public ExamClass(string id,  int count, int duration, List<Student> students = null)
        {
            this.id = id;
            this.count = count;
            this.duration = duration;
            Students = students;
        }
        public override string ToString()
        {
            return $"{Id}, c: {count}";
        }
    }
}
