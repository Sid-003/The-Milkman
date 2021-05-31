using Qmmands;
using System;
using System.Threading.Tasks;
using Disqord.Rest;

namespace The_Milkman.Attributes.Preconditions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    public sealed class RequireOwnerAttribute : CheckAttribute
    {
        public override async ValueTask<CheckResult> CheckAsync(CommandContext context)
        {
            if (context is not MilkmanCommandContext Context)
                throw new ArgumentException("Invalid context.", nameof(context));

            var application = await Context.Bot.FetchCurrentApplicationAsync();
            return application.Owner.Id == Context.Author.Id ? CheckResult.Successful : new CheckResult("You have to be the owner of this bot to run this command.");
        }
    }
}