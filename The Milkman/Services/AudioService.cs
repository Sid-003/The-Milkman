using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Pahoe;
using Pahoe.Search;
using The_Milkman.Collections;
using The_Milkman.Extensions;

namespace The_Milkman.Services
{
    public class AudioService
    {
        private readonly ConcurrentDictionary<ulong, MusicQueue> _queue = new ConcurrentDictionary<ulong, MusicQueue>();

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
            _ = _queue.TryAdd(guildId, new MusicQueue());

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

        public bool Contains(ulong guildId, LavalinkTrack track) => _queue[guildId].Contains(track);

        public void PopQueue(ulong guildId, int index)
        {
            _queue[guildId].TryPopAt(index, out _);
        }
        
        public void InsertTrack(ulong guildId, int index, LavalinkTrack track)
        {
            _queue[guildId].Insert(index, track);
        }

        public (int, string) GetQueue(ulong guildId)
        {
            var q = _queue[guildId];
            return (q.GetTotalLength(), _queue[guildId].ToString());
        }
        
    }
}