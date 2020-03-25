using System;
using System.Data;
using System.Data.OracleClient;

namespace ConsoleApplication2
{
    class Program
    {
        [Obsolete]
        static void Main(string[] args)
        {

            string conn = "DATA SOURCE=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=GPACS1)(PORT=1521)))(CONNECT_DATA=(SERVICE_NAME=spectra)));User ID=spectra;Password=artceps;";


            using (OracleConnection connection = new OracleConnection(conn))
            {

                connection.Open();
                OracleCommand command = new OracleCommand(@"SELECT STUDY.STUDY_KEY AS message_id,
                                STUDY.ACCESS_NO AS accession_number,
                                TO_CHAR(STUDY.STUDY_DTTM, 'YYYYMMDDhh24miss') AS study_date,
                                STUDY.STUDY_DESC AS study_description,
                                STUDY.PATIENT_ID AS patient_id,
                                STUDY.PATIENT_NAME AS last_name,
                                TO_CHAR(STUDY.PATIENT_BIRTH_DTTM, 'YYYYMMDD') AS birth_date,
                                STUDY.PATIENT_SEX AS sex,
                                /*MWLWL.REQUESTED_PROC_ID  AS requested_procedure_id,
                                MWLWL.REQUESTED_PROC_DESC AS requested_procedure_name,
                                MWLWL.REQUEST_DOCTOR AS last_name_request_doctor,
                                MWLWL.REFER_DOCTOR AS code_request_doctor,*/
                                '' AS requested_procedure_id,
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
                                /*INNER JOIN MWLWL
                                ON MWLWL.STUDY_INSTANCE_UID = STUDY.STUDY_INSTANCE_UID*/
                                INNER JOIN REPORT
                                ON REPORT.STUDY_KEY = STUDY.STUDY_KEY
                                INNER JOIN USERS
                                ON USERS.USER_KEY = REPORT.APPROVER_KEY
                                INNER JOIN MOVEREPORT
                                ON MOVEREPORT.STUDY_KEY = STUDY.STUDY_KEY
                                WHERE
                                ROWNUM < 500 AND STATUS = 0 AND SOURCE_AETITLE IN (:aetitle)", connection);
                OracleTransaction transaction;
                command.Parameters.Add(new OracleParameter("aetitle", "'DSVR5','DSVR9'"));
                command.CommandTimeout = 320;
                 // Start a local transaction

                 transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);
                command.Transaction = transaction;
               

                OracleDataReader dr = command.ExecuteReader();
                while (dr.Read())
                {
                    //byte[] byteArray = (Byte[])dr["REPORT_TEXT_BLOB"];
                    /*string filename = Path.Combine("c:\\teste", "teste.rtf");
                    using (FileStream fs = new FileStream(filename, FileMode.Create))
                    {
                        fs.Write(byteArray, 0, byteArray.Length);
                    }*/
                    Console.WriteLine(dr["patient_id"]);
                }
                transaction.Commit();
            }
        }
    }
}
