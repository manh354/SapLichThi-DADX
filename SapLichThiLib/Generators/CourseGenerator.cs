using SapLichThiLib.Extensions;
using SapLichThiLib.DataObjects;

namespace SapLichThiLib.Generator
{
    public class CourseGenerator
    {
        public static List<Course> generatedCourses = new();
        public static List<Course> GenerateRandomCourses(int num, List<School> schools)
        {
            List<int> generatedId = new List<int>();
            Random random = new Random();
            int maxNum = num * 10;
            int temp;
            for (int i = 0; i < num; i++)
            {
                temp = random.Next(maxNum);
                if (generatedId.Contains(temp))
                    continue;
                else generatedId.Add(temp);
            }

            List<Course> courses = new List<Course>();
            for (int i = 0; i < generatedId.Count; i++)
            {
                courses.Add(new Course(generatedId[i].ToString(), RandomExtension.ChooseProbability(0.1), schools[random.Next(schools.Count)],null, "random name"));
            }
            generatedCourses = courses;
            return courses;
        }
    }
}
