using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(YMLParser.Startup))]
namespace YMLParser
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
