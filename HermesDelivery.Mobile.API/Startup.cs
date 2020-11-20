using System.Reflection;
using System.Web.Http;
using CourierAPI;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Startup))] 

namespace CourierAPI
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var config = new HttpConfiguration();
            app.UseWebApi(config);
        }
    }
}