﻿using System;
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
using System.Text.RegularExpressions;

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
                SchedulePollingInterval = TimeSpan.FromMinutes(1),
                Queues = new[] { "run", "delete", "deletefile" }
            };

            QueueAttribute queue = new QueueAttribute("run");

            RecurringJob.AddOrUpdate("run", () => Run(), "* * * * *", queue: "run");  
            RecurringJob.AddOrUpdate("delete", () => Delete(), "0 0 1 */6 *", queue: "delete");
            RecurringJob.AddOrUpdate("deletefile", () => DeleteFile(), "0 0 * * *", queue: "deletefile");

            app.UseHangfireDashboard();
            app.UseHangfireServer(option);
        }
        [Queue("deletefile")]
        public string DeleteFile()
        {
            var error = string.Empty;

            try
            {
                string conn = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                var ds = new DataSet();
                using (var connection = new OracleConnection(conn))
                {

                    var command = new OracleCommand("SELECT * FROM MOVEPDF WHERE STATUS = 1 AND (DELETEFILE = 0 OR  DELETEFILE IS NULL) AND ROWNUM < 100", connection);
                    connection.Open();
                    connection.BeginTransaction();
                    var da = new OracleDataAdapter(command);
                    var cb = new OracleCommandBuilder(da);

                    da.Fill(ds);
                    connection.Close();
                }
                try
                {
                    var sourcePath = ConfigurationManager.AppSettings["SourcePathPDF"];
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {

                        var sourceFilePath = sourcePath + "\\" + dr["PATHNAME"].ToString() + "\\" + dr["FILENAME"].ToString();
                        try
                        {
                            FileInfo f1 = new FileInfo(sourceFilePath);
                            if(f1.Exists)
                            {
                                f1.Delete();
                            }
                            using (var connection = new OracleConnection(conn))
                            {
                                var command = new OracleCommand($"UPDATE MOVEPDF SET DELETEFILE = 1 WHERE GENERAL_FILE_KEY = '{dr["GENERAL_FILE_KEY"].ToString()}'", connection);

                                try
                                {
                                    connection.Open();
                                    connection.BeginTransaction();
                                    command.ExecuteNonQuery();
                                    command.Transaction.Commit();

                                }
                                catch (Exception ex)
                                {
                                    command.Transaction.Rollback();
                                    error += $" error update report: {ex}";
                                }
                                finally
                                {
                                    connection.Close();
                                }
                            }
                                
                        }
                        catch (Exception ex)
                        {
                            error += $"error: {ex} - {dr}\r\n";
                           
                        }
                    }
                }
                catch (Exception ex)
                {
                    error += $"error: {ex}";  
                }
            }
            catch (Exception ex)
            {
                error += $"error: {ex}";
            }

            return error;
        }

        [Queue("delete")]
        public string Delete()
        {
            var error = string.Empty;

            try
            {
                string conn = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

                using (OracleConnection connection = new OracleConnection(conn))
                {
                    OracleCommand command = new OracleCommand("DELETE FROM MOVEPDF WHERE STATUS = 1", connection);
                    //OracleCommand command = new OracleCommand("SELECT * FROM MOVEPDF WHERE STATUS = 1", connection);
                    connection.Open();
                    connection.BeginTransaction();

                    try
                    {
                        //var da = new OracleDataAdapter(command);
                        //var cb = new OracleCommandBuilder(da);
                        //var ds = new DataSet();
                        //da.Fill(ds);
                        
                        //var sourcePath = ConfigurationManager.AppSettings["SourcePathPDF"];
                        //foreach (DataRow dr in ds.Tables[0].Rows)
                        //{

                            //var sourceFilePath = sourcePath + "\\" + dr["PATHNAME"].ToString() + "\\" + dr["FILENAME"].ToString();
                            //try
                            //{
                                /*FileInfo f1 = new FileInfo(sourceFilePath);
                                f1.Delete();
                                Directory.Delete(sourcePath + "\\" + dr["PATHNAME"].ToString());*/
                                //OracleCommand command = new OracleCommand("DELETE FROM MOVEPDF WHERE STATUS = 1 AND ROWNUM < 10", connection);
                                command.ExecuteNonQuery();
                                command.Transaction.Commit();
                                
                        //    }
                        //    catch (Exception ex)
                        //    {
                        //        command.Transaction.Rollback();
                        //        error += ex.Message + "\r\n" + ex.InnerException + "\r\n" + ex.Source + "\r\n" + ex.StackTrace + "\r\n";
                        //    }
                        //    finally
                        //    {
                        //        connection.Close();
                        //    }
                        ////}
                    }
                    catch (Exception ex)
                    {
                        command.Transaction.Rollback();
                        error += ex.Message + "\n" + ex.InnerException + "\n" + ex.Source + "\n" + ex.StackTrace + "\n";
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                error += ex.Message + "\n" + ex.InnerException + "\n" + ex.Source + "\n" + ex.StackTrace + "\n";
            }

            return error;
        }

        [Queue("run")]
        [DisableConcurrentExecution(timeoutInSeconds: 60)]
        public string Run()
        {
            var error = string.Empty;

            try
            {

                string conn = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                var ds = new DataSet();
                using (OracleConnection connection = new OracleConnection(conn))
                {
                    var command = new OracleCommand(@"SELECT 
                                                                PDF.STATUS AS STATUS, 
                                                                PDF.STUDY_INSTANCE_UID AS STUDY_INSTANCE_UID,
                                                                PDF.PATHNAME AS PATHNAME,
                                                                PDF.FILENAME AS FILENAME,
                                                                PDF.ACCESSNUMBER AS ACCESSNUMBER,
                                                                PDF.ACCESS_DTTM AS ACCESS_DTTM,
                                                                PDF.GENERAL_FILE_KEY AS GENERAL_FILE_KEY,
                                                                PDF.DELETEFILE AS DELETEFILE,
                                                                S.MODALITIES AS MODALITIES, 
                                                                S.STUDY_DESC AS STUDY_DESC 
                                                                FROM MOVEPDF PDF
                                                                INNER JOIN  STUDY S 
                                                                ON S.STUDY_INSTANCE_UID = PDF.STUDY_INSTANCE_UID 
                                                                WHERE STATUS = 0 AND ROWNUM < 10", connection);
                    connection.Open();
                    connection.BeginTransaction();


                    var da = new OracleDataAdapter(command);
                    var cb = new OracleCommandBuilder(da);

                    da.Fill(ds);
                    connection.Close();
                }
                try
                {
                    var sourcePath = ConfigurationManager.AppSettings["SourcePathPDF"];
                    var destPath = ConfigurationManager.AppSettings["DestPathPDF"];
                    var destPath2 = ConfigurationManager.AppSettings["DestPathPDF2"];
                        
                    var MODALITIES = string.Empty;
                    var STUDY_DESC = string.Empty;
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {

                        var filename = dr["FILENAME"].ToString();
                        var pathName = dr["PATHNAME"].ToString();
                        var accessnumber = dr["ACCESSNUMBER"].ToString();
                        var study_uid = dr["STUDY_INSTANCE_UID"].ToString();
                        var sourceFilePath = $"{sourcePath}\\{pathName}\\{filename}";
                        var destFilePath2 = $"{destPath2}\\{accessnumber}.pdf";
                        try
                        {
                            FileInfo f1 = new FileInfo(sourceFilePath);
                            f1.CopyTo(destFilePath2, true);
                            using (var connection = new OracleConnection(conn))
                            {
                                var command = new OracleCommand($"UPDATE MOVEPDF SET STATUS = 1 WHERE STUDY_INSTANCE_UID = '{study_uid}'", connection);
                                try
                                {   
                                    connection.Open();
                                    connection.BeginTransaction();
                                    command.ExecuteNonQuery();
                                    command.Transaction.Commit();
                                    
                                }
                                catch(Exception ex)
                                {
                                    command.Transaction.Rollback();
                                    error += $" error update report: {ex}";
                                }
                                finally
                                {
                                    connection.Close();
                                }

                            }
                            error += $" study_uid: {study_uid}, patientID: {accessnumber}";
                        }
                        catch(Exception ex)
                        {
                            error += $" error:{ex} \n origem: {sourceFilePath} \n  destino: {destFilePath2}";
                        }
                    }
                }
                catch (Exception ex)
                {
                    error += $" error:{ex} ";
                }
            }
            catch (Exception ex)
            {
                error += $" error:{ex} ";
            }
            return error;
        }
    }
}