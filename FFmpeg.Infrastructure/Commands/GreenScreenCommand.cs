using System.Diagnostics;
using System.Threading.Tasks;
using FFmpeg.Core.Models;
using Ffmpeg.Command; // בשביל ה-ILogger אם נצטרך אותו בעתיד

namespace FFmpeg.Infrastructure.Commands
{
    public class GreenScreenCommand
    {
        private readonly Ffmpeg.Command.ILogger _logger;
        private readonly string _ffmpegPath;

        public GreenScreenCommand(Ffmpeg.Command.ILogger logger, string ffmpegPath)
        {
            _logger = logger;
            _ffmpegPath = ffmpegPath;
        }

        // שינינו את סוג ההחזרה ל-Task<bool> כדי שנדע אם הפעולה הצליחה
        public async Task<bool> ExecuteAsync(GreenScreenModel model)
        {
            string arguments = $"-i \"{model.InputFile}\" -i \"{model.BackgroundFile}\" " +
                               $"-filter_complex \"[0:v]chromakey=0x00FF00:0.1:0.2[ckout];[1:v][ckout]overlay[out]\" " +
                               $"-map \"[out]\" \"{model.OutputFile}\"";

            // ויתרנו על _logger.Log בגלל השגיאה, והחלפנו בהדפסה רגילה לקונסול
            System.Console.WriteLine($"Running GreenScreen command: ffmpeg {arguments}");

            var processInfo = new ProcessStartInfo
            {
                FileName = _ffmpegPath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = new Process { StartInfo = processInfo })
            {
                process.Start();
                await process.WaitForExitAsync();
                
                return process.ExitCode == 0;
            }
        }
    }
}