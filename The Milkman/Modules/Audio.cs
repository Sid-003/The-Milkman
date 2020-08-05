using System;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Events;
using Disqord.Extensions.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using Pahoe;
using Qmmands;
using Pahoe.Search;
using The_Milkman.Attributes.Preconditions;
using The_Milkman.Services;


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
        public async Task PlayAsync([Remainder]LavalinkTrack track)
        {
            var player = await _audio.GetOrConnectAsync(Context.Guild.Id, Context.Member.VoiceChannel);
            if (Context.Member.VoiceChannel != player.Channel)
            {
                await ReplyAsync("join the correct vc idiot");
                return;
            }

            ulong guildId = Context.Guild.Id;

            if (_audio.ContainsTrack(guildId, track))
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
                var reaction =  await Context.Channel.WaitForReactionAsync(e =>
                {
                    if (e.Message.Id != Context.Message.Id || !e.Reaction.HasValue || !e.Emoji.Equals(new LocalEmoji("✅")))
                        return false;
                    
                    return e.Reaction.Value.Count >= (player.Channel.Members.Count /  2);
                }, TimeSpan.FromSeconds(15));
                
                if (reaction is null)
                {
                    await ReplyAsync("not skipping idiot");
                    return;
                }
            }
            
            await ReplyAsync("skipped");
            
            await player.StopAsync();
        }

        [Command("queue", "q")]
        public async Task QueueAsync()
        {
            await ReplyAsync(_audio.GetQueue(Context.Guild.Id));
        }

        [Command("nowplaying", "np")]
        public async Task ShowCurrentTrackAsync()
        {
            var player = await _audio.GetOrConnectAsync(Context.Guild.Id, Context.Member.VoiceChannel);

            LavalinkTrack track = player.Track;
            
            if(player.State == PlayerState.Playing)
                await ReplyAsync($"[{track.Title}] ({player.Position} / {track.Length})");
            else
            {
                await ReplyAsync("nothing playing");
            }
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

        [Command("pause")]
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