using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Pahoe;
using Pahoe.Search;
using The_Milkman.Extensions;

namespace The_Milkman.Services
{
    public class AudioService
    {
        private readonly ConcurrentDictionary<ulong, List<LavalinkTrack>> _queue = new ConcurrentDictionary<ulong, List<LavalinkTrack>>();

        public LavalinkClient Client { get; }

        public bool IsReady => Client.IsReady;

        public AudioService(LavalinkClient client)
        {
            Client = client;
        }
        

        public async Task<LavalinkPlayer> GetOrConnectAsync(ulong guildId, CachedVoiceChannel vc)
        {
            if (Client.TryGetPlayer(guildId, out LavalinkPlayer player)) return player;
            
            player = await Client.ConnectAsync(vc);
            _ = _queue.TryAdd(guildId, new List<LavalinkTrack>());

            player.TrackEnded += async args =>
            {
                player.State = PlayerState.Idle;
                if (!_queue[guildId].TryPopAt(0, out LavalinkTrack nextTrack))
                    return;

                await PlayTrackAsync(player, nextTrack);
            };

            return player;
        }
        
        public async Task PlayTrackAsync(LavalinkPlayer player, LavalinkTrack track)
        {
            if (player.State == PlayerState.Playing || player.State == PlayerState.Paused)
            {
                var queue = _queue[player.GuildId];
                queue.Add(track);
                return;
            }

            await player.PlayAsync(track);
        }

        public bool ContainsTrack(ulong guilldId, LavalinkTrack track)
            =>  _queue.TryGetValue(guilldId, out var tracks) && tracks.Contains(track);

        public void PopQueue(ulong guildId, int index)
        {
            _queue[guildId].TryPopAt(index, out _);
        }

        public string GetQueue(ulong guildId)
        {
            return Markdown.CodeBlock(string.Join("\n", _queue[guildId].Select((x, i) => $"{i}. [{x.Title}]")));
        }
        
    }
}