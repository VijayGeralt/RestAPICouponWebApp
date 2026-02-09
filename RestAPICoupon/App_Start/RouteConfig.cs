using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace RestAPICoupon
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // Route root URL to the API Help page
            routes.MapRoute(
                name: "RootHelp",
                url: "",
                defaults: new { controller = "Help", action = "Index" },
                namespaces: new[] { "RestAPICoupon.Areas.HelpPage.Controllers" }
            ).DataTokens = new RouteValueDictionary(new { area = "HelpPage" });

            // Default MVC route removed (API-only + HelpPage area)
        }
    }
}
