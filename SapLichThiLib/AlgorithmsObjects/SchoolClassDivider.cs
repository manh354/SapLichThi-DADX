using SapLichThiLib.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiLib.AlgorithmsObjects
{
    public class SchoolClassDivider
    {
        public List<StudyClass> AllStudyClass { get; set; }
        public Dictionary<School, HashSet<StudyClass>> School_StudyClassesPairs { get; set; }
        public SchoolClassDivider(List<StudyClass> allStudyClass)
        {
            AllStudyClass = allStudyClass;
        }
        public void DivideStudyClassesToSchools()
        {
            School_StudyClassesPairs = new();
            foreach (var studyClass in AllStudyClass)
            {
                var thisClassSchool = studyClass.Course.School;
                if (School_StudyClassesPairs.ContainsKey(studyClass.Course.School))
                {
                    School_StudyClassesPairs[thisClassSchool].Add(studyClass);
                    continue;
                }
                School_StudyClassesPairs.Add(thisClassSchool, new HashSet<StudyClass>());
                School_StudyClassesPairs[thisClassSchool].Add(studyClass);
            }
        }
    }
}
