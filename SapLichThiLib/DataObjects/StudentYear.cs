using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiLib.DataObjects
{
    public class StudentYear
    {
        string name;
        public string Name => name;
        public StudentYear (string name)
        {
            this.name = name;
        }

        /*public static StudentYear GetStudentYearFromDesc(string desc)
        {
            if (list.Count == 0)
                throw new Exception("List studentyear is null");
            foreach (var year in list)
            {
                if(desc.Contains(year.Name))
                {
                    return year;
                }
            }
            return null;
        }*/
    }
}
