using Disqord;
using Disqord.Bot;
using Disqord.Bot.Prefixes;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;

namespace The_Milkman
{
    public class MilkmanCommandContext : DiscordCommandContext
    {        
        public new Milkman Bot { get; }

        public HttpClient HttpClient { get; }

        public MilkmanCommandContext(CachedUserMessage message, Milkman bot, IPrefix prefix) : base(bot, prefix, message, bot)
            => (Bot, HttpClient) = (bot, bot.GetRequiredService<HttpClient>());
    }
}
