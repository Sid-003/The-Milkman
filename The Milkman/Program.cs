using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Disqord.Bot;
using Disqord.Bot.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Qmmands;
using The_Milkman.Services;

namespace The_Milkman
{
    internal static class Program
    {
        private static async Task Main(string[] args)
            => await new HostBuilder()
                .ConfigureAppConfiguration((configBuilder) =>
                {
                    configBuilder.SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("config.json")
                        .AddJsonFile("lavalink.json")
                        .Build();
                })
                .ConfigureLogging(x => { x.AddConsole(); })
                .ConfigureServices((context, collection) =>
                {
                    collection
                        .AddSingleton<VoiceRecognitionService>()
                        .AddSingleton<HttpClient>()
                        .AddPrefixProvider<MilkmanPrefixProvider>()
                        .AddCommands(config => { config.DefaultRunMode = RunMode.Parallel; })
                        .BuildServiceProvider();
                })
                .ConfigureDiscordBot<Milkman>((context, bot) => { bot.Token = context.Configuration["token"]; })
                .RunConsoleAsync();
    }
}