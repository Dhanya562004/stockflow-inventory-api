using System;
using System.Diagnostics;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using StockFlow.LegacyMvc5.App_Start;

namespace StockFlow.LegacyMvc5
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            // Traditional ASP.NET MVC 5 and Web API 2 initialization pipeline
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            Trace.WriteLine("Legacy ASP.NET MVC 5 application initialized on .NET Framework 4.8.");
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            // Legacy per-request initialization hook
            HttpContext.Current.Response.AddHeader("X-Legacy-Framework", "ASP.NET MVC 5 (.NET Framework 4.8)");
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            // Legacy global unhandled exception handler
            Exception exception = Server.GetLastError();
            if (exception != null)
            {
                Trace.TraceError("Unhandled exception caught in Global.asax: " + exception.Message);
            }
        }
    }
}
