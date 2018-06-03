using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace FRC.NetworkTables.Core.Web
{
    public static class NetworkTablesMiddleware
    {
        public static void AddNetworkTables(this IServiceCollection services, NetworkTableInstance ntInstance)
        {
            services.AddSignalR();
            services.AddSingleton(typeof(NetworkTableInstance), ntInstance);
        }

        private static List<NtResponse> HandleNTRequest(ReadOnlySpan<char> remaining, HttpContext context, NetworkTableInstance nt)
        {
            var entries = nt.GetEntries(remaining, 0);
            List<NtResponse> responses = new List<NtResponse>();
            foreach (var entry in entries)
            {
                responses.Add(new NtResponse()
                {
                    Key = entry.GetEntryName(),
                    Flags = entry.GetEntryFlags(),
                    LastChange = entry.GetLastChange(),
                    Value = entry.GetObjectValue()
                });
            }
            return responses;
        }

        public static void UseNetworkTables(this IApplicationBuilder app)
        {
            app.UseSignalR(routes =>
            {
                routes.MapHub<NTHub>("/nthub");
            });

            app.Use(async (context, next) =>
            {
                var requestPath = context.Request.Path.ToString();
                if (requestPath.StartsWith("/nt", StringComparison.InvariantCultureIgnoreCase))
                {
                    var remaining = requestPath.AsMemory().Slice(3);
                    if (context.Request.Method == "GET")
                    {
                        await context.Response.WriteAsync(JsonConvert.SerializeObject(HandleNTRequest(remaining.Span, context, app.ApplicationServices.GetService<NetworkTableInstance>())));
                        return;
                    }
                    else
                    {
                        context.Response.StatusCode = 404;
                        return;
                    }
                }
                await next.Invoke();
            });

            app.ApplicationServices.GetService<NetworkTableInstance>();
        }
    }
}
