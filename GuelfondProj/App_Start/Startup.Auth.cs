using System;
using Hangfire;
using Hangfire.SQLite;
using System.Web;
using System.Diagnostics;
using System.Configuration;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.IO;
using Owin;
using System.Net.Http;
using Newtonsoft.Json;
using Oracle.ManagedDataAccess.Types;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Text;
using System.Text.RegularExpressions;

namespace GuelfondProj
{
    public partial class Startup
    {

        
        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
                var name = String.Format("{0}.{1}",
                                        Environment.MachineName,
                                        Guid.NewGuid().ToString());
                var options = new SQLiteStorageOptions();
                GlobalConfiguration.Configuration.UseSQLiteStorage("SQLiteHangfire", options);
                var option = new BackgroundJobServerOptions
                {
                    ServerName = name,
                    WorkerCount = 1,
                    SchedulePollingInterval = TimeSpan.FromMinutes(1)
                };
            

                RecurringJob.AddOrUpdate("run", () => Run(), "* * * * *");  
                //RecurringJob.AddOrUpdate("delete", () => Delete(), "0 0 */10 * *");


                app.UseHangfireDashboard();
                app.UseHangfireServer(option);
        }

        public string Delete()
        {
            var error = string.Empty;

            try
            {
                string conn = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

                using (OracleConnection connection = new OracleConnection(conn))
                {
                    OracleCommand command = new OracleCommand("SELECT * FROM MOVEPDF WHERE STATUS = 1", connection);
                    connection.Open();
                    connection.BeginTransaction();

                    try
                    {
                        var da = new OracleDataAdapter(command);
                        var cb = new OracleCommandBuilder(da);
                        var ds = new DataSet();
                        da.Fill(ds);
                        
                        //var sourcePath = ConfigurationManager.AppSettings["SourcePathPDF"];
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {

                            //var sourceFilePath = sourcePath + "\\" + dr["PATHNAME"].ToString() + "\\" + dr["FILENAME"].ToString();
                            try
                            {
                                /*FileInfo f1 = new FileInfo(sourceFilePath);
                                f1.Delete();
                                Directory.Delete(sourcePath + "\\" + dr["PATHNAME"].ToString());*/
                                command = new OracleCommand("DELETE FROM MOVEPDF WHERE STATUS = 1", connection);
                                command.ExecuteNonQuery();
                                command.Transaction.Commit();
                            }
                            catch (Exception ex)
                            {
                                command.Transaction.Rollback();
                                error += ex.Message + "\r\n" + ex.InnerException + "\r\n" + ex.Source + "\r\n" + ex.StackTrace + "\r\n";
                            }
                            finally
                            {
                                connection.Close();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        error += ex.Message + "\r\n" + ex.InnerException + "\r\n" + ex.Source + "\r\n" + ex.StackTrace + "\r\n";
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                error += ex.Message + "\r\n" + ex.InnerException + "\r\n" + ex.Source + "\r\n" + ex.StackTrace + "\r\n";
            }

            return error;
        }

        public string Run()
        {
            var error = string.Empty;

            try
            {

                string conn = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                
                using (OracleConnection connection = new OracleConnection(conn))
                {
                   OracleCommand command = new OracleCommand(@"SELECT report.*, study.PATIENT_NAME, study.CREATION_DTTM  FROM MOVEREPORT report
                                                                inner join study
                                                                on report.study_key = study.study_key WHERE STATUS = 0", connection);
                    connection.Open();
                    connection.BeginTransaction();

                    try
                    {
                        /*var da = new OracleDataAdapter(command);
                        var cb = new OracleCommandBuilder(da);
                        var ds = new DataSet();
                        da.Fill(ds);*/
                        OracleDataReader reader;
                        reader = command.ExecuteReader();

                        
                        while (reader.Read())
                        {

                            OracleClob clob = reader.GetOracleClob(2);
                            var cellValue = (string)clob.Value;
                            
                            try
                            {
                                var ACCESSNUMBER = reader["ACCESSNUMBER"].ToString();
                                var STUDY_KEY = reader["STUDY_KEY"].ToString();
                                var report = reader["REPORT_TEXT_LOB"].ToString().Replace("====== [Conclusion] ======", "");
                                var patient_name = reader["PATIENT_NAME"].ToString();
                                var creation_data = reader["CREATION_DTTM"].ToString();
                                var CRM = reader["CRM"].ToString();
                                CRM = Regex.Match(CRM, @"\d+").Value;

                                RunAsync(ACCESSNUMBER, STUDY_KEY, report, patient_name, creation_data, command, connection, CRM).Wait();
                                //====== [Conclusion] ======

                                
                            }
                            catch(Exception ex)
                            {
                                command.Transaction.Rollback();
                                error += ex.Message + "\r\n" + ex.InnerException + "\r\n" + ex.Source + "\r\n" + ex.StackTrace;
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        command.Transaction.Rollback();
                        error += ex.Message + "\r\n" + ex.InnerException + "\r\n" + ex.Source + "\r\n" + ex.StackTrace + "\r\n";
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                error += ex.Message + "\r\n" + ex.InnerException + "\r\n" + ex.Source + "\r\n" + ex.StackTrace + "\r\n";
            }

            return error;
        }

        static async Task RunAsync(string ACCESSNUMBER, string STUDY_KEY, string report, string patient_name, string creation_data, OracleCommand command, OracleConnection connection, string CRM)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("http://172.17.100.30:9090/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var teste = "";
            try
            {
                Token token = new Token {
                                UserName = "des",
                                Password = "benner",
                                Operations = new string[] { "CadastraLaudo" }
                };
                teste = JsonConvert.SerializeObject(token);
                var tk = await GetTokenAsync(client, token);
                teste = tk + "\r\n" + ACCESSNUMBER + "\r\n" + CRM;
                var chv = ACCESSNUMBER.Substring(ACCESSNUMBER.Length - 2);
                var os = ACCESSNUMBER.Substring(0, ACCESSNUMBER.Length - 2);
                teste = chv + "\r\n" + CRM + "\r\n" + os;
                Laudos laudo = new Laudos {
                                    Id = Convert.ToInt32(chv),
                                    Laudo = report,
                                    Crm = Convert.ToInt32(CRM),
                                    DtRealizacao = creation_data
                };

                

                // Create a new product
                CadastraLaudo cadastroLaudo = new CadastraLaudo { Laudos = new object[] { laudo },
                                                                  NomePaciente = patient_name,
                                                                  Os = Convert.ToInt64(os),
                                                                  Token = tk,
                                                                  UserName = "des"
                };
                teste = JsonConvert.SerializeObject(cadastroLaudo);
                var ret = await SendLaudoAsync(client, cadastroLaudo);

                command = new OracleCommand("UPDATE MOVEREPORT SET STATUS = 1, ERROR = '" + ret + "' WHERE STUDY_KEY = '" + STUDY_KEY + "'", connection);
                command.ExecuteNonQuery();
                command.Transaction.Commit();

            }
            catch (Exception e)
            {
                throw new Exception(teste + "\r\n" + e.Message + "\r\n" +  e.InnerException + "\r\n" + e.StackTrace);
            }
            
        }

        static async Task<string> SendLaudoAsync(HttpClient client, CadastraLaudo cadastraLaudo)
        {
            StringContent content = new StringContent(JsonConvert.SerializeObject(cadastraLaudo), Encoding.UTF8, "application/json");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
            HttpResponseMessage response = await client.PostAsync("API/Laudo/CadastraLaudo", content);
            response.EnsureSuccessStatusCode();
            string msg;
            if (response.IsSuccessStatusCode)
            {
                msg = await response.Content.ReadAsStringAsync();
            }
            else
            {
                msg = response.StatusCode.ToString();
            }
            // return URI of the created resource.
            return msg;
        }

        static async Task<string> GetTokenAsync(HttpClient client, Token token)
        {

            StringContent content = new StringContent(JsonConvert.SerializeObject(token), Encoding.UTF8, "application/json");
            
            HttpResponseMessage response = await client.PostAsync("api/Laudo/GetHash", content);
            string msg;
            if (response.IsSuccessStatusCode)
            {
                var data = (Newtonsoft.Json.Linq.JArray)await response.Content.ReadAsAsync<object>();
                var teste = data.ToObject<string[]>();
                msg = teste[0];
            }
            else
            {
                msg = response.StatusCode.ToString();
            }
            return msg;
        }
    }
    [Serializable]
    public class Token
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string[] Operations { get; set; }
    }
    [Serializable]
    public class CadastraLaudo
    {
        public string UserName { get; set; }
        public string Token { get; set; }
        public long Os { get; set; }
        public string NomePaciente { get; set; }
        public object[] Laudos { get; set; }
    }
    [Serializable]
    public class Laudos
    {
        public int Id { get; set; }
        public string Laudo { get; set; }
        public int Crm { get; set; }
        public string DtRealizacao { get; set; }
    }
}