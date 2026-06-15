using Ffmpeg.Command.Commands;
using FFmpeg.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FFmpeg.Core.Models;

namespace FFmpeg.Infrastructure.Commands
{
    public class ReverseVideoCommand : BaseCommand, ICommand<ReverseVideoModel>
    {
        public ReverseVideoCommand(FFmpegExecutor executor, ICommandBuilder commandBuilder) : base(executor)
        {
            CommandBuilder = commandBuilder;
        }
        public async Task<CommandResult> ExecuteAsync(ReverseVideoModel model)
        {
            CommandBuilder
                .SetInput(model.InputFile)    // זה מחליף את ה- "ffmpeg -i input.mp4" ומביא את הקובץ האמיתי
                .AddOption("-vf reverse")     // זה הלב של הפקודה מהמשימה שלך!
                .SetOutput(model.OutputFile);  // זה מחליף את ה- "output.mp4" ומייצר את הקובץ הסופי

            return await RunAsync();
        }
    }
}
