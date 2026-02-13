using SapLichThiLib.DataStructures;
using SapLichThiLib.Extensions;
using SapLichThiLib.AlgorithmsObjects;
using SapLichThiLib.FileWriter;
using SapLichThiLib.Tests;
using SapLichThiLib.DataObjects;
using SapLichThiLib.ByteArrayReader;
using SapLichThiLib.ByteArrayWriter;

namespace SapLichThiLib
{
    public class SapLichThiMain
    {
        public DateOnly I_startDate { get; set; }
        public DateOnly I_endDate { get; set; }
        public int I_shiftCount { get; set; } = 4;
        public bool I_useAnnealing { get; set; } = true;
        public byte[] I_classesByteArray { get; set; } 
        public byte[] I_coursesByteArray { get; set; } 
        public byte[] I_roomsByteArray { get; set; } 
        public byte[] I_buildingsByteArray { get; set; } 
        public byte[] I_schoolsByteArray { get; set; } 
        public byte[] I_studentYearsByteArray { get; set; }
        public byte[] I_studentsByteArray { get; set; } 
        public byte[] I_studentsEnrollByteArray { get; set; } 
        public byte[] I_timeFixedCoursesByteArray { get; set; } 
        public byte[] I_curriculumByteArray { get; set; }
        public List<byte[]> O_outputFilesAsByteArray { get; set; } = [];
        public List<string> O_outputFilesName { get; set; } = [];
        public Algorithms O_algorithms { get; set; }
        public void Run()
        {

            Console.OutputEncoding = System.Text.Encoding.UTF8;
            ByteArrayInput fileReader = new ByteArrayInput()
            {
                I_studyClassesByteArray = I_classesByteArray,
                I_coursesByteArray = I_coursesByteArray,
                I_roomsByteArray = I_roomsByteArray,
                I_buildingByteArray = I_buildingsByteArray,
                I_schoolsByteArray = I_schoolsByteArray,
                I_studentYearsByteArray = I_studentYearsByteArray,
                I_studentsByteArray = I_studentsByteArray,
                I_studentsEnrollByteArray = I_studentsEnrollByteArray,
                I_timeFixedCourseByteArray = I_timeFixedCoursesByteArray,
                I_curriculumByteArray = I_curriculumByteArray,
            };
            fileReader.LoadAllDatas();
            // fileReaderKihe.LoadAllDatas();
            var Schools = fileReader.O_schools;
            var Courses = fileReader.O_courses;
            var StudyClasses = fileReader.O_studyClasses;
            //var ExamClasses = fileReaderKihe.O_examClasses;
            var Rooms = fileReader.O_rooms;
            var StudentYears = fileReader.O_studentYears;
            var Students = fileReader.O_students;
            var hardRails = fileReader.O_examGroups;
            var curriculas = fileReader.O_curricula;
            //TemporaryChangeToHardRails(hardRails, fileReader.O_courseId_course_Dict, fileReaderKihe.O_courseId_course);
            // Filtering
            //var StudyClassTemp = StudyClasses.Where(cls => cls.IsExercise && cls.Count > 19).ToList();
            //

            int shiftlen = I_shiftCount;
            List<int> Shifts = Enumerable.Range(0, shiftlen).ToList(); // DO NOT CHANGE THE STARTING INDEX
            DatesRangeCreator datesRangeCreator = new DatesRangeCreator(new DateOnly(2023, 2, 27), new DateOnly(2023, 3, 19));
            datesRangeCreator.MakeDatesRange();
            
            var AllClassHasStudent = StudyClasses.Count(x => x.Students != null);
            Console.WriteLine("Số lượng lớp có sinh viên : " + AllClassHasStudent);
            Console.WriteLine("Trên tổng số lớp là: " + StudyClasses.Count);
            int i = 0;
            Dictionary<StudentYear, int> studentYear_prioritizedShift =
                new(StudentYears.Select(x => new KeyValuePair<StudentYear, int>(x, Math.Min(i++, 41))).ToArray());
            Algorithms algorithms = new Algorithms()
            {
                I_studyClasses = StudyClasses,
                //I_examClasses = ExamClasses,
                I_studentYears = StudentYears,
                I_curricula = curriculas,
                I_commonDivideThreshold = 60,
                I_dates = datesRangeCreator.Dates,
                I_shifts = Shifts,
                I_rooms = Rooms,
                I_examGroups = hardRails,
                I_increaseThreshold = 4f / 3,
                I_nonCommonDivideThreshold = 70,
                I_studentYear_prioritizedShift = studentYear_prioritizedShift,
                I_useAnnealing = I_useAnnealing,
            };
            algorithms.ProcessDataAndMakeSchedule();
            O_algorithms = algorithms;

            ScheduleOutput ScheduleFileWriter = new ScheduleOutput()
            {
                I_schedule = algorithms.O_schedule,
            };
            var scheduleByteArray = ScheduleFileWriter.OutputByteArray();
            O_outputFilesAsByteArray.Add(scheduleByteArray);
            O_outputFilesName.Add("Schedule.csv");



            ColorOutput colorOutput = new ColorOutput()
            {
                I_colors_studyClasses = algorithms.O_colorGroups,
                I_studyClass_examClasses = algorithms.O_studyClass_examClasses
            };
            var colorByteArray = colorOutput.OutputByteArray();
            O_outputFilesAsByteArray.Add(colorByteArray);
            O_outputFilesName.Add("Color.csv");



            CourseLinkageOutput courseLinkageOutput = new CourseLinkageOutput()
            {
                I_courseLinkage = algorithms.O_courseLinkage,
            };
            var courseLinkageByteArray = courseLinkageOutput.OutputByteArray();
            O_outputFilesAsByteArray.Add(courseLinkageByteArray);
            O_outputFilesName.Add("Linkages.csv");



            TestRoomOutput testArrayCellsFileWriter = new TestRoomOutput()
            {
                AllTestObjects = new RoomTest(algorithms.O_schedule).GiveTestResult()
            };
            var testCellByteArray = testArrayCellsFileWriter.OutputByteArray();
            O_outputFilesAsByteArray.Add(testCellByteArray);
            O_outputFilesName.Add("RoomTest.csv");



            TestStudentConflictListOutput testStudentFileWriter = new TestStudentConflictListOutput()
            {
                I_schedule = algorithms.O_schedule,
                AllConflictingStudentAndTheirClasses = new StudentConflictTest(algorithms.O_schedule, fileReader.O_studyClassId_studyClasses).GiveTestResult()
            };
            var testStudentByteArray = testStudentFileWriter.OutputByteArray();
            O_outputFilesAsByteArray.Add(testStudentByteArray);
            O_outputFilesName.Add("StudentConflictTest.csv");



            TestMissingExamClassOutput testMissingExamClassOutput = new TestMissingExamClassOutput()
            {
                I_color_examClass = algorithms.O_color_examClasses,
                I_examClasses = new MissingClassTest() { I_allExamClasses = algorithms.O_examClasses, I_schedule = algorithms.O_schedule }.GiveTestResult(),
            };
            var testMissingExamClassByteArray = testMissingExamClassOutput.OutputByteArray();
            O_outputFilesAsByteArray.Add(testMissingExamClassByteArray);
            O_outputFilesName.Add("MissingClassTest.csv");



            TestScheduleSeperationOutput testScheduleSeperationOutput = new TestScheduleSeperationOutput()
            {
                I_curriculumLinkages = new ScheduleSeparationTest()
                {
                    I_schedule = algorithms.O_schedule,
                    I_curricula = curriculas,

                }.GiveTestResult(),
            };
            var testScheduleSeperationByteArray = testScheduleSeperationOutput.OutputByteArray();
            O_outputFilesAsByteArray.Add(testScheduleSeperationByteArray);
            O_outputFilesName.Add("SeperationTest.csv");



            TestProctorCountOutput testProctorCountOutput = new TestProctorCountOutput()
            {
                I_proctorTestObjects = new ProctorTest()
                {
                    I_schedule = algorithms.O_schedule,
                    I_schools = Schools
                }.GiveTestResult(),
            };
            var testProctorCountByteArray = testProctorCountOutput.OutputByteArray();
            O_outputFilesAsByteArray.Add(testProctorCountByteArray);
            O_outputFilesName.Add("ProctorCountTest.csv");


            TestStudentYearCountOutput testStudentYearOutput = new TestStudentYearCountOutput()
            {
                I_studentYears = StudentYears,
                I_studentYearTestObjects = new StudentYearCountTest()
                {
                    I_course_mainStudentYear = algorithms.O_course_mainStudentYear,
                    I_schedule = algorithms.O_schedule,
                }.GiveTestResult()
            };
            var testStudentYearByteArray = testStudentYearOutput.OutputByteArray();
            O_outputFilesAsByteArray.Add(testStudentYearByteArray);
            O_outputFilesName.Add("StudentYearTest.csv");



            TestStudentYearOkOutput testStudentYearOkOutput = new TestStudentYearOkOutput()
            {
                I_path = @"Outputs\StudentYearOkTest.csv",
                I_studentYearOkTestObjects = new StudentYearOkTest()
                {
                    I_schedule = algorithms.O_schedule,
                    I_course_mainStudentYear = algorithms.O_course_mainStudentYear,
                    I_slot_largestYear = algorithms.O_slot_largestYears,
                    I_studentYear_prioritizedShift = algorithms.I_studentYear_prioritizedShift,
                }.GiveTestResult()
            };
            var testStudentYearOkByteArray = testStudentYearOkOutput.OutputByteArray();
            O_outputFilesAsByteArray.Add(testStudentYearOkByteArray);
            O_outputFilesName.Add("StudentYearOkTest.csv");



            EmptyCellDetailsOutput emptyCellDetailsOutput = new EmptyCellDetailsOutput()
            {
                I_schedule = algorithms.O_schedule,
            };
            var emptyCellDetailsByteArray = emptyCellDetailsOutput.OutputByteArray();
            O_outputFilesAsByteArray.Add(emptyCellDetailsByteArray);
            O_outputFilesName.Add("EmptyCellDetailsOutput.csv");



            EmptyCellTotalOutput emptyCellTotalOutput = new EmptyCellTotalOutput()
            {
                I_schedule = algorithms.O_schedule,
            };
            var emotyCellTotalByteArray = emptyCellTotalOutput.OutputByteArray();
            O_outputFilesAsByteArray.Add(emotyCellTotalByteArray);
            O_outputFilesName.Add("EmptyCellTotalOutput.csv");
        }
    }
}
