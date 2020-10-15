using System;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Pahoe;
using Qmmands;
using Pahoe.Search;
using The_Milkman.Attributes.Preconditions;
using The_Milkman.Services;
using The_Milkman.TypeParsers;


namespace The_Milkman.Modules
{
    public class Audio : DiscordModuleBase<MilkmanCommandContext>
    {
        private readonly AudioService _audio;

        public Audio(AudioService audio)
        {
            _audio = audio;
        }

        
        //need to change this to use CommandResult shit instead of ReplyAsync everywhere

        [Command("play")]
        public async Task PlayAsync()
        {
            var parser = Context.Bot.GetSpecificTypeParser<LavalinkTrack, LavalinkTrackParser>();
            
            var attachments = Context.Message.Attachments;
            if (attachments.Count == 0)
            {
                await ReplyAsync("no atttachments");
                return;
            }


            var result = await parser.ParseAsync(null, attachments.First().Url, Context);

            if (!result.HasValue)
            {
                await ReplyAsync("invalid track lamo");
                return;
            }

            await PlayAsync(result.Value);
        }


        [Command("play")]
        public async Task PlayAsync([Remainder]LavalinkTrack track)
        {
            var player = await _audio.GetOrConnectAsync(Context.Guild.Id, Context.Member.VoiceChannel);
            if (Context.Member.VoiceChannel != player.Channel)
            {
                await ReplyAsync("join the correct vc idiot");
                return;
            }

            ulong guildId = Context.Guild.Id;

            if  (_audio.Contains(guildId, track) || track.Equals(player.Track))
            {
                await ReplyAsync("already has the track");
                return;
            }
            
            await _audio.PlayTrackAsync(player, track);
            await ReplyAsync("queued the track: " + track.Title);
        }

        [Command("skip")]
        [RunMode(RunMode.Parallel)]
        public async Task SkipAsync()
        {
            var player = await _audio.GetOrConnectAsync(Context.Guild.Id, Context.Member.VoiceChannel);
            var ownerId = (await Context.Bot.GetCurrentApplicationAsync()).Owner.Id;
            if (Context.User.Id != ownerId)
            {
                var msg = Context.Message;
                var yes = new LocalEmoji("✅");
                var no = new LocalEmoji("❌");
                var god = new LocalEmoji("🙂");
                await msg.AddReactionAsync(yes);
                await msg.AddReactionAsync(no);
                await msg.AddReactionAsync(god);

                await Task.Delay(TimeSpan.FromSeconds(5));

                var updatedMsg = await Context.Channel.GetMessageAsync(msg.Id);

                int yesCount = (await updatedMsg.GetReactionsAsync(yes)).Select(x => Context.Guild.GetMember(x.Id)).Count(x => x?.VoiceChannel == player.Channel);
                int noCount = (await updatedMsg.GetReactionsAsync(no)).Select(x => Context.Guild.GetMember(x.Id)).Count(x => x?.VoiceChannel == player.Channel);

          
                if (noCount >= yesCount || (await updatedMsg.GetReactionsAsync(god)).Any(x => x.Id == ownerId))
                {
                    await ReplyAsync("not skipping idiot");
                    return;
                }
            }
            
            await ReplyAsync("skipped");
            
            await player.StopAsync();
        }

        [Command("queue", "q")]
        public async Task Queue()
        {
            (int l, string q) = _audio.GetQueue(Context.Guild.Id);
            var embed = new LocalEmbedBuilder()
                        .WithTitle("Queue")
                        .WithDescription(q is ""? "empty" : q)
                        .WithColor(new Color(236, 192, 255))
                        .WithFooter($"Total Length: {(l / 60):00}:{(l % 60):00}")
                        .Build();

            await ReplyAsync(embed: embed);
        }

        [Command("nowplaying", "np", "currenttrack", "ct")]
        public async Task ShowCurrentTrackAsync()
        {
            var player = await _audio.GetOrConnectAsync(Context.Guild.Id, Context.Member.VoiceChannel);

            LavalinkTrack track = player.Track;

            if (player.State == PlayerState.Playing)
            {
                var embed = new LocalEmbedBuilder()
                    .WithTitle("Current Track")
                    .WithDescription($"[{track.Title}]({track.Uri})")
                    .WithColor(new Color(236, 192, 255))
                    .AddField("Volume", player.Volume, true)
                    .AddField("Position", $"{player.Position:mm\\:ss} / {track.Length:mm\\:ss}", true)
                    .Build();

                await ReplyAsync(embed: embed);
            }
            else
            {
                await ReplyAsync("nothing playing");
            }
        }


        [Command("volume")]
        public async Task VolumeAsync(ushort vol)
        {
            vol = Math.Clamp(vol, (ushort)0, (ushort)100);

            var player = await _audio.GetOrConnectAsync(Context.Guild.Id, Context.Member.VoiceChannel);

            await player.SetVolumeAsync(vol);
        }
        
        
        [Command("pop")]
        [RequireOwner]
        public void Pop(int index)
        {
            _audio.PopQueue(Context.Guild.Id, index);
        }
        
        [Command("insert")]
        [RequireOwner]
        public void Insert(int index, [Remainder]LavalinkTrack track)
        {
            _audio.InsertTrack(Context.Guild.Id, index, track);
        }

        [Command("pause", "unpause", "resume")]
        public async Task PauseAsync()
        {
            var player = await _audio.GetOrConnectAsync(Context.Guild.Id, Context.Member.VoiceChannel);

            switch (player.State)
            {
                case PlayerState.Paused:
                    await player.ResumeAsync();
                    return;
                case PlayerState.Playing:
                    await player.PauseAsync();
                    return;
                default: 
                    return;
            }
        }
        
        

    }
}