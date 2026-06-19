using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ffmpeg.Command.Commands;
using FFmpeg.Core.Models;
using FFmpeg.Infrastructure.Services;

namespace FFmpeg.Infrastructure.Commands
{
    public class AudioRemovalCommand : BaseCommand, ICommand<AudioRemovalModel>
    {
        public AudioRemovalCommand(FFmpegExecutor executor, ICommandBuilder commandBuilder) : base(executor)
        {
            CommandBuilder = commandBuilder;
        }

        public async Task<CommandResult> ExecuteAsync(AudioRemovalModel model)
        {
            CommandBuilder
                .SetInput(model.InputFile)
                .AddOption("-an")  
                .SetOutput(model.OutputFile);

            return await RunAsync();
        }
    }
}