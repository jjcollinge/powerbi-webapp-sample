using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(KPMGSample.Startup))]
namespace KPMGSample
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
