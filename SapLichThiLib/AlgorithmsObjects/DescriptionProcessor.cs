using SapLichThiLib.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiLib.AlgorithmsObjects
{
    public class DescriptionProcessor
    {
        // Input
        public List<StudyClass> StudyClasses { get; set; }
        // Output
        public Dictionary<StudyClass, string> StudyClass_ProcessedDescription_Dict;
        public DescriptionProcessor(List<StudyClass> studyClasses)
        {
            StudyClasses = studyClasses;
        }
        public void ProcessAllDescription()
        {
            StudyClass_ProcessedDescription_Dict = new();
            for (int i = 0; i < StudyClasses.Count; i++)
            {
                string desc = StudyClasses[i].Description;
                string processedDesc = ProcessString(desc);
                StudyClass_ProcessedDescription_Dict.Add(StudyClasses[i], processedDesc);
            }
            Console.WriteLine();
        }
        public string ProcessString(string s)
        {
            s = s.Replace("S", string.Empty);
            s = s.Replace("K", string.Empty);
            s = s.Replace("C", string.Empty);
            s = s.ToLowerInvariant();
            s = s.Replace(" ", string.Empty);
            var builder = new StringBuilder();
            foreach (var _char in s)
            {
                if(char.IsLetter(_char))
                    builder.Append(_char);
            }
            return builder.ToString();
        }
    }
}
