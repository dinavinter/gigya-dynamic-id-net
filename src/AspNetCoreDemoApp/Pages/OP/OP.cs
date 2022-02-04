using Microsoft.AspNetCore.Routing;

namespace Ocelot.Web.Pages.OP
{
    public class GigyaOP
    {
        public string DataCenter;
        public string ApiKey;
    }

    public static class OPContext
    {
        public static string ApiKey(this RouteData routeData) => routeData.Values["apikey"]?.ToString();
        public static string Domain(this RouteData routeData) => routeData.Values["domain"]?.ToString();

        public static string GigyaJS(this RouteData routeData, string module = "gigya.js") =>
            $"https://cdns.{routeData.Domain()}.gigya.com/js/{module}?apiKey={routeData.ApiKey()}";

        public static string OidcJS(this RouteData routeData)=> routeData.GigyaJS("gigya.oidc.js");
        
        public static string GigyaJS(this GigyaOP op, string module = "gigya.js") =>
            $"https://cdns.{op.DataCenter}.gigya.com/js/{module}?apiKey={op.ApiKey}";

        public static string OidcJS(this GigyaOP op)=> op.GigyaJS("gigya.oidc.js");
    }
}