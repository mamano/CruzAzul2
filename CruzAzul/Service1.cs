using System;
using System.IO;
using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Configuration;
using Oracle.ManagedDataAccess.Client;

namespace CruzAzul
{
    public partial class Service1 : ServiceBase
    {
        private Timer timer;
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
                                                     REQUEST_DEPARTAMENT,
                                                     PROC_PRACER_ORDER_NO) 
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
                                                    '{14}')";

        public Service1()
        {
            InitializeComponent();
        }


        protected override void OnStart(string[] args)
        {
            Thread t = new Thread(new ThreadStart(this.InitTimer));
            t.Start();
        }

        private void InitTimer()
        {
            DateTime now = DateTime.Now;
            int time = Int32.Parse(ConfigurationManager.AppSettings["Timer"].ToString());
            timer = new Timer(time);
            timer.Elapsed += timer1_Tick;
            timer.Enabled = true;                             
            timer.Start();
            WriteLog("Servico Iniciado: " + DateTime.Now.ToString());
        }

        protected override void OnStop()
        {
            timer.Enabled = false;
            WriteLog("Servico Parado: " + DateTime.Now.ToString());
        }


        private void timer1_Tick(object sender, ElapsedEventArgs e)
        {
            string path = ConfigurationManager.AppSettings["Location"];
            Dictionary<string, string> dic = new Dictionary<string, string>();
            if (Directory.Exists(path))
            {
                string query = command;
                string[] fileEntries = Directory.GetFiles(path);
                foreach (string fileName in fileEntries)
                {
                    using (FileStream fileSteam = File.OpenRead(fileName))
                    {
                        XmlReaderSettings settings;

                        settings = new XmlReaderSettings();
                        settings.ConformanceLevel = ConformanceLevel.Document;

                        XmlReader reader = XmlReader.Create(fileSteam, settings);
                        XmlDocument document = new XmlDocument();
                        document.Load(reader);

                        string dttm = GetString(document.SelectNodes("/MWL_ITEM/DATE")[0]) +
                                       GetString(document.SelectNodes("/MWL_ITEM/TIME")[0]);
                        query = string.Format(query,    dttm,
                                                        dttm,
                                                       GetString(document.SelectNodes("/MWL_ITEM/REQUESTED_PROCEDURE_ID")[0]),
                                                       GetString(document.SelectNodes("/MWL_ITEM/REQUESTED_PROCEDURE_ID")[0]),
                                                       GetString(document.SelectNodes("/MWL_ITEM/REQUESTED_PROCEDURE_DESCRIPTION")[0]),
                                                       GetString(document.SelectNodes("/MWL_ITEM/REQUESTED_PROCEDURE_ID")[0]),
                                                       ConfigurationManager.AppSettings["AccessNumber"] + GetString(document.SelectNodes("/MWL_ITEM/ACCESSION_NUMBER")[0]),
                                                       GetString(document.SelectNodes("/MWL_ITEM/ACCESSION_NUMBER")[0]),
                                                       GetString(document.SelectNodes("/MWL_ITEM/REFERRING_PHYSICIAN_IDENTIFICATION")[0]),
                                                       GetString(document.SelectNodes("/MWL_ITEM/PATIENT_ID")[0]),
                                                       GetString(document.SelectNodes("/MWL_ITEM/PATIENT_NAME")[0]),
                                                       GetString(document.SelectNodes("/MWL_ITEM/PATIENT_SEX")[0]),
                                                       GetString(document.SelectNodes("/MWL_ITEM/PATIENT_BIRTHDATE")[0]),
                                                       GetString(document.SelectNodes("/MWL_ITEM/PATIENT_LOCATION")[0]),
                                                       GetString(document.SelectNodes("/MWL_ITEM/MODALITY")[0]));
                        try
                        {

                            string conn = ConfigurationManager.AppSettings["Connection"];

                            OracleConnection connection2 = new OracleConnection();
                            using (OracleConnection connection = new OracleConnection(conn))
                            {
                                OracleCommand command = new OracleCommand(query, connection);
                                connection.Open();
                                connection.BeginTransaction();
                                try
                                {
                                    command.ExecuteNonQuery();
                                    command.Transaction.Commit();
                                    WriteLog("Sucesso " + DateTime.Now.ToString() + ": " + query);
                                    dic.Add(fileName, fileName);
                                }
                                catch (OracleException ex)
                                {
                                    command.Transaction.Rollback();
                                    WriteLog("Erro " + DateTime.Now.ToString() + ": " + ex.Message + "\r\n" + ex.StackTrace + "\r\n" + ex.InnerException);
                                }
                                finally
                                {   
                                    // always call Close when done reading.
                                    reader.Close();
                                    connection.Close();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            WriteLog("Erro " + DateTime.Now.ToString() + ": " + ex.Message + "\r\n" + ex.StackTrace + "\r\n" + ex.InnerException);
                        }
                    }
                }

                foreach (KeyValuePair<string, string> entry in dic)
                {
                    File.Delete(entry.Value);
                }
            }
        }

        public static void WriteLog(string excpetion)
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

        public static string GetString(XmlNode node)
        {
            string column = string.Empty;
            if (node != null)
                column = node.InnerText;
            return column;
        }

        /*StreamWriter vWriter = new StreamWriter(@"c:\testeServico.txt", true);


        vWriter.WriteLine("Servico Rodando: " + DateTime.Now.ToString());
        vWriter.Flush();
        vWriter.Close();

        while (reader.Read())
                    {
                        switch (reader.NodeType)
                        {
                            case XmlNodeType.Element:
                                XElement el = XElement.ReadFrom(reader) as XElement;
                                switch (reader.Name)
                                {

                                        break;
                                }
                                break;
                        }
                    }
        }     */
    }
}
