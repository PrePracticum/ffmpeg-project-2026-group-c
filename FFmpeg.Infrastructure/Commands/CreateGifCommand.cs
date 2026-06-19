using FFmpeg.Core.Models;
using FFmpeg.Infrastructure.Services;
using Ffmpeg.Command.Commands;

namespace FFmpeg.Infrastructure.Commands
{
    public class CreateGifCommand : BaseCommand, ICommand<CreateGifModel>
    {
        public CreateGifCommand(FFmpegExecutor executor, ICommandBuilder commandBuilder) : base(executor)
        {
            CommandBuilder = commandBuilder;
        }

        public async Task<CommandResult> ExecuteAsync(CreateGifModel model)
        {
            CommandBuilder
                .SetInput(model.InputFile)
                .AddOption("-vf \"fps=10,scale=320:-1\"") // הפקודה הייעודית ליצירת GIF
                .SetOutput(model.OutputFile);

            return await RunAsync();
        }
    }
}
