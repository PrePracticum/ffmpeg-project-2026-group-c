using Ffmpeg.Command;
using Ffmpeg.Command.Commands;
using FFmpeg.Core.Models;
using FFmpeg.Infrastructure.Commands;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFmpeg.Infrastructure.Services
{
    public interface IFFmpegServiceFactory
    {
        ICommand<WatermarkModel> CreateWatermarkCommand();
        ICommand<ReverseVideoModel> CreateReverseVideoCommand();
        ICommand<ExtractAudioModel> CreateExtractAudioCommand();
        ICommand<BrightnessContrastModel> CreateBrightnessContrastCommand();
        ICommand<ChangeVideoSpeedModel> CreateChangeVideoSpeedCommand();
        ICommand<TextOverlayModel> CreateTextOverlayCommand();
        ICommand<FormatConversionModel> CreateFormatConversionCommand();
        ICommand<AudioEchoModel> CreateAudioEchoCommand();
    }

    public class FFmpegServiceFactory : IFFmpegServiceFactory
    {
        private readonly FFmpegExecutor _executor;
        private readonly ICommandBuilder _commandBuilder;

        public ICommand<ReverseVideoModel> CreateReverseVideoCommand()
        {
            return new ReverseVideoCommand(_executor, _commandBuilder);
        }

        public ICommand<BrightnessContrastModel> CreateBrightnessContrastCommand()
        {
            return new BrightnessContrastCommand(_executor, _commandBuilder);
        }

        public ICommand<ChangeVideoSpeedModel> CreateChangeVideoSpeedCommand()
        {
            return new ChangeVideoSpeedCommand(_executor, _commandBuilder);
        }

        public ICommand<TextOverlayModel> CreateTextOverlayCommand()
        {
            return new TextOverlayCommand(_executor, _commandBuilder);
        }

        public FFmpegServiceFactory(IConfiguration configuration, ILogger logger = null)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string ffmpegPath = Path.Combine(baseDirectory, "external", "ffmpeg.exe");

            bool logOutput = bool.TryParse(configuration["FFmpeg:LogOutput"], out bool log) && log;

            _executor = new FFmpegExecutor(ffmpegPath, logOutput, logger);
            _commandBuilder = new CommandBuilder(configuration);
        }

        public ICommand<WatermarkModel> CreateWatermarkCommand()
        {
            return new WatermarkCommand(_executor, _commandBuilder);
        }
        public ICommand<ExtractAudioModel> CreateExtractAudioCommand()
        {
            return new ExtractAudioCommand(_executor, _commandBuilder);
        }
        public ICommand<FormatConversionModel> CreateFormatConversionCommand()
        {
            return new FormatConversionCommand(_executor, _commandBuilder);
        }
        public ICommand<AudioEchoModel> CreateAudioEchoCommand()
        {
            return new AudioEchoCommand(_executor, _commandBuilder);
        }
    }
}
