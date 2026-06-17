using Ffmpeg.Command.Commands;
using FFmpeg.Core.Models;
using FFmpeg.Infrastructure.Commands;
using FFmpeg.Infrastructure.Services;
using System;
using System.Threading.Tasks;

namespace Ffmpeg.Command.Commands
{
    public class AudioEchoCommand : BaseCommand, ICommand<AudioEchoModel>
    {
        private readonly ICommandBuilder _commandBuilder;

        public AudioEchoCommand(FFmpegExecutor executor, ICommandBuilder commandBuilder)
            : base(executor)
        {
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        public async Task<CommandResult> ExecuteAsync(AudioEchoModel model)
        {
            CommandBuilder = _commandBuilder
                .SetInput(model.InputFile)
                .AddOption("-af \"aecho=0.8:0.88:60:0.4\"")
                .AddOption("-c:v copy")
                .AddOption($"-t {model.Duration}")
                .SetOutput(model.OutputFile);

            return await RunAsync();
        }
    }
}
