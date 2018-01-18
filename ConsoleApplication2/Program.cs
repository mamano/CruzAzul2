using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace ConsoleApplication2
{
    class Program
    {
        static void Main(string[] args)
        {

            string conn = ConfigurationManager.AppSettings["Connection"];


            using (OracleConnection connection = new OracleConnection(conn))
            {
                OracleCommand command = new OracleCommand("SELECT REPORT_TEXT_BLOB FROM Report", connection);
                connection.Open();
                connection.BeginTransaction();
                OracleDataReader dr = command.ExecuteReader();
                while (dr.Read())
                {
                    byte[] byteArray = (Byte[])dr["REPORT_TEXT_BLOB"];
                    string filename = Path.Combine("c:\\teste", "teste.rtf");
                    using (FileStream fs = new FileStream(filename, FileMode.Create))
                    {
                        fs.Write(byteArray, 0, byteArray.Length);
                    }

                }
            }
        }
    }
}
