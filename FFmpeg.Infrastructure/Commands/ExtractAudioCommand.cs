using Ffmpeg.Command.Commands;
using FFmpeg.Infrastructure.Services;
using FFmpeg.Core.Models;
using FFmpeg.Infrastructure.Commands;

public class ExtractAudioCommand : BaseCommand, ICommand<ExtractAudioModel>
{
    public ExtractAudioCommand(FFmpegExecutor executor, ICommandBuilder commandBuilder) : base(executor)
    {
        CommandBuilder = commandBuilder;
    }

    public async Task<CommandResult> ExecuteAsync(ExtractAudioModel model)
    {
        CommandBuilder
            .SetInput(model.InputFile)
            .AddOption("-q:a 0")
            .AddOption("-map a")
            .SetOutput(model.OutputFile);

        return await RunAsync();
    }
}