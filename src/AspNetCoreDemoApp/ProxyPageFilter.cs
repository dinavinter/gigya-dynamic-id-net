using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Filters;
using Ocelot.Web.Pages.OP;

namespace Gigya.Identity.Client
{
    public class ProxyPageFilter : IPageFilter
    {

        public ProxyPageFilter( )
        {

        }

        public void OnPageHandlerSelected(PageHandlerSelectedContext context)
        {
            Console.WriteLine("Global sync OnPageHandlerSelected called.");

            var domain = context.RouteData.Domain() ?? "us1";

            var apiKey = context.RouteData.ApiKey();
            if (apiKey != null)
            {
                context.HttpContext.Items.TryAdd(typeof(GigyaOP), new GigyaOP()
                {
                    ApiKey = apiKey,
                    DataCenter = domain
                });
            }


        }

        public void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
            Console.WriteLine("Global sync OnPageHandlerExecuting called.");
        }

        public void OnPageHandlerExecuted(PageHandlerExecutedContext context)
        {
            Console.WriteLine("Global sync OnPageHandlerExecuted called.");
        }
    }
}