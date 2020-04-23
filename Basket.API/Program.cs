using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Basket.API
{
    public class Program
    {
        public static readonly string Namespace = typeof(Program).Namespace;
        public static readonly string AppName = Namespace.Substring(Namespace.LastIndexOf('.', Namespace.LastIndexOf('.') - 1) + 1);
        public static int Main(string[] args)
        {
            //  CreateHostBuilder(args).Build().Run();
            var configuration = GetConfiguration();


            try
            {
                var host = BuildWebHost(configuration, args);

                host.Run();

                return 0;
            }
            catch (Exception ex)
            {
                return 1;
            }
        }

        //public static IHostBuilder CreateHostBuilder(string[] args) =>
        //    Host.CreateDefaultBuilder(args)
        //        .ConfigureWebHostDefaults(webBuilder =>
        //        {
        //            webBuilder.UseStartup<Startup>();
        //        });


        private static IWebHost BuildWebHost(IConfiguration configuration, string[] args) =>
           WebHost.CreateDefaultBuilder(args)
               .CaptureStartupErrors(false)
               .ConfigureKestrel(options =>
               {
                   var ports = GetDefinedPorts(configuration);
                   options.Listen(IPAddress.Any, ports.httpPort, listenOptions =>
                   {
                       listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                   });

                   options.Listen(IPAddress.Any, ports.grpcPort, listenOptions =>
                   {
                       listenOptions.Protocols = HttpProtocols.Http2;
                   });

               })
               .ConfigureAppConfiguration(x => x.AddConfiguration(configuration))
               .UseStartup<Startup>()
               .UseContentRoot(Directory.GetCurrentDirectory())
               .Build();

        private static IConfiguration GetConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            var config = builder.Build();

            if (config.GetValue<bool>("UseVault", false))
            {
               
            }

            return builder.Build();
        }

        private static (int httpPort, int grpcPort) GetDefinedPorts(IConfiguration config)
        {
            var grpcPort = config.GetValue("GRPC_PORT", 5001);
            var port = config.GetValue("PORT", 80);
            return (port, grpcPort);
        }
    }
}
