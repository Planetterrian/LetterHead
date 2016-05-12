using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(LetterHeadServer.Startup))]
namespace LetterHeadServer
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
