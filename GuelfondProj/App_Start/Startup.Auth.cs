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

        private static string error = string.Empty;
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
                RecurringJob.AddOrUpdate("delete", () => Delete(), "0 0 */10 * *");


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
                    OracleCommand command = new OracleCommand("SELECT * FROM MOVEPDF WHERE STATUS = 1 AND ROWNUM < 10", connection);
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
                                command = new OracleCommand("DELETE FROM MOVEPDF WHERE STUDY_KEY = '{STUDY_KEY}'", connection);
                                command.ExecuteNonQuery();
                                command.Transaction.Commit();
                            }
                            catch (Exception ex)
                            {
                                command.Transaction.Rollback();
                                connection.Close();
                                throw new Exception($" error: {ex}");
                            }
                            finally
                            {
                                connection.Close();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return error;
        }

        public string Run()
        {
            error = string.Empty;
            var STUDY_KEY = string.Empty;
            try
            {

                string conn = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                
                using (OracleConnection connection = new OracleConnection(conn))
                {
                   OracleCommand command = new OracleCommand(@"SELECT report.*, study.PATIENT_NAME, TO_CHAR(study.CREATION_DTTM, 'YYYY-MM-DD hh24:mi:ss')  CREATION_DTTM  FROM MOVEREPORT report
                                                                inner join study
                                                                on report.study_key = study.study_key WHERE STATUS = 0 and ROWNUM < 10", connection);
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
                            

                            var ACCESSNUMBER = reader["ACCESSNUMBER"].ToString();
                            STUDY_KEY = reader["STUDY_KEY"].ToString();
                            var report = reader["REPORT_TEXT_LOB"].ToString().Replace("====== [Conclusion] ======", "");
                            var patient_name = reader["PATIENT_NAME"].ToString();
                            var creation_data = reader["CREATION_DTTM"].ToString().Replace(" ", "T");
                            var CRM = reader["CRM"].ToString();
                            var regex = new Regex(@"[^\d]");
                            CRM = regex.Replace(CRM, "");

                            RunAsync(ACCESSNUMBER, STUDY_KEY, report, patient_name, creation_data, command, connection, CRM).Wait();
                            //====== [Conclusion] ======

 
                        }
                    }
                    catch(Exception ex)
                    {
                        command.Transaction.Rollback();
                        var teste = $" error {{ studykey: {STUDY_KEY}, ex: {ex} }}";
                        command = new OracleCommand($"UPDATE MOVEREPORT SET STATUS = 1, ERROR = '{teste}' WHERE STUDY_KEY = '{STUDY_KEY}'", connection);
                        command.ExecuteNonQuery();
                    }
                    finally
                    {
                        command.Transaction.Commit();
                        connection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                return error;
            }

            return error;
        }

        static async Task RunAsync(string ACCESSNUMBER, string STUDY_KEY, string report, string patient_name, string creation_data, OracleCommand command, OracleConnection connection, string CRM)
        {
            var client = new HttpClient();
            var host = ConfigurationManager.AppSettings["host"];
            var token = "username=tor&password=tor@1234&grant_type=password";
            client.BaseAddress = new Uri(host);
            client.DefaultRequestHeaders.Accept.Clear();
            error += $"Start: {STUDY_KEY} \r\n";
            var teste = default(string);
            try
            {
                /*Token token = new Token {
                                UserName = "des",
                                Password = "benner",
                                Operations = new string[] { "CadastraLaudo" }
                };*/
                //teste = JsonConvert.SerializeObject(token);
                var tk = await GetTokenAsync(client, token);
                error += $" token: {JsonConvert.SerializeObject(tk)} \r\n";
                teste += $" token: {JsonConvert.SerializeObject(tk)} \r\n";
                var chv = string.Empty;
                var os = string.Empty;
                if (ACCESSNUMBER.Length >= 10)
                {
                    chv = ACCESSNUMBER.Substring(ACCESSNUMBER.Length - 2);
                    os = ACCESSNUMBER.Substring(0, ACCESSNUMBER.Length - 2);
                }
                else
                {
                    chv = ACCESSNUMBER;
                }

                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tk.access_token);

                var laudo = new Laudos {
                                    Id = !string.IsNullOrEmpty(chv) ? Convert.ToInt32(chv) : 0,
                                    Laudo = @"{\\\\rtf1\\\\fbidis\\\\ansi\\\\ansicpg1252\\\\deff0\\\\deflang1046{\\\\fonttbl{\\\\f0\\\\froman\\\\fprq2\\\\fcharset0 LUCIDA CONSOLE;}{\\\\f1\\\\fnil\\\\fcharset0 LUCIDA CONSOLE;}{\\\\f2\\\\fnil\\\\fcharset178 Courier New;}}  {\\\\stylesheet{ Normal;}{\\\\s1 heading 1;}}  \\\\viewkind4\\\\uc1\\\\pard\\\\ltrpar\\\\keepn\\\\s1\\\\b\\\\f0\\\\fs23  " + report.Replace("\r\n", " \\\\par ") + " \\\\par   \\\\par \\\\b   \\\\par }",
                                    Crm = CRM ?? "0"
                };

                

                // Create a new product
                var cadastroLaudo = new CadastraLaudo { Laudos = new object[] { laudo },
                                                                  NomePaciente = patient_name,
                                                                  Os = !string.IsNullOrEmpty(os) ? Convert.ToInt64(os) : 0
                };
                error += $"laudo:  {JsonConvert.SerializeObject(cadastroLaudo)} \r\n";
                teste += $"laudo:  {JsonConvert.SerializeObject(cadastroLaudo)} \r\n";
                var ret = await SendLaudoAsync(client, cadastroLaudo);
                error += $"retorno laudo: {JsonConvert.SerializeObject(ret)}";
                teste += $"retorno laudo: {JsonConvert.SerializeObject(ret)}";
                command = new OracleCommand($"UPDATE MOVEREPORT SET STATUS = 1, ERROR = '{teste}' WHERE STUDY_KEY = '{STUDY_KEY}'", connection);
                command.ExecuteNonQuery();
                //command.Transaction.Commit();

                /*if(ret.Descricao.Contains("Requisição não encontrada"))
                {
                    command = new OracleCommand($"UPDATE STUDY SET STUDY_COMMENTS = '{teste}' WHERE STUDY_KEY = '{STUDY_KEY}'", connection);
                    command.ExecuteNonQuery();
                    //command.Transaction.Commit();
                }*/
            }
            catch (Exception e)
            {
                teste = $" laudo: {teste} exception: {e} \r\n ";
                
                command = new OracleCommand($"UPDATE MOVEREPORT SET STATUS = 1, ERROR = '{teste}' WHERE STUDY_KEY = '{STUDY_KEY}'", connection);
                command.ExecuteNonQuery();
            }
            error += "End \r\n";
        }

        static async Task<ResultCadastroLaudo> SendLaudoAsync(HttpClient client, CadastraLaudo cadastraLaudo)
        {
            var content = new StringContent(JsonConvert.SerializeObject(cadastraLaudo), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("webapi/api/integracoes/laudo/cadastrarLaudo", content);
            var result = new ResultCadastroLaudo();
            result.Descricao = $"resposta: {response.ReasonPhrase} - {((int)response.StatusCode).ToString()} \r\n";
            
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                result = JsonConvert.DeserializeObject<ResultCadastroLaudo>(data);
            }
            // return URI of the created resource.
            return result;
        }

        static async Task<Token> GetTokenAsync(HttpClient client, string token)
        {

            //StringContent content = new StringContent(JsonConvert.SerializeObject(token), Encoding.UTF8, "application/json");
            var  content = new StringContent(token, Encoding.UTF8, "text/plain");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
            var response = await client.PostAsync("webapi/token", content);
            var result = new Token();
            result.token_type = $"resposta: {response.ReasonPhrase} - {((int)response.StatusCode).ToString()} \r\n";
            
            if (response.IsSuccessStatusCode)
            {
                //var data = (Newtonsoft.Json.Linq.JArray)await response.Content.ReadAsAsync<object>();
                //var teste = data.ToObject<string[]>();
                //msg = teste[0];
                var data = await response.Content.ReadAsStringAsync();
                result = JsonConvert.DeserializeObject<Token>(data);

            }
            return result;
        }
    }
    [Serializable]
    public class Token
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
    }

    [Serializable]
    public class CadastraLaudo
    {
        public long Os { get; set; }
        public string NomePaciente { get; set; }
        public object[] Laudos { get; set; }
    }
    [Serializable]
    public class Laudos
    {
        public int Id { get; set; }
        public string Laudo { get; set; }
        public string Crm { get; set; }
    }
    [Serializable]
    public class ResultCadastroLaudo
    {
        public string Dados { get; set; }
        public int Codigo { get; set; }
        public string Descricao { get; set; }
        public int StatusRetorno { get; set; }
        public string[] Mensagens { get; set; }
    }
}