using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Catalog.API.Infrastructure;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Catalog.API.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Catalog.API.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;

namespace Catalog.API
{
    public class Program
    {
        public static readonly string Namespace = typeof(Program).Namespace;
        public static readonly string AppName = Namespace.Substring(Namespace.LastIndexOf('.', Namespace.LastIndexOf('.') - 1) + 1);

        //public static void Main(string[] args)
        //{
        //    CreateHostBuilder(args).Build().Run();
        //}

        //public static IHostBuilder CreateHostBuilder(string[] args) =>
        //    Host.CreateDefaultBuilder(args)
        //        .ConfigureWebHostDefaults(webBuilder =>
        //        {
        //            webBuilder.UseStartup<Startup>();
        //        });


        public static async Task Main(string[] args)
        {
            var host = CreateWebHostBuilder(args).Build();

            using (var serviceScope = host.Services.CreateScope())
            {
                var dbContext = serviceScope.ServiceProvider.GetRequiredService<CatalogContext>();

                await dbContext.Database.MigrateAsync();

                var env = serviceScope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
                var settings = serviceScope.ServiceProvider.GetRequiredService<IOptions<CatalogSettings>>();
                var logger = serviceScope.ServiceProvider.GetRequiredService<ILogger<CatalogContextSeed>>();

                new CatalogContextSeed()
                    .SeedAsync(dbContext, env, settings, logger)
                    .Wait();
            }

            await host.RunAsync();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }


}
