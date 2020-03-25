using Microsoft.Extensions.Hosting;
using Topshelf;

namespace GhelfondService
{
    class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(config =>
            {
                config.Service<BootStrap>(service =>
                {
                    service.ConstructUsing(s => new BootStrap());
                    service.WhenStarted(s => s.Start());
                    service.WhenStopped(s => s.Stop());
                });
                config.RunAsLocalSystem();
                config.SetDescription("TopShelf Service");
                config.SetDisplayName("TopShelfService");

            });
        }
    }
}
