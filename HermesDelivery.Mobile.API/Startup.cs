using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Web.Http;
using HermesDelivery.Mobile.API.App_Extension;
using HermesDelivery.Mobile.API.App_Extension.OAuth;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Security.OAuth;
using Owin;

[assembly: OwinStartup(typeof(HermesDelivery.Mobile.API.Startup))]

namespace HermesDelivery.Mobile.API
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var config = new HttpConfiguration();
            config.MessageHandlers.Add(new WebApiConfig.CrossDomainHandler());
            app.UseWebApi(config); 
        }
        public static OAuthAuthorizationServerOptions OAuthOptions { get; private set; }

        public static Func<UserManager<IdentityUser>> UserManagerFactory { get; set; }

        public static string PublicClientId { get; private set; }
    }
}