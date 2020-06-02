using System;
using System.IO;
using System.Configuration;
using System.Xml.Linq;
using System.Linq;
using log4net;
using LogManager = log4net.LogManager;
using Oracle.ManagedDataAccess.Client;
using System.Xml;
using System.Text;

namespace NewServiceCruzAzul
{
    public class CruzAzulService : System.ServiceProcess.ServiceBase
    {

        private static ILog Log =
             LogManager.GetLogger(typeof(CruzAzulService));
        FileSystemWatcher watcher;
        private string path;
        private readonly string command = @"INSERT INTO MWLWL(MWL_KEY,
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
                                                    (SQ_MWLWL.NEXTVAL, 
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
            Log.Info("Servico Iniciado: " + DateTime.Now.ToString());
            path = ConfigurationManager.AppSettings["Location"];
            Watch();
            // write code here that runs when the Windows Service starts up.  
        }
        public new void Stop()
        {
            Log.Info("Servico Parado: " + DateTime.Now.ToString());
            // write code here that runs when the Windows Service stops.  
        }

        private void Watch()
        {
            watcher = new FileSystemWatcher
            {
                Path = path,
                NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
                Filter = "*.xml",
                EnableRaisingEvents = true,
                IncludeSubdirectories = false
            };
            //watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.Created += new FileSystemEventHandler(OnChanged);
        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            Process(e.Name, e.FullPath);
        }

        private void Process(string Name, string FullPath)
        { 
            var pathNotProcess = string.Empty;
            var pathProcess = string.Empty;
            Directory.GetAccessControl(path);
            //Copies file to another directory.
            if (Name.StartsWith("del"))
            {
                pathNotProcess = ConfigurationManager.AppSettings["NotProcess"].ToString();
                pathNotProcess = Path.Combine(path, pathNotProcess, Name);
                File.Move(FullPath, pathNotProcess);
                Log.Info($"Arquivo não processado '{Name}' : DataHora - '{DateTime.Now}'");
            }
            else
            {
                try
                {
                    
                    var delete = string.Empty;
                    var move = string.Empty;
                    using (StreamReader fileStream = new StreamReader(FullPath, Encoding.GetEncoding("iso-8859-1")))
                    {
                        XmlReaderSettings settings;
                        settings = new XmlReaderSettings();
                        settings.ConformanceLevel = ConformanceLevel.Document;
                        XDocument data = XDocument.Load(fileStream);
 
                        var conn = ConfigurationManager.ConnectionStrings["ConnectionString"].ToString();
                        var prefix = ConfigurationManager.AppSettings["Prefix"].ToString();
                        var institution = ConfigurationManager.AppSettings["Institution"].ToString();
                        var order = (from field in data.Elements("MWL_ITEM")
                                     select field).Select(field =>
                                     {
                                         var obj = new
                                         {
                                             DATE = GetString(field.Element("DATE")),
                                             TIME = GetString(field.Element("TIME")),
                                             REQUESTED_PROCEDURE_ID = GetString(field.Element("REQUESTED_PROCEDURE_ID")),
                                             REQUESTED_PROCEDURE_DESCRIPTION = GetString(field.Element("REQUESTED_PROCEDURE_DESCRIPTION"), maxLength: 54),
                                             STUDY_INSTANCE_UID = prefix + GetString(field.Element("ACCESSION_NUMBER")),
                                             ACCESSION_NUMBER = GetString(field.Element("ACCESSION_NUMBER")),
                                             REFERRING_PHYSICIAN_IDENTIFICATION = GetString(field.Element("REFERRING_PHYSICIAN_IDENTIFICATION")),
                                             PATIENT_ID = GetString(field.Element("PATIENT_ID")),
                                             PATIENT_NAME = GetString(field.Element("PATIENT_NAME")),
                                             PATIENT_SEX = GetString(field.Element("PATIENT_SEX")),
                                             PATIENT_BIRTHDATE = GetString(field.Element("PATIENT_BIRTHDATE")),
                                             PATIENT_LOCATION = GetString(field.Element("PATIENT_LOCATION")),
                                             MODALITY = GetModality(GetString(field.Element("MODALITY"))),
                                             INSTITUTION = institution,
                                             SCHEDULED_STATION_AE_TITLE = GetString(field.Element("SCHEDULED_STATION_AE_TITLE")),
                                             SCHEDULED_PERFORMING_PHYSICIAN = GetString(field.Element("SCHEDULED_PERFORMING_PHYSICIAN"), maxLength: 28)
                                         };
                                         return obj;
                                     }).FirstOrDefault();
                        var dttm = order.DATE + order.TIME;
                        var query = string.Format(command, dttm,
                                                           dttm,
                                                           order.REQUESTED_PROCEDURE_ID,
                                                           order.REQUESTED_PROCEDURE_ID,
                                                           order.REQUESTED_PROCEDURE_DESCRIPTION,
                                                           order.REQUESTED_PROCEDURE_ID,
                                                           order.STUDY_INSTANCE_UID,
                                                           order.ACCESSION_NUMBER,
                                                           order.REFERRING_PHYSICIAN_IDENTIFICATION,
                                                           order.PATIENT_ID,
                                                           order.PATIENT_NAME,
                                                           order.PATIENT_SEX,
                                                           order.PATIENT_BIRTHDATE,
                                                           order.PATIENT_LOCATION,
                                                           order.MODALITY,
                                                           order.INSTITUTION,
                                                           order.ACCESSION_NUMBER,
                                                           order.SCHEDULED_STATION_AE_TITLE,
                                                           order.SCHEDULED_PERFORMING_PHYSICIAN,
                                                           order.REQUESTED_PROCEDURE_DESCRIPTION,
                                                           order.ACCESSION_NUMBER,
                                                           order.REFERRING_PHYSICIAN_IDENTIFICATION);

                        using (OracleConnection connection = new OracleConnection(conn))
                        {
                            OracleCommand command = new OracleCommand(query, connection);
                            connection.Open();
                            connection.BeginTransaction();
                            try
                            {
                                command.ExecuteNonQuery();
                                command.Transaction.Commit();
                                
                                Log.Info($"Pedido cadastrado com sucesso {DateTime.Now.ToString()} : \r\n {query}");
                                delete = FullPath;
                                Log.Info($"Pedido pacienteid - {order.PATIENT_ID}, accessnumber - {order.ACCESSION_NUMBER} ");
                            }
                            catch (OracleException ex)
                            {
                                command.Transaction.Rollback();
                                pathNotProcess = ConfigurationManager.AppSettings["NotProcess"].ToString();
                                pathNotProcess = Path.Combine(path, pathNotProcess, Name);
                                move = FullPath;
                                Log.Info($"Erro {DateTime.Now.ToString()} source - {move} dest - {pathNotProcess} : {ex.Message} \r\n {ex.StackTrace} \r\n {ex.InnerException}");
                                Log.Info($"Query - {query}");
                            }
                            finally
                            {
                                // always call Close when done reading.
                                connection.Close();
                            }
                        }
                        
                        
                    }
                    if (!string.IsNullOrEmpty(move))
                        File.Move(move, pathNotProcess);
                    if (!string.IsNullOrEmpty(delete))
                        File.Delete(delete);
                    
                }
                catch (Exception ex)
                {
                    Log.Info($"Erro {DateTime.Now.ToString()} : {ex.Message} \r\n {ex.StackTrace} \r\n {ex.InnerException}");
                }

            }
        }

        public string GetString(XElement element, int maxLength = 0, string def = "")
        {
            var column = def;
            if (element != null && element.Value != string.Empty)
                column = element.Value;

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
