using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SapLichThiLib.DataObjects;

namespace SapLichThiLib.Generator
{
    internal class SchoolGenerator
    {
        public static List<School> generatedSchools = new();

        public static List<School> GenerateRandomSchools(int num)
        {
            Random random = new Random();
            List<School> randomStudents = new List<School>();
            for (int i = 0; i < num; i++)
            {
                randomStudents.Add(new School(random.Next(1000).ToString()));
            }
            generatedSchools = randomStudents;
            return randomStudents;
        }


    }
}
