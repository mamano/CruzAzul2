using log4net;
using log4net.Config;
using System.Configuration;
using System.IO;
using System.Reflection;
using Topshelf;

namespace NewServiceCruzAzul
{
    internal static class ConfigureService
    {
        internal static void Configure()
        {
            var repository = LogManager.GetRepository(Assembly.GetCallingAssembly());
            var file = new FileInfo(ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).FilePath);
            XmlConfigurator.Configure(repository, file);
            HostFactory.Run(configure =>
            {
                configure.Service<CruzAzulService>(service =>
                {
                    service.ConstructUsing(s => new CruzAzulService());
                    service.WhenStarted(s => s.Start());
                    service.WhenStopped(s => s.Stop());
                });
                //Setup Account that window service use to run.  
                configure.RunAsLocalSystem();
                configure.SetServiceName("CruzAzul");
                configure.SetDisplayName("CruzAzul");
                configure.SetDescription("Serviço de integração");
            });
        }
    }
}
