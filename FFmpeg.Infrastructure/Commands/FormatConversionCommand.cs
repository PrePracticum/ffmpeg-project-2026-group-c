using FFmpeg.Core.Models;
using FFmpeg.Infrastructure.Commands;
using FFmpeg.Infrastructure.Services;
using System;
using System.Threading.Tasks;

namespace Ffmpeg.Command.Commands
{
    public class FormatConversionCommand : BaseCommand, ICommand<FormatConversionModel>
    {
        private readonly ICommandBuilder _commandBuilder;

        public FormatConversionCommand(FFmpegExecutor executor, ICommandBuilder commandBuilder)
            : base(executor)
        {
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        public async Task<CommandResult> ExecuteAsync(FormatConversionModel model)
        {
            // FFmpeg command built: ffmpeg -i input.mp4 output.avi
            CommandBuilder = _commandBuilder
                .SetInput(model.InputFile);

            // הגדרת קובץ הפלט. הפרמטר השני קובע האם זה אאודיו בלבד (false אומר שזה וידאו)
            CommandBuilder.SetOutput(model.OutputFile, false);

            return await RunAsync();
        }
    }
}