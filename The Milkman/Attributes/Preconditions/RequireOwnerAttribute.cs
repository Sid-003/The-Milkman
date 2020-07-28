using Qmmands;
using System;
using System.Threading.Tasks;

namespace The_Milkman.Attributes.Preconditions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    public sealed class RequireOwnerAttribute : CheckAttribute
    {
        public override async ValueTask<CheckResult> CheckAsync(CommandContext context)
        {
            if (!(context is MilkmanCommandContext Context))
                throw new ArgumentException("Invalid context.", nameof(context));

            var application = await Context.Message.Client.GetCurrentApplicationAsync();
            if (application.Owner.Id == Context.User.Id)
                return CheckResult.Successful;

            return new CheckResult("You have to be the owner of this bot to run this command.");
        }
    }
}
