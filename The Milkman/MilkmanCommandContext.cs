using System;
using Disqord;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using Disqord.Bot;
using Disqord.Gateway;

namespace The_Milkman
{
    public class MilkmanCommandContext : DiscordGuildCommandContext
    {
        public new Milkman Bot { get; }

        public HttpClient HttpClient { get; }

        public MilkmanCommandContext(IGatewayUserMessage message, string input, Milkman bot, IPrefix prefix) : base(bot, prefix, input, message, message.GetChannel(), bot.Services)
            => (Bot, HttpClient) = (bot, bot.Services.GetRequiredService<HttpClient>());
    }
}