using CsvHelper;
using SapLichThiLib.DataStructures;
using SapLichThiLib.DataObjects;
using SapLichThiLib.Tests;
using SapLichThiLib.Tests.TestObject;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiLib.ByteArrayWriter
{
    public class TestArrayCellsCSVOutput : IByteArrayOutput
    {
        public IEnumerable<ArrayCell> AllOutOfBoundCells { get; set; }
        public byte[] OutputByteArray()
        {
            using (var memStream = new MemoryStream())
            using (var writer = new StreamWriter(memStream))
            using (var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csvWriter.WriteField("");
                csvWriter.WriteField("Trường/Viện");
                csvWriter.WriteField("Mã lớp");
                csvWriter.WriteField("Mã HP");
                csvWriter.WriteField("Tên học phần");
                csvWriter.WriteField("Ghi chú");
                csvWriter.WriteField("Nhóm");
                csvWriter.WriteField("SLĐK");
                csvWriter.WriteField("Mã lớp thi");
                csvWriter.NextRecord();
                foreach (ArrayCell thisCell in AllOutOfBoundCells)
                {
                    foreach (var examClass in thisCell.ExamClasses)
                    {
                        csvWriter.WriteField(examClass.StudyClass.Course.School.Name);
                        csvWriter.WriteField(examClass.StudyClass.ID);
                        csvWriter.WriteField(examClass.StudyClass.Course.ID);
                        csvWriter.WriteField(examClass.StudyClass.Course.Name);
                        csvWriter.WriteField(examClass.StudyClass.Description);
                        csvWriter.WriteField(examClass.Description);
                        csvWriter.WriteField(examClass.Count);
                        csvWriter.WriteField(examClass.ID);
                        csvWriter.NextRecord();
                    }
                }
                csvWriter.Flush();
                return memStream.ToArray();
            }
        }
    }

    public class TestRoomOutput : IByteArrayOutput
    {
        public IEnumerable<RoomTestObject> AllTestObjects { get; set; }

        public byte[] OutputByteArray()
        {
            using MemoryStream memStream = new MemoryStream() ;
            using (var writer = new StreamWriter(memStream))
            using (var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csvWriter.WriteField("Trường/Viện");
                csvWriter.WriteField("Mã lớp");
                csvWriter.WriteField("Mã HP");
                csvWriter.WriteField("Tên học phần");
                csvWriter.WriteField("Ghi chú");
                csvWriter.WriteField("Nhóm");
                csvWriter.WriteField("SLĐK");
                csvWriter.WriteField("Mã lớp thi");
                csvWriter.WriteField("Ngày thi");
                csvWriter.WriteField("Kíp thi");
                csvWriter.WriteField("Phòng thi");
                csvWriter.WriteField("Sức chứa phòng");
                csvWriter.WriteField("Có hợp lệ?");
                csvWriter.WriteField("Tỉ lệ chia phòng");
                csvWriter.NextRecord();
                foreach (RoomTestObject testObject in AllTestObjects)
                {
                    var arrayCell = testObject.ArrayCell;
                    foreach (var examClass in testObject.ArrayCell.ExamClasses)
                    {
                        csvWriter.WriteField(examClass.StudyClass.Course.School.Name);
                        csvWriter.WriteField(examClass.StudyClass.ID);
                        csvWriter.WriteField(examClass.StudyClass.Course.ID);
                        csvWriter.WriteField(examClass.StudyClass.Course.Name);
                        csvWriter.WriteField(examClass.StudyClass.Description);
                        csvWriter.WriteField(examClass.Description);
                        csvWriter.WriteField(examClass.Count);
                        csvWriter.WriteField(examClass.ID);
                        csvWriter.WriteField(testObject.Date.ToString());
                        csvWriter.WriteField($"Kíp {testObject.Shift + 1}");
                        csvWriter.WriteField(testObject.Room.RoomId);
                        csvWriter.WriteField(testObject.Room.Capacity);
                        csvWriter.WriteField(testObject.Condition);
                        csvWriter.WriteField(testObject.Ratio);
                        csvWriter.NextRecord();
                    }
                }
            }
            return memStream.ToArray();
        }
    }

    /*public class TestExamClassesIntervalCSVOutput : OutputInterface
    {
        public string FilePath;
        public IEnumerable<CellLinkage> CellLinkages;
        public TestExamClassesIntervalCSVOutput(string path, IEnumerable<CellLinkage> cellLinkages)
        {
            FilePath = path;
            CellLinkages = cellLinkages;
        }
        public void OutputFile()
        {
            using (var writer = new StreamWriter(new FileStream(FilePath, FileMode.OpenOrCreate)))
            using (var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csvWriter.WriteField("Trường/Viện");
                csvWriter.WriteField("Mã lớp 1");
                csvWriter.WriteField("Mã HP 1");
                csvWriter.WriteField("Tên học phần 1");
                csvWriter.WriteField("Ghi chú 1");

                csvWriter.WriteField("Mã lớp 2");
                csvWriter.WriteField("Mã HP 2");
                csvWriter.WriteField("Tên học phần 2");
                csvWriter.WriteField("Ghi chú 2");
                csvWriter.NextRecord();
                foreach (var linkage in CellLinkages)
                {
                    csvWriter.WriteField(linkage.Cell1.StudyClass.Course.School.Name);
                    csvWriter.WriteField(linkage.Cell1.StudyClass.ID);
                    csvWriter.WriteField(linkage.Cell1.StudyClass.Course.ID);
                    csvWriter.WriteField(linkage.Cell1.StudyClass.Course.Name);
                    csvWriter.WriteField(linkage.Cell1.StudyClass.Description);

                    csvWriter.WriteField(linkage.Cell2.StudyClass.ID);
                    csvWriter.WriteField(linkage.Cell2.StudyClass.Course.ID);
                    csvWriter.WriteField(linkage.Cell2.StudyClass.Course.Name);
                    csvWriter.WriteField(linkage.Cell2.StudyClass.Description);
                    csvWriter.NextRecord();

                }
            }
        }
    }*/

    public class TestStudentConflictListOutput : IByteArrayOutput
    {
        public ExamSchedule I_schedule { get; set; }
        public IEnumerable<StudentConflictTestObject> AllConflictingStudentAndTheirClasses { get; set; }

        public byte[] OutputByteArray()
        {
            using MemoryStream memStream = new MemoryStream();
            using (var writer = new StreamWriter(memStream))
            using (var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csvWriter.WriteField("Mã sinh viên");
                csvWriter.WriteField("Mã lớp 1");
                csvWriter.WriteField("Mã HP 1");
                csvWriter.WriteField("Tên học phần 1");
                csvWriter.WriteField("Ghi chú 1");

                csvWriter.WriteField("Mã lớp 2");
                csvWriter.WriteField("Mã HP 2");
                csvWriter.WriteField("Tên học phần 2");
                csvWriter.WriteField("Ghi chú 2");

                csvWriter.WriteField("Ngày thi");
                csvWriter.WriteField("Kíp thi");
                csvWriter.WriteField("Phòng thi 1");
                csvWriter.WriteField("Phòng thi 2");

                csvWriter.NextRecord();
                foreach (var conflictObject in AllConflictingStudentAndTheirClasses)
                {
                    foreach (var (value_1, value_2) in conflictObject.ConflictPairs)
                    {
                        Student student = conflictObject.Student;
                        StudyClass one = value_1.cell.ExamClasses[0].StudyClass;
                        StudyClass two = value_2.cell.ExamClasses[0].StudyClass;
                        csvWriter.WriteField(student.Id);
                        csvWriter.WriteField(one.ID);
                        csvWriter.WriteField(one.Course.ID);
                        csvWriter.WriteField(one.Course.Name);
                        csvWriter.WriteField(one.Description);

                        csvWriter.WriteField(two.ID);
                        csvWriter.WriteField(two.Course.ID);
                        csvWriter.WriteField(two.Course.Name);
                        csvWriter.WriteField(two.Description);

                        csvWriter.WriteField(value_1.date.ToString("dd/MM/yyyy"));
                        csvWriter.WriteField($"Kíp {value_1.shift + 1}");
                        csvWriter.WriteField(value_1.room.RoomId) ;
                        csvWriter.WriteField(value_2.room.RoomId);

                        csvWriter.NextRecord();
                    }
                }
            }
            return memStream.ToArray();
        }
    }

    public class TestMissingExamClassOutput : IByteArrayOutput
    {
        public IEnumerable<ExamClass> I_examClasses { get; set; }
        public Dictionary<int, List<ExamClass>> I_color_examClass { get; set; }

        public byte[] OutputByteArray()
        {
            Dictionary<ExamClass, int> examClass_color_Dict = new();
            foreach (var color_examClass in I_color_examClass)
            {
                foreach (var examClass in color_examClass.Value)
                {
                    examClass_color_Dict.Add(examClass, color_examClass.Key);
                }
            }
            using var memStream = new MemoryStream();
            using (var writer = new StreamWriter(memStream))
            using (var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csvWriter.WriteField("Mã lớp thi");
                csvWriter.WriteField("Mã màu");
                csvWriter.WriteField("Mã lớp học");
                csvWriter.WriteField("Mã học phần");
                csvWriter.WriteField("Tên học phần");
                csvWriter.WriteField("Ghi chú");
                csvWriter.NextRecord();
                foreach (var examClass in I_examClasses)
                {
                    csvWriter.WriteField(examClass.ID); // Mã lớp thi
                    if (examClass_color_Dict.TryGetValue(examClass, out int color))
                        csvWriter.WriteField(examClass_color_Dict[examClass]); // Mã màu
                    else
                        csvWriter.WriteField("chung");
                    csvWriter.WriteField(examClass.StudyClass.ID); // Mã lớp học
                    csvWriter.WriteField(examClass.StudyClass.Course.ID); // Mã học phần 
                    csvWriter.WriteField(examClass.StudyClass.Course.Name); // Tên học phần
                    csvWriter.WriteField(examClass.StudyClass.Description); // Ghi chú 
                    csvWriter.NextRecord();
                }
            }
            return memStream.ToArray();
        }
    }

    public class TestScheduleSeperationOutput : IByteArrayOutput
    {
        public IEnumerable<CurriculumLinkage> I_curriculumLinkages { get; set; }

        public byte[] OutputByteArray()
        {
            using MemoryStream memStream = new MemoryStream();
            using (var writer = new StreamWriter(memStream))
            using (var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csvWriter.WriteField("Tên chương trình đào tạo");
                csvWriter.WriteField("Mã học phần");
                csvWriter.WriteField("Tên học phần");
                csvWriter.WriteField("Ngày thi");
                csvWriter.NextRecord();
                foreach (var curriculumLinkage in I_curriculumLinkages)
                {
                    foreach (var (course, date) in curriculumLinkage.Course_Dates)
                    {
                        csvWriter.WriteField(curriculumLinkage.Curriculum.CurriculumName);
                        csvWriter.WriteField(course.ID);
                        csvWriter.WriteField(course.Name);
                        csvWriter.WriteField(date);
                        csvWriter.NextRecord();
                    }
                }
            }
            return memStream.ToArray();
        }
    }

    public class TestProctorCountOutput : IByteArrayOutput
    {
        public IEnumerable<ProctorTestObject> I_proctorTestObjects { get; set; }

        public byte[] OutputByteArray()
        {
            using var memStream = new MemoryStream();
            using (var writer = new StreamWriter(memStream))
            using (var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csvWriter.WriteField("Ngày");
                csvWriter.WriteField("Kíp");
                csvWriter.WriteField("Mã đơn vị");
                csvWriter.WriteField("Tên đơn vị");
                csvWriter.WriteField("Số giám thị còn lại");
                csvWriter.NextRecord();
                foreach (var proctorTestObject in I_proctorTestObjects)
                {
                    foreach (var (key,value) in proctorTestObject.School_ProctorRemainCount)
                    {
                        csvWriter.WriteField(proctorTestObject.Date);
                        csvWriter.WriteField(proctorTestObject.Shift);
                        csvWriter.WriteField(key.ID);
                        csvWriter.WriteField(key.Name);
                        csvWriter.WriteField(value);
                        csvWriter.NextRecord();
                    }
                }
            }
            return memStream.ToArray();
        }
    }

    public class TestStudentYearCountOutput : IByteArrayOutput
    {
        public IEnumerable<StudentYearCountTestObject> I_studentYearTestObjects { get; set; }
        public List<StudentYear> I_studentYears { get; set; }

        public byte[] OutputByteArray()
        {
            using var memStream = new MemoryStream();
            using (var writer = new StreamWriter(memStream))
            using (var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csvWriter.WriteField("Ngày");
                csvWriter.WriteField("Kíp");
                foreach (var studentYear in I_studentYears)
                {
                    csvWriter.WriteField($"{studentYear.Name}");
                }
                csvWriter.NextRecord();
                foreach (var studentYearTestObject in I_studentYearTestObjects)
                {
                    if (studentYearTestObject.StudentYears_Count.Count == 0)
                        continue;
                    int[] array = new int[I_studentYears.Count];
                    csvWriter.WriteField(studentYearTestObject.Date);
                    csvWriter.WriteField(studentYearTestObject.Shift + 1);
                    for (int i = 0; i < array.Length; i++)
                    {
                        foreach (var (studentYear, count) in studentYearTestObject.StudentYears_Count)
                        {
                            if (studentYear == I_studentYears[i])
                                array[i] += count;
                        }
                    }
                    for (int i = 0; i < array.Length; i++)
                    {
                        csvWriter.WriteField(array[i]);
                    }
                    csvWriter.NextRecord();
                }
            }
            return memStream.ToArray();
        }
    }
    public class TestStudentYearOkOutput : IByteArrayOutput
    {
        public string I_path { get; set; }
        public IEnumerable<StudentYearOkTestObject> I_studentYearOkTestObjects { get; set;}

        public byte[] OutputByteArray()
        {
            using var memStream = new MemoryStream();
            using (var writer = new StreamWriter(memStream))
            using (var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csvWriter.WriteField("Ngày");
                csvWriter.WriteField("Kíp");
                csvWriter.WriteField("Khóa sv chính");
                csvWriter.WriteField("Đúng kíp");
                csvWriter.NextRecord();
                foreach (var studentYearOkTestObject in I_studentYearOkTestObjects)
                {
                    csvWriter.WriteField(studentYearOkTestObject.Date);
                    csvWriter.WriteField(studentYearOkTestObject.Shift + 1);
                    csvWriter.WriteField(studentYearOkTestObject.StudentYear.Name);
                    csvWriter.WriteField(studentYearOkTestObject.Okness ? "Đúng" : "Sai");
                    csvWriter.NextRecord();
                }
            }
            return memStream.ToArray();
        }
    }
}
