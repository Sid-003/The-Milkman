﻿using Qmmands;
using System;
using System.Threading.Tasks;
using The_Milkman.Attributes.Preconditions;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Scripting;
using System.Linq;
using System.Diagnostics;
using Disqord;

namespace The_Milkman.Modules
{
    public class Globals
    {
        public Milkman Bot { get; set; }
        public MilkmanCommandContext Context { get; set;}
        public DiscordClientBase Client => Context.Message.Client;
    }

    [RequireOwner]
    public class Owner : ModuleBase<MilkmanCommandContext>
    {

        private readonly ScriptOptions _options = ScriptOptions.Default
                                                      .AddReferences(AppDomain.CurrentDomain.GetAssemblies().Where(x => !x.IsDynamic && !string.IsNullOrWhiteSpace(x.Location)))
                                                      .AddImports("System", "System.IO", "The_Milkman", "System.Linq");

        [Command("eval")]
        public async Task EvalAsync([Remainder]string code) 
        {
            int nlIndex = code.IndexOf('\n') + 1;
            int length = code.Length - 4 - nlIndex;
            code = code.Substring(nlIndex, length);

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
                Title = "Evaluattion succesful",
                Color = Disqord.Color.GreenYellow,
                Footer = new LocalEmbedFooterBuilder()
                {
                    Text = $"Compilation Time: {compileTimer.ElapsedMilliseconds} ms | Execution Time: {executionTimer.ElapsedMilliseconds}"
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
