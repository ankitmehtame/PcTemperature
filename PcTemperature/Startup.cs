using PcTemperature.Helpers;
using Microsoft.Owin;
using Owin;
using System.Web.Http;

[assembly: OwinStartup(typeof(PcTemperature.Startup))]

namespace PcTemperature
{
    internal class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            HttpConfiguration config = new HttpConfiguration();

            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            var jsonFormatter = config.Formatters.JsonFormatter;
            jsonFormatter.SerializerSettings = JsonHelper.Settings;
            config.Formatters.Clear();
            config.Formatters.Add(jsonFormatter);

            appBuilder.UseWebApi(config);
        }
    }
}
