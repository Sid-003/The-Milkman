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
        public async Task PlayAsync([Remainder] string toSearch)
        {
            if (Context.Member.VoiceChannel is null)
            {
                await ReplyAsync("join vc idiot");
                return;
            }
            
            var track = (await _audio.Client.SearchAsync(toSearch)).Tracks.FirstOrDefault();
            track ??= (await _audio.Client.SearchYouTubeAsync(toSearch)).Tracks.FirstOrDefault();
            if (track is null)
            {
                await ReplyAsync("track not found lmao");
                return;
            }

            if (track.Length.TotalMinutes > 15)
            {
                await ReplyAsync("too long just like 😳");
                return;
            }
            
            
            ulong guildId = Context.Guild.Id;

            if (_audio.ContainsTrack(guildId, track))
            {
                await ReplyAsync("already has the track");
                return;
            }

            var player = await _audio.GetOrConnectAsync(Context.Guild.Id, Context.Member.VoiceChannel);
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
                var reaction =  await Context.Channel.WaitForReactionAsync(e => e.Message.Id == Context.Message.Id && e.User.Id == ownerId, TimeSpan.FromSeconds(15));
                
                if (reaction is null || !reaction.Emoji.Equals(new LocalEmoji("✅")))
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
            await ReplyAsync($"[{track.Title}] ({player.Position} / {track.Length})");
        }

        [Command("pop")]
        [RequireOwner]
        public void PopAsync(int index)
        {
            _audio.PopQueue(Context.Guild.Id, index);
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