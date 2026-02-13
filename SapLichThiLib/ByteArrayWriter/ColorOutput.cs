using CsvHelper;
using SapLichThiLib.ByteArrayWriter;
using SapLichThiLib.DataObjects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiLib.ByteArrayWriter
{
    internal class ColorOutput : IByteArrayOutput
    {
        public Dictionary<int, HashSet<StudyClass>> I_colors_studyClasses { get; set; }
        public Dictionary<StudyClass, List<ExamClass>> I_studyClass_examClasses { get; set; }

        public byte[] OutputByteArray()
        {
            var color_courses = I_colors_studyClasses.Select(x => (x.Key, x.Value.Select(y => y.Course).ToHashSet())).ToList();
            using var memStream = new MemoryStream();
            using (var writer = new StreamWriter(memStream))
            using (var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csvWriter.WriteField("colorNum");
                csvWriter.WriteField("courseId");
                csvWriter.WriteField("courseName");
                csvWriter.NextRecord();
                foreach (var (color, courseList) in color_courses)
                {
                    foreach (var course in courseList)
                    {
                        csvWriter.WriteField(color);
                        csvWriter.WriteField(course.ID);
                        csvWriter.WriteField(course.Name);
                        csvWriter.NextRecord();
                    }
                }
            }
            return memStream.ToArray();
        }
    }
}
