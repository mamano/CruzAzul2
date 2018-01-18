using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(CruzAzulWeb.Startup))]
namespace CruzAzulWeb
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
