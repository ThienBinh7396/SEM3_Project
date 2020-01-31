using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(RechargeWeb.Startup))]
namespace RechargeWeb
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
