using Disqord;
using Disqord.Bot;
using Disqord.Bot.Prefixes;
using Microsoft.Extensions.Logging;
using Qmmands;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Disqord.Extensions.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using Pahoe;

namespace The_Milkman
{
    public class Milkman : DiscordBot
    {
        private readonly ILogger _logger;

        public Milkman(TokenType tokenType, string token, IPrefixProvider prefixProvider, ILogger logger, DiscordBotConfiguration configuration = null) : base(tokenType, token, prefixProvider, configuration)
        {
            _logger = logger;
            AddModules(Assembly.GetExecutingAssembly());
            this.CommandExecutionFailed += OnFailed;
        }

        private Task OnFailed(CommandExecutionFailedEventArgs e)
        {
            _logger.Log(LogLevel.Warning, $"{e.Result.Command.Name} failed because: " + e.Result.Exception.ToString());
            return Task.CompletedTask;
        }

        protected override ValueTask<bool> CheckMessageAsync(CachedUserMessage message)
            => message.Author.IsBot || (message.Channel is IDmChannel) ? new ValueTask<bool>(false) : new ValueTask<bool>(true);

        protected override ValueTask<DiscordCommandContext> GetCommandContextAsync(CachedUserMessage message, IPrefix prefix)
            => new ValueTask<DiscordCommandContext>(new MilkmanCommandContext(message, this, prefix));

        public override async Task RunAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            var client = this.GetService<LavalinkClient>();
            await AddExtensionAsync(new InteractivityExtension());
            this.Ready += async e =>
            {
                await client.StartAsync();
                _logger.Log(LogLevel.Information, "lavalink client initialized");
            };
            await base.RunAsync(cancellationToken);
        }
    }
}
