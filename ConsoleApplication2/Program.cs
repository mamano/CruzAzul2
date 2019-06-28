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

            string conn = "DATA SOURCE=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=172.17.150.2)(PORT=1521)))(CONNECT_DATA=(SERVICE_NAME=spectra)));User ID=spectra;Password=artceps";


            using (OracleConnection connection = new OracleConnection(conn))
            {
                connection.Open();
                OracleCommand command = new OracleCommand("SELECT * FROM study WHERE ROWNUM < 10", connection);
                OracleTransaction transaction;

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
                    Console.WriteLine(dr["PATIENT_ID"]);
                }
                transaction.Commit();
            }
        }
    }
}
