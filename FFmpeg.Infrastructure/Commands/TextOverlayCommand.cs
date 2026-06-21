using Ffmpeg.Command.Commands;
using FFmpeg.Core.Models;
using FFmpeg.Infrastructure.Commands;
using FFmpeg.Infrastructure.Services;
using System;
using System.Threading.Tasks;

namespace Ffmpeg.Command.Commands
{
    public class TextOverlayCommand : BaseCommand, ICommand<TextOverlayModel>
    {
        private readonly ICommandBuilder _commandBuilder;

        public TextOverlayCommand(FFmpegExecutor executor, ICommandBuilder commandBuilder)
            : base(executor)
        {
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        public async Task<CommandResult> ExecuteAsync(TextOverlayModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            // Build x expression (animated or fixed)
            string xExpr = model.Animate ? $"t*{model.Speed}" : model.XPosition.ToString();

            // Escape single quotes in text
            string safeText = (model.Text ?? string.Empty).Replace("'", "\\'");

            CommandBuilder = _commandBuilder
                .SetInput(model.InputFile)
                .AddOption($"-vf \"drawtext=text='{safeText}':x={xExpr}:y={model.YPosition}:fontsize={model.FontSize}:fontcolor={model.FontColor}\"")
                // Do not force audio-only mapping; allow ffmpeg to include video stream by default
                .AddOption("-c:a copy")
                .SetOutput(model.OutputFile);

            return await RunAsync();
        }
    }
}
