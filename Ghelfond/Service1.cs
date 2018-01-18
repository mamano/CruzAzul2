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
using System.Net.Http;

namespace Ghelfond
{
    public partial class Service1 : ServiceBase
    {
        private Timer timer;
        private string command = @"SELECT * FROM STUDY')";

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
            string query = command;
            query = string.Format(query);
            try
            {

                string conn = ConfigurationManager.AppSettings["Connection"];

                using (OracleConnection connection = new OracleConnection(conn))
                {
                    OracleCommand command = new OracleCommand(query, connection);
                    connection.Open();
                    connection.BeginTransaction();
                    OracleDataReader reader = command.ExecuteReader();
                    try
                    {
                        while (reader.Read())
                        {
                            Console.WriteLine(reader.GetValue(0));
                        }
                        command.Transaction.Commit();
                        WriteLog("Sucesso " + DateTime.Now.ToString() + ": " + query);
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

        private static string CallWebAPI()
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:8888/");

            // Usage
            HttpResponseMessage response = await client.PostAsJsonAsync<list
            <product>>(serviceUrl + @"/PostProducts", products);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("ERROR:  Products Not Posted." + response.ReasonPhrase);
                return null;
            }
            return "";
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

        private class product
        {
        }
    }
}
