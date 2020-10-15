using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Pahoe.Search;
using Qmmands;
using The_Milkman.Services;

namespace The_Milkman.TypeParsers
{
    public class LavalinkTrackParser : TypeParser<LavalinkTrack>
    {
        public override async ValueTask<TypeParserResult<LavalinkTrack>> ParseAsync(Parameter parameter, string value, CommandContext context)
        {
            if(!(context is MilkmanCommandContext ctx))
                return TypeParserResult<LavalinkTrack>.Unsuccessful("Invalid context provided.");
            
            var client = ctx.Bot.GetService<AudioService>().Client;
            
            
            var track = (await client.SearchAsync(value)).Tracks.FirstOrDefault(x => x.Length < TimeSpan.FromMinutes(15));
            track ??= (await client.SearchYouTubeAsync(value)).Tracks.FirstOrDefault(x => x.Length < TimeSpan.FromMinutes(15));
            
            if (track is null)
                return TypeParserResult<LavalinkTrack>.Unsuccessful("Track not found.");
            
            return track.Length.TotalMinutes > 15 && !track.IsStream ? TypeParserResult<LavalinkTrack>.Unsuccessful("track too long just like 😳") : TypeParserResult<LavalinkTrack>.Successful(track);
        }
    }
}