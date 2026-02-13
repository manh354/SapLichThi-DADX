using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.Serialization.Formatters;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using SapLichThiLib.DataStructures;
using SapLichThiLib.DataObjects;
using SapLichThiLib.AlgorithmsObjects.ExamGroupInserter;
using SapLichThiLib.AlgorithmsObjects.Coloring;
using SapLichThiLib.AlgorithmsObjects.DynamicPooling;
using SapLichThiLib.AlgorithmsObjects.AnnealingOptimizations;

namespace SapLichThiLib.AlgorithmsObjects
{
    public class Algorithms
    {
        // Input
        public List<StudyClass> I_studyClasses { get; set; }
        public List<ExamClass> I_examClasses { get; set; }
        public List<StudentYear> I_studentYears { get; set; }
        public List<DateOnly> I_dates { get; set; }
        public List<int> I_shifts { get; set; }
        public List<Room> I_rooms { get; set; }
        public List<ExamGroup> I_examGroups { get; set; }
        public List<Curriculum> I_curricula { get; set; }
        public Dictionary<StudentYear, int> I_studentYear_prioritizedShift { get; set; }
        public Dictionary<StudyClass, List<ExamClass>> O_studyClass_examClasses { get; set; }
        public Dictionary<int, HashSet<StudyClass>> O_colorGroups { get; set; }
        public Dictionary<Course, StudentYear> O_course_mainStudentYear { get; set; }
        public Dictionary<(int date, int shift), StudentYear?> O_slot_largestYears { get; set; }
        public Dictionary<string, HashSet<Course>> I_stringDateShiftRoom_course { get; set; }
        public int[] I_biasTable { get; set; }
        public int I_commonDivideThreshold { get; set; }
        public int I_nonCommonDivideThreshold { get; set; }
        public float I_increaseThreshold { get; set; }
        // Output
        public ExamSchedule O_schedule { get; set; }
        public Dictionary<int, List<ExamClass>> O_color_examClasses { get; set; }
        public List<ExamClass> O_examClasses { get; set; }
        public Dictionary<Course, HashSet<Course>> O_courseLinkage { get; set; }
        public Dictionary<(int date, int shift), List<Course>> O_slot_courses { get; set; }
        public bool I_useAnnealing { get; set; }
        public void ProcessDataAndMakeSchedule()
        {
            //I_studyClasses = I_studyClasses.FindAll(x => x.Course.ID.StartsWith("MI"));
            ClassesGrouper classesGrouper = new(I_studyClasses);
            classesGrouper.GroupAllClasses();
            CommonClassSeparator commonClassSeperator = new()
            {
                 AllStudyClasses = I_studyClasses,
                 HardRails = I_examGroups,
            };
            commonClassSeperator.SeperateClasses();
            CommonClassDivider commonClassesDivider = new()
            {
                I_allStudyClasses = I_studyClasses,
                I_commonDivideThreshold = I_commonDivideThreshold,
                I_allHardRails = I_examGroups,
                I_courseGroup = classesGrouper.O_courseGroup
            };
            commonClassesDivider.Run();
            
            NonCommonClassDivider nonCommonClassDivider = new NonCommonClassDivider()
            {
                AllNonCommonStudyClasses = commonClassSeperator.O_allNonCommonClasses,
                StartId = commonClassesDivider.O_id,
                NonCommonDivideThreshold = I_nonCommonDivideThreshold
            };

            nonCommonClassDivider.Run();
            O_examClasses = nonCommonClassDivider.O_examClasses.Concat(commonClassesDivider.O_examClasses).ToList();
            O_studyClass_examClasses = nonCommonClassDivider.O_studyClasses_examClasses;

            DescriptionProcessor descriptionProcessor = new(I_studyClasses);
            descriptionProcessor.ProcessAllDescription();

            ScheduleInitializer scheduleInitializer = new ScheduleInitializer()
            {
                RoomList = I_rooms,
                DateList = I_dates,
                ShiftList = I_shifts
            };
            scheduleInitializer.CreateEmptySchedule();
            ExamGroupInserter.ExamGroupInserter scheduleHardRailing = new ExamGroupInserter.ExamGroupInserter()
            {
                I_examGroups = I_examGroups,
                I_course_classes = classesGrouper.O_courseGroup,
                I_studyClass_examClasses = commonClassesDivider.O_studyClass_examClasses,
                //I_studyClass_examClasses = O_studyClass_examClasses,
                I_biasTable = new int[] { 1, 2, 3, 0, 4 },
                I_totalLargeRoomCapacity = scheduleInitializer.TotalLargeRoomCapacity,
                I_schedule = scheduleInitializer.O_schedule,
                I_slots_condition = scheduleInitializer.DateShift_Reserved_Dictionary
            };
            scheduleHardRailing.Run();
            O_schedule = scheduleHardRailing.I_schedule;
            ScheduleResourcesSeperator scheduleResourcesSeperator = new ScheduleResourcesSeperator()
            {
                I_schedule = O_schedule,
                // O_course_partialEmptySlots
                // O_partialEmptySlots
                // O_emptySlots
            };
            scheduleResourcesSeperator.Run();

            /*AdaperToColor adapterToColor = new AdaperToColor()
            {
                I_stringDateShiftRoom_course = I_stringDateShiftRoom_course,
                // O_color_courses
            };
            adapterToColor.Run();

            SearchCommonCourseInColor searchCommonCourseInColor = new SearchCommonCourseInColor()
            {
                I_color_courses = adapterToColor.O_color_courses,
                I_commonCourses = hardrailClassesDivider.O_commonCourse.ToHashSet(),
            };
            searchCommonCourseInColor.Run();

            GroupStudyClassInColor groupStudyClassInColor = new GroupStudyClassInColor()
            {
                I_commonCourses = hardrailClassesDivider.O_commonCourse.ToHashSet(),
                I_color_courses = adapterToColor.O_color_courses,
                I_course_studyClasses = classesGrouper.CourseGroup,
                // O_color_studyClasses
            };
            groupStudyClassInColor.Run();

            SchemeCreater schemeCreater = new SchemeCreater()
            {
                I_color_studyClasses = groupStudyClassInColor.O_color_studyClasses,
                I_course_studyClasses = classesGrouper.CourseGroup,
                I_studyClasses_examClasses = nonCommonClassDivider.StudyClasses_ExamClasses_Dictionary,
                I_commonCourse_partialEmptySlot = scheduleResourcesSeperator.O_commonCourse_partialEmptySlots,
                I_color_commonCourse = searchCommonCourseInColor.O_color_commonCourse,
                I_emptySlots = scheduleResourcesSeperator.O_emptySlots,
                I_rooms = scheduleInitializer.Schedule.rooms.ToList(),
            };
            schemeCreater.Run();

            ScheduleInsert scheduleInsert = new ScheduleInsert()
            {
                I_schedule = scheduleInitializer.Schedule,
                I_classPositions = schemeCreater.O_classPositions
            };
            scheduleInsert.Run();
            ScheduleFilter scheduleFilter = new ScheduleFilter()
            {
                Schedule = O_schedule
            };
            scheduleFilter.FilterSchedule();*/
            /* Doan dang sau ko can thiet nua
             * Ta dang thu mot thuat toan moi
             * Ma du doan la se rat kho
             * 
             */
            CourseLinkageByCommonStudent courseLinkageByCommonStudent = new CourseLinkageByCommonStudent() 
            {
                I_courses_studyClasses = classesGrouper.O_courseGroup,
                I_studyClasses = I_studyClasses
            };
            courseLinkageByCommonStudent.LinkCourses();
            O_courseLinkage = courseLinkageByCommonStudent.O_courseLinkage;
            ClassGraphFiller nonCommonClassGraphFiller = new(commonClassSeperator.O_allNonCommonClasses, descriptionProcessor.StudyClass_ProcessedDescription_Dict)
            { I_courseLinkages = courseLinkageByCommonStudent.O_courseLinkage };
            nonCommonClassGraphFiller.FillClassGraph();
            ClassGraphColorer nonCommonClassGraphColorer = new ClassGraphColorer()
            {
                I_graph = nonCommonClassGraphFiller.O_classGraph,
            };
            nonCommonClassGraphColorer.ColorGraph();

            /*CourseGraphColorer courseGraphColorer = new CourseGraphColorer()
            {
                I_graph = new CourseGraph(courseLinkageByCommonStudent.O_courseLinkage)
            };
            courseGraphColorer.ColorGraph();*/

            /*CourseColorCombinator courseColorCombinator = new CourseColorCombinator()
            {
                I_examGroups = I_examGroups,
                I_color_courses = courseGraphColorer.O_color_courses,
                I_course_studyClasses = classesGrouper.O_courseGroup,
            };
            courseColorCombinator.Run();*/

            O_colorGroups = nonCommonClassGraphColorer.O_color_studyClasses;
            ClassRoomObjectBuilder classRoomObjectBuilder = new ClassRoomObjectBuilder()
            {
                I_dates = I_dates,
                I_shifts = I_shifts,
                I_rooms = I_rooms,
                I_emptySlots = scheduleResourcesSeperator.O_emptySlots,
                I_partialEmptySlots = scheduleResourcesSeperator.O_partialEmptySlots,
                I_courseLinkage = courseLinkageByCommonStudent.O_courseLinkage,
            };
            classRoomObjectBuilder.Run();

            MainStudentYearGenerator studentYearGenerator = new MainStudentYearGenerator()
            {
                I_course_studyClasses = classesGrouper.O_courseGroup,
                I_studentYear = I_studentYears,
                I_studyClass_examClasses = nonCommonClassDivider.O_studyClasses_examClasses.Concat(commonClassesDivider.O_studyClass_examClasses).Select(x=>x.Value).ToDictionary(x=>x.First().StudyClass)
            };
            studentYearGenerator.Run();
            O_course_mainStudentYear = studentYearGenerator.O_course_mainStudentYear;

            RoomFitting classRoomTreeSearcher = new RoomFitting()
            {
                I_color_studyClasses = nonCommonClassGraphColorer.O_color_studyClasses,
                I_studyClass_ExamClasses = nonCommonClassDivider.O_studyClasses_examClasses,
                I_course_mainStudentYear = studentYearGenerator.O_course_mainStudentYear,
                I_sea = classRoomObjectBuilder.O_sea
            };

            classRoomTreeSearcher.Run();

            O_color_examClasses = classRoomTreeSearcher.O_color_examClasses;

            FillScheduleFromSea fillScheduleFromSea = new FillScheduleFromSea()
            {
                I_classRoomSea = classRoomTreeSearcher.I_sea,
                I_schedule = O_schedule,
            };

            fillScheduleFromSea.Run();

            //nonCommonClassFitter.InsertIntoSchedule();
            /*ScheduleMaker schedualMaker = new ScheduleMaker(
                commonClassGraphColorer.Color_ClassesPairs,
                commonClassGrouper.CourseGroup,
                nonCommonClassGraphColorer.Color_ClassesPairs,
                examClassDivider.StudyClass_ExamClassPairs,
                schoolClassDivider.School_StudyClassesPairs,
                Rooms,
                Dates,
                Shifts,
                IncreaseThreshold);
            schedualMaker.MakeSchedual();*/

            SlotInfoMapper slotInfoMapper = new SlotInfoMapper()
            {
                I_schedule = O_schedule,
                I_curricula = I_curricula,
                I_hardRails = I_examGroups,
                I_studentYears = I_studentYears,
                I_course_mainStudentYear = studentYearGenerator.O_course_mainStudentYear,
            };
            slotInfoMapper.Run();
            O_slot_courses = slotInfoMapper.O_slot_courses;
            

            if (I_useAnnealing)
            {
                AnnealingOptimization annealingOptimization = new()
                {
                    I_schedule = O_schedule,
                    I_curricula = I_curricula,
                    I_hardRails = I_examGroups,
                    I_studentYears = I_studentYears,
                    I_studentYear_prioritizedShift = I_studentYear_prioritizedShift,
                    I_course_mainStudentYear = studentYearGenerator.O_course_mainStudentYear,
                    I_slot_courses = slotInfoMapper.O_slot_courses,
                    I_course_slots = slotInfoMapper.O_course_slots,
                    I_slot_largestYearCount = slotInfoMapper.O_slot_largestYearCount,
                    I_slot_largestYears = slotInfoMapper.O_slot_largestYears,
                    I_slot_movability = slotInfoMapper.O_slot_movability,
                };
                annealingOptimization.Run();
                O_slot_largestYears = annealingOptimization.I_slot_largestYears;
                /*AnnealingOptimizationForStudentYear annealingOptimizationForStudentYear = new()
                {
                    I_schedule = O_schedule,
                    I_examGroups = I_examGroups,
                    I_studentYears = I_studentYears,
                    I_studentYear_priorityShift = new(new KeyValuePair<StudentYear, int>[]{
                        new KeyValuePair<StudentYear, int>(I_studentYears[0],0),
                        new KeyValuePair<StudentYear, int>(I_studentYears[1],1),
                        new KeyValuePair<StudentYear, int>(I_studentYears[2],2),
                        new KeyValuePair<StudentYear, int>(I_studentYears[3],3),
                        new KeyValuePair<StudentYear, int>(I_studentYears[4],4),
                        new KeyValuePair<StudentYear, int>(I_studentYears[5],4),
                        new KeyValuePair<StudentYear, int>(I_studentYears[6],0),
                    })
                };
                annealingOptimizationForStudentYear.Run();*/
            }


        }    
    }
}
