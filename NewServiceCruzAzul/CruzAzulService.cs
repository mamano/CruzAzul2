using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Configuration;
using System.Xml;
using System.Xml.Linq;
using System.Linq;

namespace NewServiceCruzAzul
{
    public class CruzAzulService : System.ServiceProcess.ServiceBase
    {
        FileSystemWatcher watcher;
        private string path;
        private string command = @"INSERT INTO MWLWL(MWL_KEY,
                                                     REPLICA_DTTM,
                                                     SCHEDULED_AETITLE,
                                                     CHARACTER_SET,
                                                     SCHEDULED_PROC_STATUS,
                                                     TRIGGER_DTTM,
                                                     SCHEDULED_DTTM,
                                                     SCHEDULED_PROC_ID,
                                                     REQUESTED_PROC_ID,
                                                     REQUESTED_PROC_DESC,
                                                     REQUESTED_PROC_CODES,
                                                     STUDY_INSTANCE_UID,
                                                     ACCESSION_NO,
                                                     REFER_DOCTOR,
                                                     PATIENT_ID,
                                                     PATIENT_NAME,
                                                     PATIENT_SEX,
                                                     PATIENT_BIRTH_DATE,
                                                     PATIENT_LOCATION,
                                                     SCHEDULED_MODALITY,
                                                     REQUEST_DEPARTMENT,
                                                     PROC_PLACER_ORDER_NO,
                                                     SCHEDULED_STATION,
                                                     SCHEDULED_LOCATION,
                                                     SCHEDULED_PROC_DESC,
                                                     SCHEDULED_ACTION_CODES,
                                                     REQUEST_DOCTOR) 
                                                    VALUES 
                                                    (SPECTRA.SQ_MWLWL.NEXTVAL, 
                                                    'ANY',
                                                    'ANY',
                                                    'ISO_IR 100',
                                                    '120',
                                                    '{0}',
                                                    '{1}',
                                                    '{2}',
                                                    '{3}',
                                                    '{4}',
                                                    '{5}',
                                                    '{6}',
                                                    '{7}',
                                                    '{8}',
                                                    '{9}',
                                                    '{10}',
                                                    '{11}',
                                                    '{12}',
                                                    '{13}',
                                                    '{14}',
                                                    '{15}',
                                                    '{16}',
                                                    '{17}',
                                                    '{18}',
                                                    '{19}',
                                                    '{20}',
                                                    '{21}')";

        public void Start()
        {
            WriteLog("Servico Iniciado: " + DateTime.Now.ToString());
            path = ConfigurationManager.AppSettings["Location"];
            watch();
            // write code here that runs when the Windows Service starts up.  
        }
        public void Stop()
        {
            WriteLog("Servico Parado: " + DateTime.Now.ToString());
            // write code here that runs when the Windows Service stops.  
        }

        private void watch()
        {
            watcher = new FileSystemWatcher();
            watcher.Path = path;
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
                                   | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            watcher.Filter = "*.xml";
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.Created += new FileSystemEventHandler(OnChanged);
            
            watcher.EnableRaisingEvents = true;
        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            //Copies file to another directory.

            if (e.Name.StartsWith("del"))
            {
                File.Move(e.FullPath, path + "\\NÃO PROCESSADOS\\" + e.Name);
            }
            else
            {
                using (StreamReader sr = File.OpenText(e.Name))
                {
                    XmlReaderSettings settings;
                    settings = new XmlReaderSettings();
                    settings.ConformanceLevel = ConformanceLevel.Document;


                    try
                    {
                        var xmlText = sr.ReadToEnd();
                        xmlText = xmlText.Replace("\0", string.Empty); // retira os caracteres nulos;
                        var document = new XmlDocument(); // cria o xml


                        XElement doc = XElement.Parse(xmlText);
                        var result = doc.Elements("MWL_ITEM")
                                        .Select(x => x)
                                        .ToList();
                        File.Delete(e.Name);
                    }
                    catch(Exception ex)
                    {

                    }
                }
            }
        }

        public void WriteLog(string excpetion)
        {
            StreamWriter log;
            FileStream fileStream = null;
            DirectoryInfo logDirInfo = null;
            FileInfo logFileInfo;

            string logFilePath = ConfigurationManager.AppSettings["ErrorLog"].ToString();
            logFilePath = logFilePath + "Log-" + System.DateTime.Today.ToString("MM-dd-yyyy") + "." + "txt";
            logFileInfo = new FileInfo(logFilePath);
            logDirInfo = new DirectoryInfo(logFileInfo.DirectoryName);
            if (!logDirInfo.Exists) logDirInfo.Create();
            if (!logFileInfo.Exists)
            {
                fileStream = logFileInfo.Create();
            }
            else
            {
                fileStream = new FileStream(logFilePath, FileMode.Append);
            }
            log = new StreamWriter(fileStream);
            log.WriteLine(excpetion);
            log.Close();
        }

        public string GetString(XmlNode node, int maxLength = 0, string def = "")
        {
            string column = string.Empty;
            if (node != null)
                column = node.InnerText;

            return column.Truncate(maxLength, def);
        }


        public string GetModality(string modality)
        {
            switch (modality)
            {
                case "RX":
                    modality = "CR";
                    break;
            }
            return modality;
        }
    }

    public static class StringExt
    {
        public static string Truncate(this string value, int maxLength, string def)
        {
            if (string.IsNullOrEmpty(value)) return def;
            if (maxLength == 0)
                return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
    }
}
