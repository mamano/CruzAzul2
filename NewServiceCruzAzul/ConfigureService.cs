using Topshelf;

namespace NewServiceCruzAzul
{
    internal static class ConfigureService
    {
        internal static void Configure()
        {
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
