using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using SnomedToSQLite.Menu;
using SnomedToSQLite.Menu.ConvertRf2ToSQLite;
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
                            services.AddScoped<IFileFinder, FileFinder>();
                            services.AddScoped<IImportService, ImportService>();
                            services.AddScoped<ISQLiteDatabaseService, SQLiteDatabaseService>();
                            services.AddScoped<ISqlDataAccess, SQLiteDataAccess>();
                            services.AddScoped<IConvertRf2ToSQLiteRunner,  ConvertRf2ReleaseToSQLiteRunner>();
                            services.AddTransient<IGraphProcessingService,  GraphProcessingService>();
                            services.AddTransient<IConversionHelper, ConversionHelper>();
                            services.AddSingleton<MenuOptions>();
                            services.AddSingleton<IMenuOption, ConvertRf2FullReleaseToSQLiteOption>();
                            services.AddSingleton<IMenuOption, ConvertRf2SnapshotReleaseToSQLiteOption>();
                            services.AddSingleton<IConnectionStringService, ConnectionStringService>();
                            //services.Configure<MySettings>(context.Configuration.GetSection("MySettings"));
                            //services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<MySettings>>().Value);
                        })
                        .Build();

            using var serviceScope = host.Services.CreateScope();
            var services = serviceScope.ServiceProvider;


            try
            {
                var menuOptions = services.GetRequiredService<MenuOptions>();
                var menu = new MenuUI(menuOptions.GetOrderedMenuOptions());

                await menu.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
        }
    }
}