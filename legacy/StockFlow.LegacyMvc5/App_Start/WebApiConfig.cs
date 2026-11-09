using System.Web.Http;

namespace StockFlow.LegacyMvc5.App_Start
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API 2 attribute routing
            config.MapHttpAttributeRoutes();

            // Web API 2 convention-based routes
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // Force JSON response formatting in legacy pipeline
            config.Formatters.Remove(config.Formatters.XmlFormatter);
        }
    }
}
