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
        private const string V = "0";
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
                RecurringJob.AddOrUpdate("delete", () => Delete(), "0 0 */20 * *");


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
                    connection.Open();
                    OracleCommand command = new OracleCommand("delete from movereport where approval_dttm <=  (SELECT SYSDATE -10   from dual) and status = 1", connection);
                    
                    var transaction = connection.BeginTransaction();
                    //var sourceFilePath = sourcePath + "\\" + dr["PATHNAME"].ToString() + "\\" + dr["FILENAME"].ToString();
                    command.Transaction = transaction;
                    try
                    {
                        command.ExecuteNonQuery();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        connection.Close();
                        error = $" error: {ex}";
                    }
                    finally
                    {
                        connection.Close();
                        error = $"Sucesso";
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return error;
        }

        [DisableConcurrentExecution(timeoutInSeconds: 0)]
        public string Run()
        {
            error = string.Empty;
            var STUDY_KEY = string.Empty;
            try
            {

                string conn = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                
                using (OracleConnection connection = new OracleConnection(conn))
                {
                    connection.Open();
                    OracleCommand command = new OracleCommand(@"SELECT report.*, study.PATIENT_NAME, TO_CHAR(study.CREATION_DTTM, 'YYYY-MM-DD hh24:mi:ss')  CREATION_DTTM  FROM MOVEREPORT report
                                                                inner join study
                                                                on report.study_key = study.study_key WHERE STATUS = 0 and ROWNUM < 500 AND SOURCE_AETITLE not IN ('DSVR5','DSVR9')", connection);
                    
                    var transaction = connection.BeginTransaction();

                    try
                    {
                        /*var da = new OracleDataAdapter(command);
                        var cb = new OracleCommandBuilder(da);
                        var ds = new DataSet();
                        da.Fill(ds);*/
                        OracleDataReader reader;
                        command.Transaction = transaction;
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
                            using (OracleConnection connection1 = new OracleConnection(conn))
                            {
                                connection1.Open();
                                var transaction1 = connection1.BeginTransaction();
                                RunAsync(ACCESSNUMBER, STUDY_KEY, report, patient_name, creation_data, null, connection1, CRM, transaction1).Wait();
                                //====== [Conclusion] ======
                            }
                        }
                    }
                    catch(Exception ex)
                    {   
                        error += $" error {{ studykey: {STUDY_KEY}, ex: {ex} }}";
                        command.Connection = connection;
                        command.CommandText = $"UPDATE MOVEREPORT SET STATUS = 1, ERROR = :clobparam WHERE STUDY_KEY = '{STUDY_KEY}'";
                        OracleParameter clobparam = new OracleParameter("clobparam", OracleDbType.Clob, error.Length);
                        clobparam.Direction = ParameterDirection.Input;
                        clobparam.Value = error;
                        command.Parameters.Add(clobparam);
                        command.Transaction = transaction;
                        command.ExecuteNonQuery();
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
            catch 
            {
                return error;
            }

            return error;
        }

        static async Task RunAsync(string ACCESSNUMBER, string STUDY_KEY, string report, string patient_name, string creation_data, OracleCommand command, OracleConnection connection, string CRM, OracleTransaction transaction)
        {
            var client = new HttpClient();
            var host = ConfigurationManager.AppSettings["host"];
            var token = "username=tor&password=tor@1234&grant_type=password";
            client.BaseAddress = new Uri(host);
            client.DefaultRequestHeaders.Accept.Clear();
            error += $"Start: {STUDY_KEY} \r\n";
            var teste = default(string);
            command = new OracleCommand();
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
                                    Laudo = PlainTextToRtf(report),
                                    Crm = CRM ?? V
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

                command = new OracleCommand();
                command.Connection = connection;
                command.CommandText = $"UPDATE MOVEREPORT SET STATUS = 1, ERROR = :clobparam WHERE STUDY_KEY = '{STUDY_KEY}'";
                OracleParameter clobparam = new OracleParameter("clobparam", OracleDbType.Clob, teste.Length);
                clobparam.Direction = ParameterDirection.Input;
                clobparam.Value = teste;
                command.Parameters.Add(clobparam);
                command.Transaction = transaction;
                command.ExecuteNonQuery();
                transaction.Commit();
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
                transaction.Rollback();
                teste = $" laudo: {teste} exception: {e} \r\n ";
                command = new OracleCommand();
                command.Connection = connection;
                command.CommandText = $"UPDATE MOVEREPORT SET STATUS = 1, ERROR = :clobparam WHERE STUDY_KEY = '{STUDY_KEY}'";
                OracleParameter clobparam = new OracleParameter("clobparam", OracleDbType.Clob, teste.Length);
                clobparam.Direction = ParameterDirection.Input;
                clobparam.Value = teste;
                command.Parameters.Add(clobparam);
                command.Transaction = transaction;
                command.ExecuteNonQuery();
                transaction.Commit();
            }
            finally
            {
                connection.Close();
            }
            error += "End \r\n";
        }

        static async Task<ResultCadastroLaudo> SendLaudoAsync(HttpClient client, CadastraLaudo cadastraLaudo)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "webapi/api/integracoes/laudo/cadastrarLaudo");
            request.Content = new StringContent(JsonConvert.SerializeObject(cadastraLaudo), Encoding.UTF8, "application/json");
            var response = await client.SendAsync(request);


            var result = new ResultCadastroLaudo();



            result.Descricao = $"resposta: {response.ReasonPhrase} - {((int)response.StatusCode).ToString()} \r\n";
            if (response != null)
            {
                var data = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(data))
                    result = JsonConvert.DeserializeObject<ResultCadastroLaudo>(data);
            }
            // return URI of the created resource.
            return result;
        }

        static async Task<Token> GetTokenAsync(HttpClient client, string token)
        {

            //StringContent content = new StringContent(JsonConvert.SerializeObject(token), Encoding.UTF8, "application/json");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "webapi/token");
            request.Content = new StringContent(token, Encoding.UTF8, "application/x-www-form-urlencoded");
            var response = await client.SendAsync(request);
            //response.EnsureSuccessStatusCode();

            var result = new Token();



                result.token_type = $"resposta: {response.ReasonPhrase} - {((int)response.StatusCode).ToString()} \r\n";
            //var data = (Newtonsoft.Json.Linq.JArray)await response.Content.ReadAsAsync<object>();
            //var teste = data.ToObject<string[]>();
            //msg = teste[0];
            if (response != null)
            {
                var data = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(data))
                    result = JsonConvert.DeserializeObject<Token>(data);
            }

            
            return result;
        }

        public static string PlainTextToRtf(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return "";

            string escapedPlainText = plainText.Replace(@"\", @"\\").Replace("{", @"\{").Replace("}", @"\}");
            escapedPlainText = EncodeCharacters(escapedPlainText);

            string rtf = @"{\rtf1\ansi\ansicpg1250\deff0{\fonttbl\f0\fswiss Helvetica;}\f0\pard ";
            rtf += escapedPlainText.Replace(Environment.NewLine, "\\par\r\n ");
            rtf += " }";
            return rtf;
        }

        private static string EncodeCharacters(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "";

            return text
                .Replace("ą", @"\'b9")
                .Replace("ć", @"\'e6")
                .Replace("ę", @"\'ea")
                .Replace("ł", @"\'b3")
                .Replace("ń", @"\'f1")
                .Replace("ó", @"\'f3")
                .Replace("ś", @"\'9c")
                .Replace("ź", @"\'9f")
                .Replace("ż", @"\'bf")
                .Replace("Ą", @"\'a5")
                .Replace("Ć", @"\'c6")
                .Replace("Ę", @"\'ca")
                .Replace("Ł", @"\'a3")
                .Replace("Ń", @"\'d1")
                .Replace("Ó", @"\'d3")
                .Replace("Ś", @"\'8c")
                .Replace("Ź", @"\'8f")
                .Replace("Ż", @"\'af");
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