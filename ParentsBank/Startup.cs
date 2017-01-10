using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ParentsBank.Startup))]
namespace ParentsBank
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
