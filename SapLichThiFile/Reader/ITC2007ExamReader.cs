using SapLichThiCore.DataObjects;
using SapLichThiCore.DataType;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SapLichThiStream.Reader
{
    /// <summary>
    /// Reader for ITC 2007 Exam Timetabling format.
    /// Implements both IDataSource and ISchedulingModel interfaces.
    /// </summary>
    public class ITC2007ExamReader : IDataSource, ISchedulingModel
    {
        private readonly string _filePath;
        
        // Data storage
        private readonly List<ExamClass> _examClasses = new();
        private readonly Dictionary<string, ExamClass> _examClassMap = new(); // ID to Object
        private readonly Dictionary<int, ExamClass> _examIndexMap = new(); // Index to Object

        private readonly List<Student> _students = new();
        private readonly Dictionary<int, Student> _studentIdMap = new();

        private readonly List<Room> _rooms = new();
        private readonly Dictionary<Room, int> _roomPenalties = new();

        private readonly List<Period> _periods = new();
        private readonly Dictionary<Period, int> _periodWeights = new();
        
        private readonly List<BinaryConstraint> _binaryConstraints = new();
        private readonly List<UnaryConstraint> _unaryConstraints = new();

        // ITC 2007 specific data
        private int _twoInARow = 0;
        private int _twoInADay = 0;
        private int _periodSpread = 0;
        private int _nonMixedDurations = 0;
        private int[] _frontLoad = new int[] { 0, 0, 0 };
        private int _frontLoadThreshold = 0;
        
        // Scheduling model data
        private List<DateOnly> _dates = new();
        private int _numShift = 1;
        private bool _useAnnealing = true;
        private float _optimalPercentage = 1.0f;
        private bool _useExamClass = true;
        private int _startId = 0;
        
        public ITC2007ExamReader(string filePath)
        {
            _filePath = filePath;
            Load();
        }
        
        private void Load()
        {
            if (!File.Exists(_filePath))
                throw new FileNotFoundException($"ITC 2007 exam file not found: {_filePath}");
            
            using var reader = new StreamReader(_filePath);
            string line;
            string state = null;
            int count = 0;
            int linesRead = 0; // Track how many lines we've read for current section
            
            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();
                if (string.IsNullOrEmpty(line))
                    continue;
                
                // Check for section header [SectionName] or [SectionName:count]
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    // Reset counter when entering new section
                    linesRead = 0;
                    
                    state = line.Substring(1, line.Length - 2);
                    if (state.Contains(':'))
                    {
                        var parts = state.Split(':');
                        state = parts[0];
                        count = int.Parse(parts[1]);
                    }
                    else
                    {
                        count = 0;
                    }
                    continue;
                }
                
                // For sections with counts (Exams, Periods, Rooms), process all lines at once
                if (count > 0 && linesRead == 0)
                {
                    switch (state)
                    {
                        case "Exams":
                            ParseExams(reader, count, line);
                            linesRead = count;
                            count = 0; // Reset count after processing
                            continue;
                        case "Periods":
                            ParsePeriods(reader, count, line);
                            linesRead = count;
                            count = 0;
                            continue;
                        case "Rooms":
                            ParseRooms(reader, count, line);
                            linesRead = count;
                            count = 0;
                            continue;
                    }
                }
                
                // For sections without counts, process line by line
                if (count == 0)
                {
                    switch (state)
                    {
                        case "PeriodHardConstraints":
                            ParsePeriodHardConstraints(line);
                            break;
                        case "RoomHardConstraints":
                            ParseRoomHardConstraints(line);
                            break;
                        case "InstitutionalWeightings":
                            ParseInstitutionalWeightings(line);
                            break;
                    }
                }
            }
            
            BuildSchedulingModel();
        }
        
        private void ParseExams(StreamReader reader, int count, string firstLine)
        {
            // Process the first line that was already read
            ProcessExamLine(firstLine, 0);
            
            // Process remaining lines
            for (int i = 1; i < count; i++)
            {
                var line = reader.ReadLine();
                if (string.IsNullOrEmpty(line))
                    continue;
                
                ProcessExamLine(line, i);
            }
        }
        
        private void ProcessExamLine(string line, int index)
        {
            var tokens = line.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length < 1)
                return;
            
            int length = int.Parse(tokens[0]);
            var examStudents = new List<Student>();
            
            for (int j = 1; j < tokens.Length; j++)
            {
                int studentId = int.Parse(tokens[j]);
                if (!_studentIdMap.TryGetValue(studentId, out var student))
                {
                    student = new Student(studentId.ToString());
                    _studentIdMap[studentId] = student;
                    _students.Add(student);
                }
                examStudents.Add(student);
            }

            var examClass = new ExamClass($"E{index}", examStudents.Count, length, examStudents);
            _examClasses.Add(examClass);
            _examClassMap[examClass.Id] = examClass;
            _examIndexMap[index] = examClass;
        }

        private void ParsePeriods(StreamReader reader, int count, string firstLine)
        {
            string lastPeriodDayStr = null;
            int lastPeriodDayIndex = 0;
            int lastPeriodTimeIndex = -1;

            // Helper to process line
            void Process(string l, int i)
            {
                 var tokens = l.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length < 4)
                    return;
                
                string day = tokens[0];
                string time = tokens[1];
                int length = int.Parse(tokens[2]);
                int weight = int.Parse(tokens[3]);
                
                int currentDayIndex;
                int currentTimeIndex;

                if (lastPeriodDayStr == null)
                {
                    currentDayIndex = 0;
                    currentTimeIndex = 0;
                }
                else if (lastPeriodDayStr.Equals(day))
                {
                    currentDayIndex = lastPeriodDayIndex;
                    currentTimeIndex = lastPeriodTimeIndex + 1;
                }
                else
                {
                    currentDayIndex = lastPeriodDayIndex + 1;
                    currentTimeIndex = 0;
                }

                // Determine DateOnly
                // We build dates list as we go or after. Let's lazily ensure dates exist
                while (_dates.Count <= currentDayIndex)
                {
                    var startDate = _dates.Count == 0 ? DateOnly.FromDateTime(DateTime.Today) : _dates.Last().AddDays(1);
                    _dates.Add(startDate);
                }

                var date = _dates[currentDayIndex];
                
                var period = new Period(i, date, currentTimeIndex, length);
                _periods.Add(period);
                _periodWeights[period] = weight;

                lastPeriodDayStr = day;
                lastPeriodDayIndex = currentDayIndex;
                lastPeriodTimeIndex = currentTimeIndex;
            }

            // Process the first line that was already read
            Process(firstLine, 0);
            
            // Process remaining lines
            for (int i = 1; i < count; i++)
            {
                var line = reader.ReadLine();
                if (string.IsNullOrEmpty(line))
                    continue;

                Process(line, i);
            }
            
            _frontLoadThreshold = Math.Max(0, _periods.Count - _frontLoad[1]);
        }
        
        private void ParseRooms(StreamReader reader, int count, string firstLine)
        {
            // Process the first line that was already read
            ProcessRoomLine(firstLine, 0);
            
            // Process remaining lines
            for (int i = 1; i < count; i++)
            {
                var line = reader.ReadLine();
                if (string.IsNullOrEmpty(line))
                    continue;

                // Stop if we hit a new section
                if (line.StartsWith("["))
                    break;

                ProcessRoomLine(line, i);
            }
        }
        
        private void ProcessRoomLine(string line, int index)
        {
            var tokens = line.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length < 2)
                return;
            
            int capacity = int.Parse(tokens[0]);
            int penalty = tokens.Length > 1 ? int.Parse(tokens[1]) : 0;
            
            var roomType = Room.GetRoomTypeFromCapacity(capacity);
            var room = new Room($"R{index}", capacity, roomType, null, false, 0);
            
            _rooms.Add(room);
            _roomPenalties[room] = penalty;
        }
        
        private void ParsePeriodHardConstraints(string line)
        {
            var tokens = line.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length < 3)
                return;

            int exam1Id = int.Parse(tokens[0]);
            string constraint = tokens[1];
            int exam2Id = int.Parse(tokens[2]);
            
            if (!_examIndexMap.TryGetValue(exam1Id, out var ex1) || !_examIndexMap.TryGetValue(exam2Id, out var ex2))
                return;
            
            if (ex1 == ex2)
                return;
            
             // "AFTER", "EXAM_COINCIDENCE", "EXCLUSION"
            BinaryConstraintType type;
            if (constraint == "EXAM_COINCIDENCE") type = BinaryConstraintType.SAME_SLOT;
            else if (constraint == "EXCLUSION") type = BinaryConstraintType.DIFFERENT_SLOT;
            else if (constraint == "AFTER") type = BinaryConstraintType.AFTER;
            else return;

            _binaryConstraints.Add(new BinaryConstraint
            {
                ExamClass1 = ex1,
                ExamClass2 = ex2,
                ConstraintType = type,
                IsHard = true
            });
        }
        
        private void ParseRoomHardConstraints(string line)
        {
            var tokens = line.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length < 2)
                return;
            
            int examId = int.Parse(tokens[0]);
            string constraint = tokens[1];
            
            if (constraint == "ROOM_EXCLUSIVE" && _examIndexMap.TryGetValue(examId, out var exam))
            {
                _unaryConstraints.Add(new UnaryConstraint
                {
                    ExamClass = exam,
                    ConstraintType = UnaryConstraintType.ROOM_EXCLUSIVE,
                    IsHard = true
                });
            }
        }
        
        private void ParseInstitutionalWeightings(string line)
        {
            var tokens = line.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length < 2)
                return;
            
            string constraint = tokens[0];
            switch (constraint)
            {
                case "TWOINAROW":
                    _twoInARow = int.Parse(tokens[1]);
                    break;
                case "TWOINADAY":
                    _twoInADay = int.Parse(tokens[1]);
                    break;
                case "PERIODSPREAD":
                    _periodSpread = int.Parse(tokens[1]);
                    break;
                case "NONMIXEDDURATIONS":
                    _nonMixedDurations = int.Parse(tokens[1]);
                    break;
                case "FRONTLOAD":
                    if (tokens.Length >= 4)
                    {
                        _frontLoad[0] = int.Parse(tokens[1]);
                        _frontLoad[1] = int.Parse(tokens[2]);
                        _frontLoad[2] = int.Parse(tokens[3]);
                    }
                    break;
            }
        }
        
        private void BuildSchedulingModel()
        {
            if (_periods.Count > 0)
                _numShift = _periods.Max(p => p.Shift) + 1;
        }
        
        public List<ExamClass> GetAllExamClasses() => _examClasses;
        public List<Room> GetRooms() => _rooms;
        public List<Room> GetSpareRooms() => new List<Room>();
        public List<Student> GetStudents() => _students;
        public List<Period> GetPeriods() => _periods;
        public InputDataType GetInputDataType() => InputDataType.ITC;
        
        // ISchedulingModel implementation
        public List<DateOnly> GetDates() => _dates;
        public int GetNumShift() => _numShift;
        public bool GetUseAnnealing() => _useAnnealing;
        public float GetOptimalPercentage() => _optimalPercentage;
        public bool GetUseExamClass() => _useExamClass;
        public int GetStartId() => _startId;
        
        public Dictionary<ExamClass, Dictionary<Period, int>> GetExamClassValidSlots()
        {
            var result = new Dictionary<ExamClass, Dictionary<Period, int>>();
            
            foreach (var exam in _examClasses)
            {
                var slots = new Dictionary<Period, int>();
                foreach (var period in _periods)
                {
                    int penalty = _periodWeights.ContainsKey(period) ? _periodWeights[period] : 0;
                    slots[period] = penalty;
                }
                result[exam] = slots;
            }
            
            return result;
        }
        
        public Dictionary<ExamClass, Dictionary<Room, int>> GetExamClassValidRooms()
        {
            var result = new Dictionary<ExamClass, Dictionary<Room, int>>();
            
            foreach (var exam in _examClasses)
            {
                result[exam] = new Dictionary<Room, int>();
                foreach (var room in _rooms)
                {
                    result[exam][room] = 0; // No room penalty for exam-room pairing specifically
                }
            }
            
            return result;
        }
        
        public Dictionary<Room, Dictionary<Period, int>> GetRoomValidSlots()
        {
            var result = new Dictionary<Room, Dictionary<Period, int>>();
            
            foreach (var room in _rooms)
            {
                result[room] = new Dictionary<Period, int>();
                int penalty = _roomPenalties.TryGetValue(room, out var p) ? p : 0;
                foreach (var period in _periods)
                {
                   result[room][period] = penalty; // Room penalty applies to all slots
                }
            }
            
            return result;
        }
        
        public Dictionary<Period, int> GetSlotPriority()
        {
            // Simply return the weights map
            return new Dictionary<Period, int>(_periodWeights);
        }
        
        public HardConstraints GetHardConstraints()
        {
            return new HardConstraints
            {
                HardConstraint_NoStudentConflict = true,
                HardConstraint_LimitedCapacity = true,
                HardConstraint_DifferentRoomForCourses = false // ITC allows same room for different exams
            };
        }

        public List<BinaryConstraint> GetBinaryConstraints() => _binaryConstraints;
        public List<UnaryConstraint> GetUnaryConstraints() => _unaryConstraints;
        
        // ITC 2007 specific properties exposed if needed
        public int TwoInARow => _twoInARow;
        public int TwoInADay => _twoInADay;
        public int PeriodSpread => _periodSpread;
        public int NonMixedDurations => _nonMixedDurations;
        public int[] FrontLoad => _frontLoad;
        public int FrontLoadThreshold => _frontLoadThreshold;
    }
}
