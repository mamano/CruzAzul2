using System;
using System.Configuration;
using Oracle.ManagedDataAccess.Client;
using GhelfondService;
using System.Linq;
using System.Data;
using System.Collections.Generic;
using System.Windows.Controls;
using System.IO;

namespace GhelfondService
{
    public class Service
    {
        private string QUERY = @"SELECT STUDY.STUDY_KEY AS message_id,
                                STUDY.ACCESS_NO AS accession_number,
                                TO_CHAR(STUDY.STUDY_DTTM, 'YYYYMMDDhh24miss') AS study_date,
                                STUDY.STUDY_DESC AS study_description,
                                STUDY.PATIENT_ID AS patient_id,
                                STUDY.PATIENT_NAME AS last_name,
                                TO_CHAR(STUDY.PATIENT_BIRTH_DTTM, 'YYYYMMDD') AS birth_date,
                                STUDY.PATIENT_SEX AS sex,
                                ''  AS requested_procedure_id,
                                '' AS requested_procedure_name,
                                '' AS last_name_request_doctor,
                                '' AS code_request_doctor,
                                USERS.FIRST_NAME AS first_name_read_doctor,
                                USERS.LAST_NAME AS last_name_read_doctor,
                                USERS.USER_SSN AS code_read_doctor,
                                TO_CHAR(REPORT.APPROVAL_DTTM, 'YYYYMMDDhh24miss') AS report_sign_datetime, 
                                REPORT.REPORT_TEXT_LOB AS report
                                FROM 
                                STUDY
                                INNER JOIN REPORT
                                ON REPORT.STUDY_KEY = STUDY.STUDY_KEY
                                INNER JOIN USERS 
                                ON USERS.USER_KEY = REPORT.APPROVER_KEY
                                INNER JOIN MOVEREPORT
                                ON MOVEREPORT.STUDY_KEY = STUDY.STUDY_KEY
                                WHERE
                                ROWNUM < 500 AND STATUS = 0 AND SOURCE_AETITLE IN ({0})";
       /* private string QUERY = @"SELECT {0} AS message_id,
                                        '454666' AS accession_number,
                                        '20200101120000' AS study_date,
                                        'ASDASD' AS study_description,
                                        '12345455' AS patient_id,
                                        'TESTE 'AS last_name,
                                        '19800101' AS birth_date,
                                        'M' AS sex,
                                        '666' AS requested_procedure_id,
                                        'GFGFGF' AS requested_procedure_name,
                                        'TESTERRR' AS last_name_request_doctor,
                                        '89898' AS code_request_doctor,
                                        'YUYUY' AS first_name_read_doctor,
                                        'VBVBVB' AS last_name_read_doctor,
                                        '4546666' AS code_read_doctor,
                                        '20200318120000' AS report_sign_datetime,
                                        TO_CLOB('TOMOGRAFIA COMPUTADORIZADA DO TÓRAX


Método:
Aquisição volumétrica, sem contraste.Exame realizado em caráter de urgência.

Indicação: Suspeita de pneumonia viral.


Análise:


Achados mais relevantes no contexto de urgência:


Opacidades pulmonares em vidro fosco, esparsas, em distribuição multifocal, bilateral, predominantemente periférica e posterior.  O aspecto é inespecifico, mas pode estar relacionado a processo infeccioso / inflamatório, sobretudo por agentes infecciosos virais.

Demais achados: 


Ausência de derrame pleural. 
Restante do parênquima pulmonar com atenuação preservada.
Traqueia e brônquios principais pérvios e com calibres conservados.Não há linfonodomegalias mediastinais.
Grandes vasos do mediastino de trajeto e calibre conservados. 
Arcabouço ósseo torácico sem particularidades.') AS report

                                   from dual
                                ";*/

        public void ReportSalvalus()
        {
            //string s = System.IO.File.ReadAllText("C:\\temp\\31_492721.rtf");
            //RichTextBox rtb = new RichTextBox();
            
            //TODO: Execute business logic from this method.
            //CacheManagerBusiness.Execute();
            var conn = ConfigurationManager.ConnectionStrings["InfinittOracle"].ConnectionString;
            var outxml = ConfigurationManager.AppSettings["OutReport"];
            var aetitle = String.Join(",", ConfigurationManager.AppSettings["AeTitles"].Split(','));
            var reports = new List<ReportModel>();
            using (OracleConnection connection = new OracleConnection(conn))
            {
                using (OracleCommand command = new OracleCommand(string.Format(QUERY, aetitle), connection))
                {
                    connection.Open();
                    var transaction = connection.BeginTransaction();
                    command.Transaction = transaction;
                    command.CommandTimeout = 320;
                    using (OracleDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            reports = reader.Cast<IDataRecord>().Select(r => ReportModel.Create(r)).ToList();
                        }
                    }
                    
                }
                foreach (var report in reports)
                {
                    var doc = Common.GenerateXMLString(report);
                    doc.Save(outxml + $"\\{report.accession_number}.xml");
                    Common.Save_RTF_file(outxml + $"\\{report.accession_number}.rtf", Common.BuildStringReport(report));
                    using (OracleCommand command = new OracleCommand())
                    {
                        command.CommandTimeout = 320;
                        command.Connection = connection;
                        command.CommandText = $"UPDATE MOVEREPORT SET STATUS = 1 WHERE STUDY_KEY = {report.message_id}";
                        command.ExecuteNonQuery();
                        command.Transaction.Commit();
                    }
                }

                connection.Close();
            }
        }

        public void Stop() { }
    }
}
