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
                .SetInput(model.InputFile)   
                .AddOption("-vf reverse")    
                .SetOutput(model.OutputFile);  

            return await RunAsync();
        }
    }
}
