using System;
using Disqord;
using Disqord.Bot;
using Disqord.Gateway;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace The_Milkman
{
    public class Milkman : DiscordBot
    {
        public Milkman(IOptions<DiscordBotConfiguration> options, ILogger<DiscordBot> logger, IServiceProvider services, DiscordClient client) : base(options, logger, services, client)
        {
            
        }

        public override DiscordCommandContext CreateCommandContext(
            IPrefix prefix,
            string input,
            IGatewayUserMessage message,
            CachedTextChannel channel)
        {
            return new MilkmanCommandContext(message, input, this, prefix);
        }
    }
}