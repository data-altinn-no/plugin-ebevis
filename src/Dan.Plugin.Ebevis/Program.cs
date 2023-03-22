using Dan.Common.Extensions;
using Dan.Plugin.Ebevis.Config;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace Dan.Plugin.Ebevis
{
    class Program
    {
        private static Task Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureDanPluginDefaults()

                .ConfigureAppConfiguration((context, configuration) =>
                {
                    // Add more configuration sources if necessary. ConfigureDanPluginDefaults will load environment variables, which includes
                    // local.settings.json (if developing locally) and applications settings for the Azure Function
                })
                .ConfigureServices((context, services) =>
                {
                    // Add any additional services here

                    // This makes IOption<Settings> available in the DI container.
                    services.Configure<ApplicationSettings>(context.Configuration);

                })
                .Build();

            return host.RunAsync();
        }
    }
}
