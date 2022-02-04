namespace Gigya.Identity.Client.Models
{
    public class GigyaOP
    {
        public string DataCenter;
        public string ApiKey;

    }
    
    public static class OPContext
    {
         
        public static string GigyaJS(this GigyaOP op, string module = "gigya.js") =>
            $"https://cdns.{op.DataCenter}.gigya.com/js/{module}?apiKey={op.ApiKey}";

        public static string OidcJS(this GigyaOP op)=> op.GigyaJS("gigya.oidc.js");
    }
}