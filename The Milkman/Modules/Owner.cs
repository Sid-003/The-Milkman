using Qmmands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using The_Milkman.Attributes.Preconditions;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Scripting;
using System.Linq;
using System.Diagnostics;
using Disqord;
using Disqord.Bot;
using The_Milkman.Extensions;

namespace The_Milkman.Modules
{
    public class Globals
    {
        public Milkman Bot { get; set; }
        public MilkmanCommandContext Context { get; set;}
        public DiscordClientBase Client => Context.Message.Client;
        
        public async Task<IUserMessage> ReplyAsync(string message = null)
            => await Context.Channel.SendMessageAsync(message);
    }

    [RequireOwner]
    public class Owner : DiscordModuleBase<MilkmanCommandContext>
    {
        private static readonly IEnumerable<string> IMPORTS = new[]
        {
            "System",
            "System.Collections",
            "System.Reflection",
            "System.Threading.Tasks",
            "System.Linq",
            "System.IO",

            "The_Milkman",
            "The_Milkman.Services",

            "Disqord",
            "Disqord.Bot",

            "Qmmands",
            
            "Microsoft.Extensions.DependencyInjection",
        
            "Newtonsoft.Json"
        };
        
        private readonly ScriptOptions _options = ScriptOptions.Default
                                                               .AddReferences(AppDomain.CurrentDomain.GetAssemblies().Where(x => !x.IsDynamic && !string.IsNullOrWhiteSpace(x.Location)))
                                                               .AddImports(IMPORTS);


        [Command("eval")]
        public async Task EvalAsync([Remainder]string code) 
        {
            code = SanityUtilities.ExtractCode(code);
            var script = CSharpScript.Create(code, _options, typeof(Globals));
            var compileTimer = Stopwatch.StartNew();
            var diagnostics = script.Compile();
            compileTimer.Stop();
            var errors = diagnostics.Where(x => x.Severity == DiagnosticSeverity.Error).Select(x => $"Error {x.Id} at {x.Location.GetLineSpan()}\nMessage: {x.GetMessage()}").ToArray();

            if (errors.Length != 0) 
            {
                var embed = new LocalEmbedBuilder()
                {
                    Title = "Script failed to compile",
                    Description = string.Join("\n", errors),
                    Color = Disqord.Color.Red
                };

                await Context.Channel.SendMessageAsync(embed: embed.Build());
                return;
            }

            compileTimer.Stop();
            var executionTimer = Stopwatch.StartNew();
            var result = await script.RunAsync(new Globals()
            {
                Bot = Context.Bot,
                Context = Context
            });
            executionTimer.Stop();


            var resultEmbed = new LocalEmbedBuilder()
            {
                Title = "Evaluation sucessful",
                Color = Disqord.Color.GreenYellow,
                Footer = new LocalEmbedFooterBuilder()
                {
                    Text = $"Compilation Time: {compileTimer.ElapsedMilliseconds} ms | Execution Time: {executionTimer.ElapsedMilliseconds} ms"
                } 
            };

            var value = result.ReturnValue;
            if (value is null)
                resultEmbed.Description = "null";
            else
                resultEmbed.AddField(value.GetType().ToString(), value);
            await Context.Channel.SendMessageAsync(embed: resultEmbed.Build());
        }
    }
}
