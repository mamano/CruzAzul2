using Hangfire;
using Hangfire.sql
using Microsoft.AspNetCore.Builder;
using Owin;
using System;
using System.Configuration;

namespace ConsoleApp4
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
        }
    }

    public class Startup
    {
        /// <summary>
        /// Configure Hangfire Connection string, dashboard
        /// </summary>
        /// <param name="appBuilder"></param>
        public void Configuration(IApplicationBuilder appBuilder)
        {
            GlobalConfiguration.Configuration.UseSQLiteStorage(ConfigurationManager
                .ConnectionStrings["HangFireConnectionString"].ConnectionString);

            appBuilder.UseHangfireDashboard();
            appBuilder.UseHangfireServer();

        }
    }
}
