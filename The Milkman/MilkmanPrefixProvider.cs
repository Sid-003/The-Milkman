using Disqord;
using Disqord.Bot.Prefixes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace The_Milkman
{
    class MilkmanPrefixProvider : IPrefixProvider
    {
        public ValueTask<IEnumerable<IPrefix>> GetPrefixesAsync(CachedUserMessage message)
            => new ValueTask<IEnumerable<IPrefix>>(new[]
              {
                new StringPrefix("mm!", StringComparison.InvariantCultureIgnoreCase)
              });
    }
}
