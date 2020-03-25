using Owin;
using Hangfire;
using System;
using Microsoft.Owin.Hosting;
using System.Configuration;
using Hangfire.SQLite;

namespace GhelfondService
{
    public class BootStrap
    {
        private IDisposable _host;

        /// <summary>
        /// Configure owin hosting options for the hangfire
        /// </summary>
        public void Start()
        {
           
            var options = new StartOptions { Port = 8999 };
            _host = WebApp.Start<Startup>(options);
            Console.WriteLine();
            Console.WriteLine("HangFire has started");
            Console.WriteLine("Dashboard is available at http://localhost:8999/hangfire");
            Console.WriteLine();
        }

        public void Stop()
        {
            _host.Dispose();
        }
    }

    public class Startup
    {
        /// <summary>
        /// Configure Hangfire Connection string, dashboard
        /// </summary>
        /// <param name="appBuilder"></param>
        public void Configuration(Owin.IAppBuilder appBuilder)
        {
            var options = new SQLiteStorageOptions();

            
            GlobalConfiguration.Configuration.UseSQLiteStorage("SQLiteHangfire", options);

            var option = new BackgroundJobServerOptions
            {
                ServerName = "Ghelfond",
                WorkerCount = 1,
                SchedulePollingInterval = TimeSpan.FromMinutes(1)
            };
            appBuilder.UseHangfireDashboard();
            appBuilder.UseHangfireServer(option);   

            var jobSvc = new HangFireService();
            jobSvc.ScheduleJobs();
        }
    }
}