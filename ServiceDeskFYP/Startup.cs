using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ServiceDeskFYP.Startup))]
namespace ServiceDeskFYP
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
