using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Disqord.Bot;
using Disqord.Gateway;

namespace The_Milkman
{
    class MilkmanPrefixProvider : IPrefixProvider
    {
        public ValueTask<IEnumerable<IPrefix>> GetPrefixesAsync(IGatewayUserMessage message)
            => new(new[]
            {
                new StringPrefix("mm!", StringComparison.InvariantCultureIgnoreCase),
                new StringPrefix("amogus!", StringComparison.InvariantCulture),
            });
    }
}