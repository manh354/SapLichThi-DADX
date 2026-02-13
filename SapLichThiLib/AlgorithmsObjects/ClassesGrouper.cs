using SapLichThiLib.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiLib.AlgorithmsObjects
{
    internal class ClassesGrouper
    {
        public List<StudyClass> I_studyClasses { get; set; }
        public Dictionary<Course, HashSet<StudyClass>> O_courseGroup { get; set; }
        public ClassesGrouper( List<StudyClass> allStudyClasses)
        {
            I_studyClasses = allStudyClasses;
        }

        public void GroupAllClasses()
        {
            
            O_courseGroup = new Dictionary<Course, HashSet<StudyClass>>();
            foreach (var studyClass in I_studyClasses)
            {
                /*if (!studyClass.Course.IsCommon)
                {
                    continue;
                }*/
                if(!O_courseGroup.ContainsKey(studyClass.Course))
                {
                    O_courseGroup.Add(studyClass.Course, new HashSet<StudyClass>());
                }
                O_courseGroup[studyClass.Course].Add(studyClass);
            }
        }
    }
}
