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
    public class ExtractFrameCommand : BaseCommand, ICommand<ExtractFrameModel>
    {
        public ExtractFrameCommand(FFmpegExecutor executor, ICommandBuilder commandBuilder) : base(executor)
        {
            CommandBuilder = commandBuilder;
        }

        public async Task<CommandResult> ExecuteAsync(ExtractFrameModel model)
        {
            CommandBuilder
                .SetInput(model.InputFile)
                .AddOption($"-ss {model.TimeStamp}")
                .AddOption("-vframes 1")
                .SetOutput(model.OutputFile);

            return await RunAsync();
        }
    }
}
