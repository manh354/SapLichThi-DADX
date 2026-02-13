using CsvHelper;
using CsvHelper.Configuration;
using SapLichThiCore.DataObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Globalization;
using SapLichThiCore.DataType;

namespace SapLichThiStream.Reader
{
    public class GeneralStreamCsvInput(string studentFilePath, string classFilePath) : IDataSource
    {
        // Backing sets for each output type
        private readonly HashSet<ExamClass> _examClasses = new();
        private readonly HashSet<Building> _buildings = new();
        private readonly HashSet<Room> _rooms = new();
        private readonly HashSet<Room> _spareRooms = new();
        private readonly HashSet<Student> _students = new();

        // Expose HashSets as read-only properties when callers need set semantics
        public HashSet<ExamClass> ExamClasses => _examClasses;
        public HashSet<Building> BuildingsSet => _buildings;
        public HashSet<Room> RoomsSet => _rooms;
        public HashSet<Room> SpareRoomsSet => _spareRooms;
        public HashSet<Student> StudentsSet => _students;

        // IDataSource implementations: convert sets to lists for compatibility
        public List<ExamClass> GetAllExamClasses()
        {
            return _examClasses.ToList();
        }

        public List<Building> GetBuildings()
        {
            return _buildings.ToList();
        }

        public InputDataType GetInputDataType()
        {
            return InputDataType.Toronto;
        }


        public List<Room> GetRooms()
        {
            if (_rooms.Count == 0)
                _rooms.Add(new Room("General", int.MaxValue, RoomType.large, new Building("General"), false, 0));
            return _rooms.ToList();
        }

        public List<Room> GetSpareRooms()
        {
            return _spareRooms.ToList();
        }


        public List<Student> GetStudents()
        {
            return _students.ToList();
        }                   

        public void LoadClasses()
        {
            if (string.IsNullOrWhiteSpace(classFilePath) || !File.Exists(classFilePath))
                return;

            // Detect header presence by peeking first line
            using var fs = File.OpenRead(classFilePath);
            using var peekReader = new System.IO.StreamReader(fs, leaveOpen: true);
            var firstLine = peekReader.ReadLine();
            bool hasHeader = false;
            if (firstLine != null)
            {
                var parts = firstLine.Split(',');
                if (parts.Length >= 2)
                {
                    // treat as header if second column is not an integer
                    if (!int.TryParse(parts[1].Trim(), NumberStyles.AllowThousands | NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
                    {
                        hasHeader = true;
                    }
                }
            }

            // reset stream
            fs.Seek(0, SeekOrigin.Begin);

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = hasHeader,
                MissingFieldFound = null,
                BadDataFound = null,
                IgnoreBlankLines = true,
                TrimOptions = TrimOptions.Trim
            };

            using var reader = new System.IO.StreamReader(fs);
            using var csv = new CsvReader(reader, config);

            if (hasHeader)
            {
                // ensure header is read
                csv.Read();
                csv.ReadHeader();
            }

            while (csv.Read())
            {
                string id;
                string countStr;
                try
                {
                    id = csv.GetField(0)?.Trim() ?? string.Empty;
                }
                catch
                {
                    continue;
                }

                if (string.IsNullOrEmpty(id))
                    continue;

                try
                {
                    countStr = csv.GetField(1)?.Trim() ?? "0";
                }
                catch
                {
                    countStr = "0";
                }

                if (!int.TryParse(countStr, NumberStyles.AllowThousands | NumberStyles.Integer, CultureInfo.InvariantCulture, out int count))
                {
                    var digits = new string(countStr.Where(char.IsDigit).ToArray());
                    if (!int.TryParse(digits, out count))
                        count = 0;
                }

                // skip if exam class already exists
                if (_examClasses.Any(x => string.Equals(x.Id, id, StringComparison.OrdinalIgnoreCase)))
                    continue;


                // create exam class
                var examClass = new ExamClass(id, count, 0 ,students: null);
                _examClasses.Add(examClass);
            }
        }

        public void LoadStudent()
        {
            if (string.IsNullOrWhiteSpace(studentFilePath) || !File.Exists(studentFilePath))
                return;

            // Detect header by peeking first line
            using var fs = File.OpenRead(studentFilePath);
            using var peekReader = new System.IO.StreamReader(fs, leaveOpen: true);
            var firstLine = peekReader.ReadLine();
            bool hasHeader = false;
            if (firstLine != null)
            {
                var parts = firstLine.Split(',');
                if (parts.Length >= 2)
                {
                    // treat as header if second column is not numeric (examclass id might be numeric but header is not)
                    if (!int.TryParse(parts[1].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
                    {
                        hasHeader = true;
                    }
                }
            }
            fs.Seek(0, SeekOrigin.Begin);

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = hasHeader,
                MissingFieldFound = null,
                BadDataFound = null,
                IgnoreBlankLines = true,
                TrimOptions = TrimOptions.Trim
            };

            using var reader = new System.IO.StreamReader(fs);
            using var csv = new CsvReader(reader, config);

            if (hasHeader)
            {
                csv.Read();
                csv.ReadHeader();
            }

            var examClassDict = _examClasses.ToDictionary(ec => ec.Id, ec => ec, StringComparer.OrdinalIgnoreCase);
            // build a lookup for existing students by id
            var studentDict = _students.ToDictionary(s => s.ID, s => s, StringComparer.OrdinalIgnoreCase);

            while (csv.Read())
            {
                string studentId = string.Empty;
                string examClassId = string.Empty;
                try
                {
                    studentId = csv.GetField(0)?.Trim() ?? string.Empty;
                }
                catch
                {
                    continue;
                }
                if (string.IsNullOrEmpty(studentId))
                    continue;

                try
                {
                    examClassId = csv.GetField(1)?.Trim() ?? string.Empty;
                }
                catch
                {
                    examClassId = string.Empty;
                }

                // reuse existing student if present, otherwise create and add to set
                if (!studentDict.TryGetValue(studentId, out var student))
                {
                    student = new Student(studentId);
                    _students.Add(student);
                    studentDict[studentId] = student;
                }

                if (string.IsNullOrEmpty(examClassId))
                    continue;

                // try to find exam class by provided id (support numeric and non-numeric ids)
                ExamClass? examClass = null;
                if (examClassId != null)
                {
                    if (int.TryParse(examClassId, NumberStyles.Integer, CultureInfo.InvariantCulture, out var numericId))
                    {
                        examClassDict.TryGetValue(numericId.ToString(), out examClass);
                    }

                    if (examClass == null)
                        examClassDict.TryGetValue(examClassId, out examClass);
                }

                if (examClass == null)
                {
                    // not found: skip association
                    continue;
                }

                if (examClass.Students == null)
                    examClass.Students = new List<Student>();

                // avoid duplicate references
                if (!examClass.Students.Any(s => string.Equals(s.ID, student.ID, StringComparison.OrdinalIgnoreCase)))
                {
                    examClass.Students.Add(student);
                }

            }

        }
    }
}
