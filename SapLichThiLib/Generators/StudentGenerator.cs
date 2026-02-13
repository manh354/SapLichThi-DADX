using SapLichThiLib.DataObjects;

namespace SapLichThiLib.Generator
{
    internal class StudentGenerator
    {
        public static List<Student> generatedStudents = new();

        public static List<Student> GenerateRandomIdStudents(int num)
        {
            Random random = new Random();
            List<Student> randomStudents = new List<Student>();
            for (int i = 0; i < num; i++)
            {
                randomStudents.Add(new Student(random.Next(num * 100).ToString()));
            }
            generatedStudents = randomStudents;
            return randomStudents;
        }
    }
}
