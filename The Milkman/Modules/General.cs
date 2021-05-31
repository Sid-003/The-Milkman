using Qmmands;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq;
using System;
using System.IO;
using Disqord;
using Disqord.Bot;

namespace The_Milkman.Modules
{
    public class General : DiscordModuleBase<MilkmanCommandContext>
    {
        [Command("ping")]
        public DiscordCommandResult Ping()
        {
            return Reply("pong your mother");
        }

        private static Task<bool> StartProcessAsync(string name, string args)
        {
            var tcs = new TaskCompletionSource<bool>();

            var startInfo = new ProcessStartInfo()
            {
                FileName = name,
                Arguments = args,
                CreateNoWindow = false,
                UseShellExecute = false,
            };


            var p = new Process
            {
                StartInfo = startInfo,
                EnableRaisingEvents = true
            };

            p.Exited += (s, e) =>
            {
                _ = tcs.TrySetResult(true);
                p.Dispose();
            };

            p.Start();
            _ = Task.Run
            (
                async () =>
                {
                    await Task.Delay(20000);
                    _ = tcs.TrySetCanceled();
                }
            );

            return tcs.Task;
        }


        [Command("stickbug")]
        public async Task<DiscordResponseCommandResult> StickbugAsync(string outputname = "output")
            => await MergeVideo("video", outputname);

        [Command("hwurmp")]
        public async Task<DiscordResponseCommandResult> HrumpwAsync(string outputname = "output")
            => await MergeVideo("hrump", outputname);


        [Command("souptime")]
        public async Task<DiscordResponseCommandResult> SouptimeAsync(string outputname = "output")
            => await MergeVideo("lizard", outputname);


        private async Task<DiscordResponseCommandResult> MergeVideo(string video, string outputname)
        {
            var url = Context.Message.Attachments.FirstOrDefault()?.Url;

            if (url is null)
                throw new Exception("lmao idiot what are you doing");

            string extension = url.Substring(url.LastIndexOf('.') + 1);
            string fileName = $"image.{extension}";

            await using var stream = await Context.HttpClient.GetStreamAsync(url);

            var ms = new MemoryStream();
            await stream.CopyToAsync(ms);

            await File.WriteAllBytesAsync(fileName, ms.ToArray());


            string convertString = extension switch
            {
                "mp4" => "",
                "gif" => $"-y -i {fileName} -movflags faststart -pix_fmt yuv420p -vf scale=1280:-2 image.mp4",
                _ => $"-y -loop 1 -framerate 30 -i {fileName} -c:v libx264 -t 0.01 -pix_fmt yuv420p -vf scale=1280:-2 image.mp4"
            };

            string[] args =
            {
                convertString,
                "-y -i image.mp4 -c copy -bsf:v h264_mp4toannexb -f mpegts image.ts",
                $"-y -i {video}.mp4 -c copy -bsf:v h264_mp4toannexb -f mpegts video.ts",
                "-y -i \"concat:image.ts|video.ts\" -c copy -bsf:a aac_adtstoasc output.mp4"
            };

            const string ffmpeg = "ffmpeg.exe";
            foreach (string t in args)
            {
                //ideally you would try catch and return a proper command result, too lazy lmao
                if (!(await StartProcessAsync(ffmpeg, t)))
                {
                    throw new Exception("something went wrong, blame yourself");
                }
            }

            var fs = new MemoryStream(await File.ReadAllBytesAsync("output.mp4"));

            return Response(new LocalMessageBuilder()
                .WithAttachments(new LocalAttachment(fs, outputname + ".mp4"))
                .Build());
        }
    }
}