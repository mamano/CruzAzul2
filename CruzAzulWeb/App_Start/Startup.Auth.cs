using System;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Owin;
using CruzAzulWeb.Models;
using Hangfire;
using Hangfire.SQLite;
using System.Web;
using System.Diagnostics;
using System.Configuration;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.IO;
using Spire.Pdf;

namespace CruzAzulWeb
{
    public partial class Startup
    {
        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            var name = String.Format(
        "{0}.{1}",
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
                   OracleCommand command = new OracleCommand("SELECT * FROM MOVEPDF WHERE STATUS = 0", connection);
                    connection.Open();
                    connection.BeginTransaction();

                    try
                    {
                        var da = new OracleDataAdapter(command);
                        var cb = new OracleCommandBuilder(da);
                        var ds = new DataSet();
                        da.Fill(ds);
                        

                        var sourcePath = ConfigurationManager.AppSettings["SourcePathPDF"];
                        var destPath = ConfigurationManager.AppSettings["DestPathPDF"];
                        var destPath2 = ConfigurationManager.AppSettings["DestPathPDF2"];
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {

                            var sourceFilePath = sourcePath + "\\" + dr["PATHNAME"].ToString() + "\\" + dr["FILENAME"].ToString();
                            var destFilePath = destPath + "\\" + dr["ACCESSNUMBER"].ToString() + ".pdf";
                            var destFilePath2 = destPath2 + "\\" + dr["ACCESSNUMBER"].ToString() + ".pdf";
                            try
                            {
                                
                                /*PdfDocument pdf = new PdfDocument();
                                pdf.LoadFromFile(sourceFilePath);
                                string output = destPath + "\\" + dr["ACCESSNUMBER"].ToString() + ".doc";
                                pdf.SaveToFile(output, FileFormat.DOC);
                                System.Diagnostics.Process.Start(output);*/

                                FileInfo f1 = new FileInfo(sourceFilePath);
                                f1.CopyTo(destFilePath, true);
                                f1.Delete();
                                f1 = new FileInfo(destFilePath);
                                f1.CopyTo(destFilePath2, true);
                                command = new OracleCommand("UPDATE MOVEPDF SET STATUS = 1 WHERE STUDY_INSTANCE_UID = '" + dr["STUDY_INSTANCE_UID"] + "'", connection);
                                command.ExecuteNonQuery();
                                command.Transaction.Commit();
                            }
                            catch(Exception ex)
                            {
                                command.Transaction.Rollback();
                                error += ex.Message + "\r\n" + ex.InnerException + "\r\n" + ex.Source + "\r\n" + ex.StackTrace + "\r\n" + sourceFilePath + "\r\n" + destFilePath;
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
    }
}