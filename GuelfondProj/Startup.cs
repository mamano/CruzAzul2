using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(GuelfondProj.Startup))]
namespace GuelfondProj
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
