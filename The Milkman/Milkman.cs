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
using The_Milkman.Modules;
using The_Milkman.Services;
using The_Milkman.TypeParsers;

namespace The_Milkman
{
    public class Milkman : DiscordBot
    {
        private readonly ILogger _logger;
        

        public Milkman(TokenType tokenType, string token, IPrefixProvider prefixProvider, ILogger logger, DiscordBotConfiguration configuration = null) : base(tokenType, token, prefixProvider, configuration)
        {
            _logger = logger;
            AddTypeParser(new LavalinkTrackParser());
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
            this.VoiceStateUpdated += async e =>
            {
                var user = e.Member;
                var ownerId = (await e.Client.GetCurrentApplicationAsync()).Owner.Id;
                if (user.Id != ownerId || user.Guild.CurrentMember.VoiceChannel is null)
                    return;
                
                if (e.Member.VoiceChannel?.Id != e.OldVoiceState.ChannelId)
                    await e.Client.UpdateVoiceStateAsync(user.Guild.Id, e.NewVoiceState.ChannelId, false, true);
            };
            await base.RunAsync(cancellationToken);
        }
    }
}
