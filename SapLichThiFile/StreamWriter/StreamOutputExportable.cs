using SapLichThiCore.ExportableObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using System.Globalization;

namespace SapLichThiStream.StreamWriter
{
    public class StreamOutputExportable
    {
        public IEnumerable<IExportableObject> I_exportables { get; set; }
        public CultureInfo I_cultureInfo { get; set; }
        Stream I_stream { get; set; }
        public StreamOutputExportable(IEnumerable<IExportableObject> exportables, Stream stream, CultureInfo cultureInfo)
        {
            I_exportables = exportables;
            I_cultureInfo = cultureInfo;
            I_stream = stream;
        }
        public void OutputStream()
        {
            using(var streamWriter = new System.IO.StreamWriter(I_stream))
            using (CsvWriter csvWriter = new CsvWriter(streamWriter, I_cultureInfo))
            {
                if (!I_exportables.Any())
                {
                    csvWriter.WriteField("Không tồn tại xung đột trong quá trình test.");
                    return;
                }
                foreach (var headerElem in I_exportables.First().GetHeaders())
                {
                    csvWriter.WriteField(headerElem);
                }
                csvWriter.NextRecord();
                foreach (var exportable in I_exportables)
                {
                    foreach (var bodyElem in exportable.GetValuesAsString())
                    {
                        foreach (var field in bodyElem)
                        {
                            csvWriter.WriteField(field ?? string.Empty);
                        }
                        csvWriter.NextRecord();
                    }
                }
                csvWriter.Flush();
                streamWriter.Flush();
            }
        }
    }
}
