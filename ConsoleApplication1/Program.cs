using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;

using System.Xml;
using System.Xml.Linq;
using System.Configuration;
using System.Data.OleDb;
using Oracle.ManagedDataAccess.Client;

namespace ConsoleApplication1
{
    class Program
    {
        /* const string command = @"INSERT INTO MWLWL(MWL_KEY,TRIGGER_DTTM,SCHEDULED_AETITLE,SCHEDULED_DTTM,
                                               SCHEDULED_PROC_ID,SCHEDULED_PROC_DESC,
                                               REQUESTED_PROC_ID,REQUESTED_PROC_DESC,
                                               REQUESTED_PROC_CODES,REQUEST_DEPARTMENT,
                                               STUDY_INSTANCE_UID,ACCESSION_NO,REFER_DOCTOR,
                                               SCHEDULED_LOCATION,REQUEST_DOCTOR,
                                               PATIENT_ID,PATIENT_NAME,PATIENT_SEX,
                                               PATIENT_BIRTH_DATE,PATIENT_LOCATION,
                                               SCHEDULED_MODALITY) VALUES 
                                               (SPECTRA.SQ_MWLWL.NEXTVAL, '{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}',
                                                '{8}','{9}','{10}','{11}','{12}','{13}','{14}','{15}',
                                                '{16}','{17}','{18}','{19}')";                        */
        const string command = @"INSERT INTO MWLWL(MWL_KEY,
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
                                                     SCHEDULED_MODALITY) 
                                                    VALUES 
                                                    (123, 
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
        static void Main(string[] args)
        {
            timer1_Tick();
        }
        private static void timer1_Tick()
        {
            string path = @"C:\Temp2";
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

                        query = string.Format(query, dttm,
                                  dttm,
                                 GetString(document.SelectNodes("/MWL_ITEM/REQUESTED_PROCEDURE_ID")[0]),
                                 GetString(document.SelectNodes("/MWL_ITEM/REQUESTED_PROCEDURE_ID")[0]),
                                 GetString(document.SelectNodes("/MWL_ITEM/REQUESTED_PROCEDURE_DESCRIPTION")[0]),
                                 GetString(document.SelectNodes("/MWL_ITEM/REQUESTED_PROCEDURE_ID")[0]),
                                  "1.2.410.2000010.82.121.300860727." + GetString(document.SelectNodes("/MWL_ITEM/ACCESSION_NUMBER")[0]),
                                 GetString(document.SelectNodes("/MWL_ITEM/ACCESSION_NUMBER")[0]),
                                 GetString(document.SelectNodes("/MWL_ITEM/REFERRING_PHYSICIAN_IDENTIFICATION")[0]),
                                 GetString(document.SelectNodes("/MWL_ITEM/PATIENT_ID")[0]),
                                 GetString(document.SelectNodes("/MWL_ITEM/PATIENT_NAME")[0]),
                                 GetString(document.SelectNodes("/MWL_ITEM/PATIENT_SEX")[0]),
                                 GetString(document.SelectNodes("/MWL_ITEM/PATIENT_BIRTHDATE")[0]),
                                 GetString(document.SelectNodes("/MWL_ITEM/PATIENT_LOCATION")[0]),
                                 GetString(document.SelectNodes("/MWL_ITEM/MODALITY")[0]));
                        /*query = string.Format(query, dttm,
                                                     GetString(document.SelectNodes("/MWL_ITEM/SCHEDULED_STATION_AE_TITLE")[0]),
                                                        dttm,
                                                        GetString(document.SelectNodes("/MWL_ITEM/REQUESTED_PROCEDURE_ID")[0]),
                                                        GetString(document.SelectNodes("/MWL_ITEM/REQUESTED_PROCEDURE_DESCRIPTION")[0]),
                                                        GetString(document.SelectNodes("/MWL_ITEM/REQUESTED_PROCEDURE_ID")[0]),
                                                        GetString(document.SelectNodes("/MWL_ITEM/REQUESTED_PROCEDURE_DESCRIPTION")[0]),
                                                        GetString(document.SelectNodes("/MWL_ITEM/REQUESTED_PROCEDURE_ID")[0]),
                                                        "",
                                                        "1.2.410.2000010.82.121.300860727." + GetString(document.SelectNodes("/MWL_ITEM/ACCESSION_NUMBER")[0]),
                                                        GetString(document.SelectNodes("/MWL_ITEM/ACCESSION_NUMBER")[0]),
                                                        GetString(document.SelectNodes("/MWL_ITEM/REFERRING_PHYSICIAN_IDENTIFICATION")[0]),
                                                        GetString(document.SelectNodes("/MWL_ITEM/SCHEDULED_PERFORMING_PHYSICIAN")[0]),
                                                        GetString(document.SelectNodes("/MWL_ITEM/REFERRING_PHYSICIAN")[0]),
                                                        GetString(document.SelectNodes("/MWL_ITEM/PATIENT_ID")[0]),
                                                        GetString(document.SelectNodes("/MWL_ITEM/PATIENT_NAME")[0]),
                                                        GetString(document.SelectNodes("/MWL_ITEM/PATIENT_SEX")[0]),
                                                        GetString(document.SelectNodes("/MWL_ITEM/PATIENT_BIRTHDATE")[0]),
                                                        GetString(document.SelectNodes("/MWL_ITEM/PATIENT_LOCATION")[0]),
                                                        GetString(document.SelectNodes("/MWL_ITEM/MODALITY")[0]));  */
                        try
                        {

                            string constr = "DATA SOURCE=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521)))(CONNECT_DATA=(SERVICE_NAME=XE)));User ID=system;Password=noturno";
                            //string constr = "user id=spectra;password=artceps;data source=Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=209.208.26.124)(PORT=1521))(CONNECT_DATA=(SID=spectra)))";
                            //Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=209.208.26.124)(PORT=1521))(CONNECT_DATA=(SID=spectra)))
                            //";User Id=spectra;Password=artceps"
                            OracleConnection connection2 = new OracleConnection();
                            using (OracleConnection connection = new OracleConnection(constr))
                            {
                                OracleCommand command = new OracleCommand(query, connection);
                                connection.Open();
                            
                                try
                                {
                                    command.ExecuteNonQuery();
                                }
                                catch(OracleException ex)
                                {
                                    StreamWriter vWriter = new StreamWriter(@"c:\temp3\testeServico.txt", true);


                                    vWriter.WriteLine("Servico Rodando: " + ex.Message + ex.StackTrace);
                                    vWriter.Flush();
                                    vWriter.Close();
                                }
                                finally
                                {
                                    // always call Close when done reading.
                                    reader.Close();
                                    connection.Close();
                                }
                            }
                        }
                        catch (OracleException ex)
                        {
                            StreamWriter vWriter = new StreamWriter(@"c:\temp3\testeServico.txt", true);


                            vWriter.WriteLine("Servico Rodando: " + ex.Message + ex.StackTrace + ex.InnerException);
                            vWriter.Flush();
                            vWriter.Close();
                        }
                        catch (Exception ex)
                        {
                            StreamWriter vWriter = new StreamWriter(@"c:\temp3\testeServico.txt", true);


                            vWriter.WriteLine("Servico Rodando: " + ex.Message + ex.StackTrace+ ex.InnerException);
                            vWriter.Flush();
                            vWriter.Close();
                        }
                    }
                }
            }
        }

        public static string GetString(XmlNode node)
        {
            string column = string.Empty;
            if (node != null)
                column = node.InnerText; 
            return column;
        }
    }
}

