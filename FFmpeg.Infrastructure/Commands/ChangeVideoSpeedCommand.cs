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
    public class ChangeVideoSpeedCommand : BaseCommand, ICommand<ChangeVideoSpeedModel>
    {
        public ChangeVideoSpeedCommand(FFmpegExecutor executor, ICommandBuilder commandBuilder) : base(executor)
        {
            CommandBuilder = commandBuilder;
        }

        public async Task<CommandResult> ExecuteAsync(ChangeVideoSpeedModel model)
        {
            double setPtsValue = 1.0 / model.Speed;

            CommandBuilder
                .SetInput(model.InputFile)
                .AddOption($"-vf \"setpts={setPtsValue}*PTS\"")
                .SetOutput(model.OutputFile);

            return await RunAsync();
        }
    }
}
