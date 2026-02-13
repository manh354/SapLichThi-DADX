using CsvHelper;
using SapLichThiLib.ByteArrayWriter;
using SapLichThiLib.DataStructures;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiLib.FileWriter
{
    public class EmptyCellDetailsOutput : IByteArrayOutput
    {
        public ExamSchedule I_schedule { get; set; }


        public byte[] OutputByteArray()
        {
            using (var memStream = new MemoryStream())
            using (var writer = new StreamWriter(memStream))
            using (var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csvWriter.WriteField("Ngày");
                csvWriter.WriteField("Kíp");
                csvWriter.WriteField("Phòng");
                csvWriter.WriteField("Loại phòng");
                csvWriter.WriteField("Kích cỡ phòng");
                csvWriter.NextRecord();
                var dates = I_schedule.dates;
                var shifts = I_schedule.shifts;
                var rooms = I_schedule.rooms;
                for (int date = 0; date < dates.Length; date++)

                    for (var shift = 0; shift < shifts.Length; shift++)

                        for (int room = 0; room < rooms.Length; room++)
                        {
                            ArrayCell cell = I_schedule.GetCell(date, shift, room)!;
                            if (cell == null)
                            {
                                continue;
                            }
                            if (cell.ExamClasses == null)
                            {
                                continue;
                            }
                            if (cell.ExamClasses.Count > 0)
                            {
                                continue;
                            }
                            csvWriter.WriteField(dates[date]);
                            csvWriter.WriteField(shifts[shift]);
                            var roomexport = rooms[room];
                            csvWriter.WriteField(roomexport.RoomId);
                            csvWriter.WriteField(roomexport.RoomType == DataObjects.RoomType.large ? "L" : roomexport.RoomType == DataObjects.RoomType.medium ? "M" : "S");
                            csvWriter.WriteField(roomexport.Capacity);
                            csvWriter.NextRecord();

                        }
                return memStream.ToArray();
            }

        }
    }

    public class EmptyCellTotalOutput : IByteArrayOutput
    {
        public ExamSchedule I_schedule { get; set; }

        public byte[] OutputByteArray()
        {
            using (var memStream = new MemoryStream())
            using (var writer = new StreamWriter(memStream))
            using (var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csvWriter.WriteField("Ngày");
                csvWriter.WriteField("Kíp");
                csvWriter.WriteField("SL phòng nhỏ trống");
                csvWriter.WriteField("SL phòng vừa trống");
                csvWriter.WriteField("SL phòng lớn trống");
                csvWriter.WriteField("Tổng cộng");
                csvWriter.NextRecord();
                var dates = I_schedule.dates;
                var shifts = I_schedule.shifts;
                var rooms = I_schedule.rooms;
                for (int date = 0; date < dates.Length; date++)

                    for (var shift = 0; shift < shifts.Length; shift++)

                    {
                        int l = 0;
                        int m = 0;
                        int s = 0;
                        csvWriter.WriteField(dates[date]);
                        csvWriter.WriteField(shifts[shift]);
                        for (int room = 0; room < rooms.Length; room++)
                        {
                            ArrayCell cell = I_schedule.GetCell(date, shift, room);
                            if (cell == null)
                            {
                                continue;
                            }
                            if (cell.ExamClasses == null)
                            {
                                continue;
                            }
                            if (cell.ExamClasses.Count > 0)
                            {
                                continue;
                            }
                            var roomexport = rooms[room];
                            switch (roomexport.RoomType)
                            {
                                case DataObjects.RoomType.small: s += 1; break;
                                case DataObjects.RoomType.medium: m += 1; break;
                                case DataObjects.RoomType.large: l += 1; break;
                                default: throw new Exception("Weird room detected");
                            }

                        }
                        csvWriter.WriteField(s);
                        csvWriter.WriteField(m);
                        csvWriter.WriteField(l);
                        csvWriter.WriteField(s + m + l);
                        csvWriter.NextRecord();
                    }

                return memStream.ToArray();
            }

        }
    }
}
