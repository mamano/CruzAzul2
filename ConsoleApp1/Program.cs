using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Configuration;

namespace ConsoleApp1
{
    public class Program
    {
        public void Main(string[] args)
        {
            timer1_Tick();
        }
        private void timer1_Tick()
        {
            string path = @"C:\Users\Marcel\Documents\Visual Studio 2015\Projects\CruzAzul\CruzAzul\";
            if (File.Exists(path))
            {
                string query = command;
                string[] fileEntries = Directory.GetFiles(path);
                foreach (string fileName in fileEntries)
                {
                    using (XmlReader reader = XmlReader.Create(path + fileName))
                    {
                        reader.MoveToContent();
                        string dttm = GetString(reader["DATE"]) + GetString(reader["TIME"]);
                        query = string.Format(query, dttm,
                                                     GetString(reader["SCHEDULED_STATION_AE_TITLE"]),
                                                     dttm,
                                                     GetString(reader["REQUESTED_PROCEDURE_ID"]),
                                                     GetString(reader["REQUESTED_PROCEDURE_DESCRIPTION"]),
                                                     GetString(reader["REQUESTED_PROCEDURE_ID"]),
                                                     GetString(reader["REQUESTED_PROCEDURE_DESCRIPTION"]),
                                                     GetString(reader["REQUESTED_PROCEDURE_ID"]),
                                                     GetString(reader["REQUESTING_DEPARTMENT"]),
                                                     "1.2.410.2000010.82.121.300860727." + GetString(reader["ACCESSION_NUMBER"]),
                                                     GetString(reader["ACCESSION_NUMBER"]),
                                                     GetString(reader["REFERRING_PHYSICIAN_IDENTIFICATION"]),
                                                     GetString(reader["SCHEDULED_PERFORMING_PHYSICIAN"]),
                                                     GetString(reader["REFERRING_PHYSICIAN"]),
                                                     GetString(reader["PATIENT_ID"]),
                                                     GetString(reader["PATIENT_NAME"]),
                                                     GetString(reader["PATIENT_SEX"]),
                                                     GetString(reader["PATIENT_BIRTHDATE"]),
                                                     GetString(reader["PATIENT_LOCATION"]),
                                                     GetString(reader["MODALITY"]));
                        reader.Close();

                    }
                }
            }
        }

        public string GetString(string column)
        {
            if (string.IsNullOrEmpty(column))
                column = "";
            return column;
        }
    }
}
