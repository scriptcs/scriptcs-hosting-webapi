using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace ScriptCs.Hosting.WebApi.IIS
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
            var builder = new WebApiConfigurationBuilder(config, HttpContext.Current.Server.MapPath("bin"));
            builder.Build();

        }
    }
}
