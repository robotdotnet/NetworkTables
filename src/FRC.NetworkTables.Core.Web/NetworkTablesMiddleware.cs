using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
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

            var nt = app.ApplicationServices.GetService<NetworkTableInstance>();
            nt.AddConnectionListener((in ConnectionNotification n) =>
            {
                var hub = app.ApplicationServices.GetService<IHubContext<NTHub>>();
                hub.Clients.All.SendAsync("ConnectionNotification", n.Conn, n.Connected).Wait();
            }, false);

            _ = nt.AddEntryListener("", (in RefEntryNotification notification) =>
              {
                  var (Name, Value, Flags) =
                  (
                      notification.Name.ToString(),
                      notification.Value.Value.GetValue(),
                      notification.Flags
                  );
                  var hub = app.ApplicationServices.GetService<IHubContext<NTHub>>();
                  hub.Clients.All.SendAsync("EntryNotification", Name, Value, Flags).Wait();
              }, (NotifyFlags)0xFFFFFFFFu);



            app.Use(async (context, next) =>
            {
                var requestPath = context.Request.Path.ToString();
                if (requestPath.StartsWith("/nttable", StringComparison.InvariantCultureIgnoreCase))
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
        }
    }
}
