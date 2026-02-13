using SapLichThiLib.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using System.Globalization;
using SapLichThiLib.DataObjects;
using SapLichThiLib.ErrorAndLog;

namespace SapLichThiLib.ByteArrayWriter
{
    public class ScheduleOutput : IByteArrayOutput
    {
        public ExamSchedule I_schedule { get; set; }

        public byte[] OutputByteArray()
        {
            var dateLength = I_schedule.dates.Count();
            var shiftLength = I_schedule.shifts.Count();
            var roomLength = I_schedule.rooms.Count();

            using var buffer = new MemoryStream();
            using (var writer = new StreamWriter(buffer))
            using (var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csvWriter.WriteField("STT");
                csvWriter.WriteField("Trường/Viện");
                csvWriter.WriteField("Mã lớp");
                csvWriter.WriteField("Mã HP");
                csvWriter.WriteField("Tên học phần");
                csvWriter.WriteField("Ghi chú");
                csvWriter.WriteField("Nhóm");
                csvWriter.WriteField("Ngày thi");
                csvWriter.WriteField("Kíp thi");
                csvWriter.WriteField("SLĐK");
                csvWriter.WriteField("Phòng thi");
                csvWriter.WriteField("Mã lớp thi");
                csvWriter.NextRecord();

                int id = 1;
                for (int date = 0; date < dateLength; date++)
                {
                    for (int shift = 0; shift < shiftLength; shift++)
                    {
                        for (int room = 0; room < roomLength; room++)
                        {
                            var thisCell = I_schedule.GetCell(date, shift, room);
                            if (thisCell.IsEmpty())
                                continue;
                            int examClassId = 1;
                            Logger.logger.LogMessage($"Số lượng examClasses của cell = {thisCell.ExamClasses.Count}");
                            foreach (var examClass in thisCell.ExamClasses)
                            {
                                csvWriter.WriteField(id);
                                csvWriter.WriteField(examClass.StudyClass.Course.School.Name);
                                csvWriter.WriteField(examClass.StudyClass.ID);
                                csvWriter.WriteField(examClass.StudyClass.Course.ID);
                                csvWriter.WriteField(examClass.StudyClass.Course.Name);
                                csvWriter.WriteField(examClass.StudyClass.Description);
                                csvWriter.WriteField(examClass.Description);
                                csvWriter.WriteField(I_schedule.dates[date].ToString());
                                csvWriter.WriteField($"Kíp {shift + 1}");
                                csvWriter.WriteField(examClass.Count);
                                csvWriter.WriteField(I_schedule.rooms[room].RoomId);
                                csvWriter.WriteField(examClass.ID);
                                csvWriter.NextRecord();
                                id++;
                            }
                        }
                    }
                }
            }
            return buffer.ToArray();
        }
    }
}
