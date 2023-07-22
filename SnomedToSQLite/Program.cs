using System.Runtime;
using System.Text;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using SnomedToSQLite.Services;

using SqliteLibrary;

namespace SnomedToSQLite
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                        .ConfigureAppConfiguration((hostingContext, configuration) =>
                        {
                            configuration.Sources.Clear();

                            var env = hostingContext.HostingEnvironment;

                            configuration
                                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true);

#if DEBUG
                            configuration.AddUserSecrets<Program>();
#endif

                        })
                        .ConfigureServices((context, services) =>
                        {
                            services.AddScoped<IImportService, ImportService>();
                            services.AddScoped<ISQLiteDatabaseService, SQLiteDatabaseService>();
                            services.AddScoped<ISqlDataAccess, SQLiteDataAccess>();
                            services.AddTransient<Runner>();
                            //services.Configure<MySettings>(context.Configuration.GetSection("MySettings"));
                            //services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<MySettings>>().Value);
                        })
                        .Build();

            using var serviceScope = host.Services.CreateScope();
            var services = serviceScope.ServiceProvider;

            try
            {
                var runner = services.GetRequiredService<Runner>();

                //When running in visual studio, set the argument in Debug -> properties.

                if (args.Length > 0)
                {
                    await runner.ConvertRf2ToSQLIte(args[0], args[1], args[2]);

                }
                else
                {
                    throw new ArgumentException("Please provide a file path.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }

            await host.RunAsync();
        }
    }
}