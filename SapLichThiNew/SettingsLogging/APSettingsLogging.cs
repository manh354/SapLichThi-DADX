using Newtonsoft.Json;
using SapLichThiAlgorithm.AlgorithmsObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SapLichThiNew.SettingsLogging
{
    public class APSettingsLogging : AlgoProcess
    {
        protected override void BuildSubProcesses()
        {
            return;
        }

        public override string GetProcessName()
        {
            return "Lưu dữ liệu cài đặt.";
        }

        protected override ConsoleLogMessages? CreateConsoleLog(AlgorithmContext context)
        {
            return null;
        }

        protected override InputRequestEventArgs? CreateInputRequestFromContext(AlgorithmContext context)
        {
            string json = context.SerializeSettingsAsJson();
            XmlDocument doc = JsonConvert.DeserializeXmlNode(json, "root");
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,              // Enable indentation
                IndentChars = "  ",         // Use 2 spaces for indentation
                NewLineChars = "\r\n",      // Line breaks
                NewLineHandling = NewLineHandling.Replace
            };
            string xml;
            // Write formatted XML to a string
            using (StringWriter stringWriter = new StringWriter())
            using (XmlWriter xmlWriter = XmlWriter.Create(stringWriter, settings))
            {
                doc.Save(xmlWriter);
                xml = stringWriter.ToString();
            }
            return new() { InputQAndAs = [new APLoggingQAndA()
            {
                ContextConvertedJson = json,
                ContextConvertedXml = xml,
            }] };
        }

    }

    public class APLoggingQAndA : BaseInputQAndA
    {
        public string ContextConvertedJson { get; set; }
        public string ContextConvertedXml { get; set; }
    }
}
