using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Disqord;
using Disqord.Extensions.Interactivity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pahoe;
using Qmmands;
using The_Milkman.Logging;
using The_Milkman.Services;

namespace The_Milkman
{
    internal class Program
    {
        private static async Task Main(string[] args)
            => await new Program().StartAsync();


        private async Task StartAsync()
        {
            var provider = ConfigureServices();
            var configuration = provider.GetService<IConfigurationRoot>();
            
            var loggerFactory = provider.GetService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("The Milkman");
            
            var prefixProvider = new MilkmanPrefixProvider();

            var milkman = new Milkman
            (
                TokenType.Bot,
                configuration["token"],
                prefixProvider,
                logger,
                new DiscordBotConfiguration()
                {
                    MessageCache = null,
                    ProviderFactory = _ => provider,
                    Activity = new LocalActivity("https://youtu.be/-knOGoRYhE0", ActivityType.Watching),
                    CommandServiceConfiguration = new CommandServiceConfiguration()
                    {
                        IgnoresExtraArguments = true,
                    },
                    Logger = new Optional<Disqord.Logging.ILogger>(new DisqordLogger(logger)),
                }
            );

            await milkman.RunAsync();
        }
        
        private IServiceProvider ConfigureServices()
            => new ServiceCollection()
                    .AddSingleton(new ConfigurationBuilder()
                                            .SetBasePath(Directory.GetCurrentDirectory())
                                            .AddJsonFile("config.json")
                                            .AddJsonFile("lavalink.json")
                                            .Build()
                    )
                    .AddSingleton(LoggerFactory.Create(logging =>
                                       {
                                           logging.AddConsole();
                                           logging.AddDebug();
                                       })
                    )
                    .AddSingleton(
                        provider =>
                        {
                            var configuration = provider.GetService<IConfigurationRoot>();
                            var client = provider.GetService<Milkman>();
                            
                            var lavalinkClient = new LavalinkClient(client, new LavalinkConfiguration()
                            {
                                Address = configuration["address"],
                                Authorization = configuration["auth"],
                                Port = configuration.GetValue<int>("port"),
                                SelfDeaf = false
                            });

                            return lavalinkClient;
                        })
                    .AddSingleton<HttpClient>()
                    .AddSingleton<AudioService>()
                    .BuildServiceProvider();



        
        
        
    }
}