using CsvHelper;
using SapLichThiLib.DataObjects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiLib.ByteArrayReader
{
    internal class ByteArrayInput
    {
        public byte[] I_schoolsByteArray { get; set; }
        public byte[] I_coursesByteArray { get; set; }
        public byte[] I_studyClassesByteArray { get; set; }
        public byte[] I_studentYearsByteArray { get; set; }
        public byte[] I_studentsByteArray { get; set; }
        public byte[] I_timeFixedCourseByteArray { get; set; }
        public byte[] I_curriculumByteArray { get; set; }
        public byte[] I_studentsEnrollByteArray { get; set; }
        public byte[] I_buildingByteArray { get; set; }
        public byte[] I_roomsByteArray { get; set; }
        // public Dictionary<string, Student> StudentId_Students_Dict { get; set; }
        // public Dictionary<string, StudyClass> StudyClassId_StudyClass_Dict { get; set; }
        public Dictionary<Course, HashSet<Curriculum>> O_courseInCurriculaLinkage { get; set; }

        public List<Building> O_buildings { get; set; }
        public List<Room> O_rooms { get; set; }
        public List<School> O_schools { get; set; }
        public List<Course> O_courses { get; set; }
        public List<StudentYear> O_studentYears { get; set; }
        public List<StudyClass> O_studyClasses { get; set; }
        public List<Curriculum> O_curricula { get; set; }
        public Dictionary<string, Building> O_buildingId_building { get; set; }
        public Dictionary<string, School> O_schoolId_school { get; set; }
        public Dictionary<string, Course> O_courseId_course { get; set; }
        public Dictionary<string, StudyClass> O_studyClassId_studyClasses { get; set; }
        public List<ExamGroup> O_examGroups { get; set; }
        public List<CourseGroup> O_courseGroups { get; set; }
        public List<Student> O_students { get; set; }
        public Dictionary<string, Student> O_studentsId_student { get; set; }

        public void LoadAllDatas()
        {
            O_studentYears = LoadAllStudentYears();
            O_schools = LoadAllSchools();
            O_courses = LoadAllCourse();
            O_studyClasses = LoadAllStudyClasses();
            O_students = LoadAllStudents();
            O_examGroups = LoadAllExamGroups();
            O_buildings = LoadAllBuidings();
            O_rooms = LoadAllRooms();
            O_curricula = LoadAllCurricula();
            FillAllStudyClasses();
            ChangeToHash();
        }
        private List<School> LoadAllSchools()
        {
            O_schoolId_school = new Dictionary<string, School>();
            using (var memStream = new MemoryStream(I_schoolsByteArray))
            using (var reader = new StreamReader(memStream))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = new List<School>();
                csv.Read();
                csv.ReadHeader();
                DummyDebug();
                while (csv.Read())
                {
                    int a;
                    if (!int.TryParse(csv.GetField<string>("proctorCount"), out a))
                    {
                        a = 100;
                    }
                    var record = new School
                    (
                        id: csv.GetField<string>("schoolId"),
                        name: csv.GetField<string>("schoolName"),
                        maxProctor: a
                    );
                    records.Add(record);
                    O_schoolId_school.Add(record.ID, record);
                }
                return records;
            }
        }

        private List<StudentYear> LoadAllStudentYears()
        {
            using (var memStream = new MemoryStream(I_studentYearsByteArray))
            using (var reader = new StreamReader(memStream))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = new List<StudentYear>();
                csv.Read();
                csv.ReadHeader();
                DummyDebug();
                while (csv.Read())
                {
                    var record = new StudentYear(csv.GetField("studentYear"));
                    records.Add(record);
                }
                return records;
            }
        }

        private List<Building> LoadAllBuidings()
        {
            O_buildingId_building = new();
            using (var memStream = new MemoryStream(I_buildingByteArray))
            using (var reader = new StreamReader(memStream))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Read();
                csv.ReadHeader();
                DummyDebug();

                var records = new List<Building>();

                while (csv.Read())
                {
                    string buildingId = csv.GetField("buildingId")!;
                    var record = new Building
                    (
                        buildingId
                    );
                    O_buildingId_building.TryAdd(buildingId, record);
                    records.Add(record);
                }
                return records;
            }
        }

        private List<Room> LoadAllRooms()
        {
            using (var memStream = new MemoryStream(I_roomsByteArray))
            using (var reader = new StreamReader(memStream))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = new List<Room>();
                csv.Read();
                csv.ReadHeader();
                DummyDebug();
                while (csv.Read())
                {
                    var record = new Room
                    (
                        roomId: csv.GetField<string>("roomId"),
                        capacity: (int)(csv.GetField<float>("capacity")),
                        roomType: GetRoomType(csv.GetField("size")),
                        building: O_buildingId_building[csv.GetField("buildingId")]
                    );
                    records.Add(record);
                }
                return records;
            }
        }

        private List<Course> LoadAllCourse()
        {
            O_courseId_course = new();
            using (var memStream = new MemoryStream(I_coursesByteArray))
            using (var reader = new StreamReader(memStream))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = new List<Course>();
                csv.Read();
                csv.ReadHeader();
                DummyDebug();

                while (csv.Read())
                {
                    List<School> signupSchools = new List<School>();
                    var record = new Course
                    (
                        id: csv.GetField<string>("courseId"),
                        isCommon: csv.GetField<bool>("isCommon"),
                        manageSchool: O_schoolId_school[csv.GetField<string>("schoolId")],
                        signupSchools: signupSchools,
                        name: csv.GetField<string>("name")
                    );

                    O_courseId_course.Add(record.ID, record);
                    records.Add(record);
                }
                return records;
            }
        }
        private List<StudyClass> LoadAllStudyClasses()
        {
            O_studyClassId_studyClasses = new();
            using (var memStream = new MemoryStream(I_studyClassesByteArray))
            using (var reader = new StreamReader(memStream))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = new List<StudyClass>();
                csv.Read();
                csv.ReadHeader();
                DummyDebug();

                while (csv.Read())
                {
                    var record = new StudyClass
                    (
                        id: csv.GetField("classId"),
                        linkedClassId: csv.GetField("theoId"),
                        course: O_courseId_course[csv.GetField("courseId")],
                        isElitech: csv.GetField<bool>("isElitech"),
                        isExercise: csv.GetField<bool>("isExercise"),
                        description: csv.GetField("desc"),
                        studentYear: GetStudentYear(O_studentYears, csv.GetField("studentYear")),
                        term: csv.GetField("term"),
                        count: csv.GetField<int>("numStu")
                    );
                    O_studyClassId_studyClasses.Add(record.ID, record);
                    records.Add(record);
                    /*if (record.LinkedClassID == "-1")
                        continue;
                    record.LinkedClass = StudyClassId_StudyClasses_Dict[record.LinkedClassID];*/
                }
                return records;
            }
        }

        private List<Student> LoadAllStudents()
        {
            O_studentsId_student = new();
            using (var memStream = new MemoryStream(I_studentsByteArray))
            using (var reader = new StreamReader(memStream))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = new List<Student>();
                csv.Read();
                csv.ReadHeader();
                DummyDebug();

                while (csv.Read())
                {
                    var record = new Student
                    (
                        id: csv.GetField("studentId")
                    );
                    records.Add(record);
                    O_studentsId_student.Add(record.Id, record);
                }
                return records;
            }
        }


        private List<ExamGroup> LoadAllExamGroups()
        {
            using (var memStream = new MemoryStream(I_timeFixedCourseByteArray))
            using (var reader = new StreamReader(memStream))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = new List<ExamGroup>();
                csv.Read();
                csv.ReadHeader();
                DummyDebug();

                while (csv.Read())
                {
                    string coursesString = csv.GetField("courseIds")!;
                    var seperatedString = coursesString.Split('-');
                    List<Course> courses = new List<Course>();
                    foreach (var s in seperatedString)
                    {
                        try
                        {
                            courses.Add(O_courseId_course[s]);

                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.ToString());
                        }
                    }
                    if (courses.Count == 0)
                        continue;
                    string roomTypeString = csv.GetField("defaultRoom")!;
                    var allRoomTypesString = roomTypeString.Split("-");
                    var allRoomTypes = allRoomTypesString.Select(x => x.Contains('L') ? RoomType.large : x.Contains('M') ? RoomType.medium : RoomType.small).ToList();
                    var record = new ExamGroup
                    (
                        course: courses,
                        date: csv.GetField<int?>("date"),
                        numshift: csv.GetField<int>("numShift"),
                        defaultShift: csv.GetField<int?>("defaultShift"),
                        prioritizedRooms: allRoomTypes,
                        mode: csv.GetField("mode")!
                    );
                    records.Add(record);
                CONTINUE:
                    continue;
                }
                return records;
            }
        }

        private static StudentYear GetStudentYear(List<StudentYear> studentYears, string desc)
        {
            if (studentYears == null || studentYears.Count == 0)
                throw new Exception("Student year is corrupted");
            foreach (var year in studentYears)
            {
                if (desc.Contains(year.Name))
                    return year;
            }
            return studentYears[studentYears.Count - 1];
        }


        private void FillAllStudyClasses()
        {
            using (var memStream = new MemoryStream(I_studentsEnrollByteArray))
            using (var reader = new StreamReader(memStream))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Read();
                csv.ReadHeader();
                DummyDebug();

                while (csv.Read())
                {
                    string? studentId = csv.GetField("studentId");
                    string? classId = csv.GetField("classId");
                    if (classId == null) continue;
                    if (O_studyClassId_studyClasses.TryGetValue(classId, out StudyClass? thisStudyClass))
                    {
                        if (thisStudyClass.Students == null)
                            thisStudyClass.Students = new List<Student>();
                        thisStudyClass.Students.Add(O_studentsId_student[studentId]);
                    }
                }
            }
        }

        private List<Curriculum> LoadAllCurricula()
        {
            using (var memStream = new MemoryStream(I_curriculumByteArray))
            using (var reader = new StreamReader(memStream))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Read();
                csv.ReadHeader();
                DummyDebug();

                var allCurricula = new List<Curriculum>();
                var notfoundCourses = O_courses.ToHashSet();
                var courseHash = new HashSet<Course>();
                while (csv.Read())
                {
                    var record = new List<Course>();
                    string curriculumName = csv.GetField<string>("name")!;
                    string coursesCombined = csv.GetField<string>("courseIds")!;
                    string[] coursesIds = coursesCombined.Split('-');
                    foreach (var courseId in coursesIds)
                    {
                        if (O_courseId_course.TryGetValue(courseId, out Course course))
                        {
                            record.Add(course);
                        }
                    }
                    allCurricula.Add(new Curriculum(curriculumName, record));
                }
                return allCurricula;
            }
        }
        private void ChangeToHash()
        {
            O_courseInCurriculaLinkage = new();
            foreach (var curriculum in O_curricula)
            {
                foreach (var course in curriculum.Courses)
                {
                    if (!O_courseInCurriculaLinkage.ContainsKey(course))
                    {
                        O_courseInCurriculaLinkage.Add(course, new());
                    }
                    O_courseInCurriculaLinkage[course].Add(curriculum);
                }
            }
        }
        private static RoomType GetRoomType(string str)
        {
            if (str.StartsWith('S'))
                return RoomType.small;
            if (str.StartsWith('M'))
                return RoomType.medium;
            else
                return RoomType.large;
        }

        public (List<School>, List<Course>, List<StudyClass>, List<Room>, List<StudentYear>, List<Student>, List<ExamGroup>) GetAllDatas()
        {
            return (O_schools, O_courses, O_studyClasses, O_rooms, O_studentYears, O_students, O_examGroups);
        }
        private void DummyDebug()
        {

        }
    }
}
