using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Dicom.Network;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IHostedServiceAsAService
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var server = DicomServer.Create<CStoreSCP>(11112);
            var isService = !(Debugger.IsAttached || args.Contains("--console"));

            var builder = new HostBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<FileWriterService>();
                });

            if (isService)
            {
                await builder.RunAsServiceAsync();
            }
            else
            {
                await builder.RunConsoleAsync();
            }
        }
    }
}