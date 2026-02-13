using CsvHelper;
using SapLichThiLib.DataObjects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiLib.ByteArrayWriter
{
    public class CourseLinkageOutput : IByteArrayOutput
    {
        public Dictionary<Course, HashSet<Course>> I_courseLinkage { get; set; }
        public byte[] OutputByteArray()
        {
            using MemoryStream memStream = new MemoryStream();
            using (var writer = new StreamWriter(memStream))
            using (var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csvWriter.WriteField("Course 1");
                csvWriter.WriteField("Course 2");
                csvWriter.NextRecord();
                foreach (var kvp in I_courseLinkage)
                {
                    foreach (var course in kvp.Value)
                    {
                        csvWriter.WriteField(kvp.Key.ID);
                        csvWriter.WriteField(course.ID);
                        csvWriter.NextRecord();
                    }
                }
            }
            return memStream.ToArray();
        }
    }
}
