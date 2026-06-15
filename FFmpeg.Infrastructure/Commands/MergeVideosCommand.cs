using FFmpeg.Core.Models;
using FFmpeg.Infrastructure.Commands;
using FFmpeg.Infrastructure.Services;
using System;
using System.Threading.Tasks;

namespace Ffmpeg.Command.Commands
{
    public class MergeCommand : BaseCommand, ICommand<MergeVideosModel>
    {
        private readonly ICommandBuilder _commandBuilder;

        public MergeCommand(FFmpegExecutor executor, ICommandBuilder commandBuilder)
            : base(executor)
        {
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        public async Task<CommandResult> ExecuteAsync(MergeVideosModel model)
        {
            string filter = model.IsHorizontal ? "hstack" : "vstack";

            CommandBuilder = _commandBuilder
                .SetInput(model.InputFile1)
                .SetInput(model.InputFile2)
                .AddOption($"-filter_complex \"[0:v][1:v]{filter}=inputs=2\"")
                .SetOutput(model.OutputFile);

            return await RunAsync();
        }
    }
}