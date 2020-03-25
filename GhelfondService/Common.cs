using System;
using System.IO;
using System.Reflection.Metadata;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace GhelfondService
{
    public static class Common
    {
        //This Instances a new RichTextBox Control and uses it so save the Text
        public static void Save_RTF_file(string pFilePath, string pRTFText)
        {
            var iso = Encoding.GetEncoding("ISO-8859-1");
            try
            {
                using (FileStream fs = new FileStream(pFilePath, FileMode.OpenOrCreate))
                {
                    using (StreamWriter sw = new StreamWriter(fs, iso))
                    {
                        sw.Write(pRTFText);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static XmlDocument GenerateXMLString(ReportModel model)
        {
            var doc = new XmlDocument();
            var report = doc.CreateElement("report");
            doc.AppendChild(report);
            var node = doc.CreateElement("message_datetime"); 
            node.AppendChild(doc.CreateTextNode(DateTime.Now.ToString("yyyyMMddhhmmss")));
            report.AppendChild(node);
            node = doc.CreateElement("message_id");
            node.AppendChild(doc.CreateTextNode(model.message_id));
            report.AppendChild(node);
            node = doc.CreateElement("encoding");
            node.AppendChild(doc.CreateTextNode("UTF-8"));
            report.AppendChild(node);
            node = doc.CreateElement("version");
            node.AppendChild(doc.CreateTextNode("3.0.11.4"));
            report.AppendChild(node);
            node = doc.CreateElement("site_identifier");
            node.AppendChild(doc.CreateTextNode("01"));
            report.AppendChild(node);
            /******************order_details*********************/
            node = doc.CreateElement("order_details");
            report.AppendChild(node);
            var child = doc.CreateElement("accession_number");
            child.AppendChild(doc.CreateTextNode(model.accession_number));
            node.AppendChild(child);
            child = doc.CreateElement("study_date");
            child.AppendChild(doc.CreateTextNode(model.study_date));
            node.AppendChild(child);
            child = doc.CreateElement("study_description");
            child.AppendChild(doc.CreateTextNode(model.study_description));
            node.AppendChild(child);
            child = doc.CreateElement("requested_preocedure_id");
            child.AppendChild(doc.CreateTextNode(model.requested_procedure_id));
            node.AppendChild(child);
            child = doc.CreateElement("requested_preocedure_name");
            child.AppendChild(doc.CreateTextNode(model.requested_procedure_name));
            node.AppendChild(child);
            /******************patient_details*********************/
            node = doc.CreateElement("patient_details");
            report.AppendChild(node);
            child = doc.CreateElement("patient_id");
            child.AppendChild(doc.CreateTextNode(model.patient_id));
            node.AppendChild(child);
            child = doc.CreateElement("issuer_of_patient_id");
            node.AppendChild(child);
            child = doc.CreateElement("first_name");
            node.AppendChild(child);
            child = doc.CreateElement("middle_name");
            node.AppendChild(child);
            child = doc.CreateElement("last_name");
            child.AppendChild(doc.CreateTextNode(model.last_name));
            node.AppendChild(child);
            child = doc.CreateElement("birth_date");
            child.AppendChild(doc.CreateTextNode(model.birth_date));
            node.AppendChild(child);
            child = doc.CreateElement("sex");
            child.AppendChild(doc.CreateTextNode(model.sex));
            node.AppendChild(child);
            /******************referring_physician_details*********************/
            node = doc.CreateElement("referring_physician_details");
            report.AppendChild(node);
            child = doc.CreateElement("title");
            node.AppendChild(child);
            child = doc.CreateElement("first_name");
            child.AppendChild(doc.CreateTextNode(""));
            node.AppendChild(child);
            child = doc.CreateElement("middle_name");
            child.AppendChild(doc.CreateTextNode(""));
            node.AppendChild(child);
            child = doc.CreateElement("last_name");
            child.AppendChild(doc.CreateTextNode(model.last_name_request_doctor));
            node.AppendChild(child);
            child = doc.CreateElement("code");
            child.AppendChild(doc.CreateTextNode(model.code_request_doctor));
            node.AppendChild(child);
            /******************reading_physician_details*********************/
            node = doc.CreateElement("reading_physician_details");
            report.AppendChild(node);
            child = doc.CreateElement("first_name");
            child.AppendChild(doc.CreateTextNode(model.first_name_read_doctor));
            node.AppendChild(child);
            child = doc.CreateElement("middle_name");
            node.AppendChild(child);
            child = doc.CreateElement("last_name");
            child.AppendChild(doc.CreateTextNode(model.last_name_read_doctor));
            node.AppendChild(child);
            child = doc.CreateElement("title");
            node.AppendChild(child);
            child = doc.CreateElement("code");
            child.AppendChild(doc.CreateTextNode(model.code_read_doctor));
            node.AppendChild(child);
            /******************report_details*********************/
            node = doc.CreateElement("report_details");
            report.AppendChild(node);
            child = doc.CreateElement("document_name");
            child.AppendChild(doc.CreateTextNode(model.accession_number + ".rtf"));
            node.AppendChild(child);
            child = doc.CreateElement("document_type");
            child.AppendChild(doc.CreateTextNode("rtf"));
            node.AppendChild(child);
            child = doc.CreateElement("is_full_report");
            child.AppendChild(doc.CreateTextNode("Yes"));
            node.AppendChild(child);
            child = doc.CreateElement("internal_report_version");
            node.AppendChild(child);
            child = doc.CreateElement("method");
            child.AppendChild(doc.CreateTextNode("Insert"));
            node.AppendChild(child);
            child = doc.CreateElement("report_sign_datetime");
            child.AppendChild(doc.CreateTextNode(model.report_sign_datetime));
            node.AppendChild(child);

            return doc;
        }

        public static string PlainTextToRtf(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return "";

            string escapedPlainText = plainText.Replace(@"\", @"\\").Replace("{", @"\{").Replace("}", @"\}");
            //escapedPlainText = EncodeCharacters(escapedPlainText);

            var rtf = escapedPlainText.Replace(Environment.NewLine, "\\par\r\n ");
            
            return rtf;
        }

        private static string EncodeCharacters(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "";

            return text
                .Replace("ą", @"\'b9")
                .Replace("ć", @"\'e6")
                .Replace("ę", @"\'ea")
                .Replace("ł", @"\'b3")
                .Replace("ń", @"\'f1")
                .Replace("ó", @"\'f3")
                .Replace("ś", @"\'9c")
                .Replace("ź", @"\'9f")
                .Replace("ż", @"\'bf")
                .Replace("Ą", @"\'a5")
                .Replace("Ć", @"\'c6")
                .Replace("Ę", @"\'ca")
                .Replace("Ł", @"\'a3")
                .Replace("Ń", @"\'d1")
                .Replace("Ó", @"\'d3")
                .Replace("Ś", @"\'8c")
                .Replace("Ź", @"\'8f")
                .Replace("Ż", @"\'af");
        }

        public static string BuildStringReport(ReportModel model)
        {
            var report = @"{\rtf1\ansi\ansicpg949\deff0\nouicompat\deflang1046\deflangfe1046{\fonttbl{\f0\fswiss\fprq2\fcharset0 Arial;}{\f1\fnil\fcharset0 Calibri;}}
                            {\colortbl ;\red0\green0\blue0;}
                            {\*\generator Riched20 10.0.14393}{\*\mmathPr\mnaryLim0\mdispDef1\mwrapIndent1440 }\viewkind4\uc1 
                            
                            \par
                            \par
                            \pard\widctlpar\tx1134\tx2268\tx3402\tx4536\tx5670\tx12474\tx13608\cf0\b\fs28 ";
            report += model.requested_procedure_name;
            report += @" \fs24\par

                            \pard\widctlpar\qj\tx1134\tx2268\tx3402\tx4536\tx5670\tx6804\tx7938\tx9072\tx10206\tx11340\tx12474\tx13608\tx14742\tx15876\b0\par
                            \line T\'c9CNICA:\par ";
            report += model.study_description;
            report += @"\par
                            \par
                            \line AN\'c1LISE:\par ";
            report += PlainTextToRtf(model.report);
            report += @" \par
                            \pard\widctlpar\qj\cf1\par
                            \fs22\par }";

            return report;
        }
    }
}
