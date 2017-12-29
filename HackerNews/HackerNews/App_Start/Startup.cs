using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(HackerNews.Startup))]
namespace HackerNews
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}