using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Disqord;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Qmmands;
using The_Milkman.Logging;

namespace The_Milkman
{
    class Program
    {
        
        static async Task Main(string[] args)
            => await new Program().StartAsync();


        private async Task StartAsync()
        {
            await ConfigureServices().GetService<Milkman>().RunAsync();
        }


        private IServiceProvider ConfigureServices()
            => new ServiceCollection()
                    .AddSingleton(new ConfigurationBuilder()
                                            .SetBasePath(Directory.GetCurrentDirectory())
                                            .AddJsonFile("config.json")
                                            .Build()
                    )
                    .AddSingleton(LoggerFactory.Create(logging =>
                                       {
                                           logging.AddConsole();
                                           logging.AddDebug();
                                       })
                    )
                    .AddSingleton(x =>
                    {
                        var configuration = x.GetService<IConfigurationRoot>();
                        var loggerFactory = x.GetService<ILoggerFactory>();
                        var logger = loggerFactory.CreateLogger("The Milkman");
                        var prefixProvider = new MilkmanPrefixProvider();
                        return new Milkman(TokenType.Bot, configuration["token"], prefixProvider, logger, new DiscordBotConfiguration()
                        {    
                            ProviderFactory = _ => x,
                            Activity = new LocalActivity("https://youtu.be/-knOGoRYhE0", ActivityType.Watching),
                            CommandServiceConfiguration = new CommandServiceConfiguration()
                            {
                                IgnoresExtraArguments = true,
                            },
                            Logger = new Optional<Disqord.Logging.ILogger>(new DisqordLogger(logger)),
                        });


                    })
                    .AddSingleton<HttpClient>()
                    .BuildServiceProvider();



        
        
        
    }
}