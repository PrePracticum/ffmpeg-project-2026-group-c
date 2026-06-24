using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FFmpeg.API.DTOs;
using FFmpeg.Core.Interfaces;
using FFmpeg.Core.Models;
using FFmpeg.Infrastructure.Services;
//using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace FFmpeg.API.Endpoints
{
    public static class VideoEndpoints
    {
        public static void MapEndpoints(this WebApplication app)
        {
            app.MapPost("/api/video/watermark", AddWatermark)
                .DisableAntiforgery()
                .WithMetadata(new RequestSizeLimitAttribute(104857600)); 

            app.MapPost("/api/video/text-overlay", AddTextOverlay)
                .DisableAntiforgery()
                .WithMetadata(new RequestSizeLimitAttribute(104857600)); 

            app.MapPost("/api/video/merge", MergeVideos)
                .DisableAntiforgery()
                .WithMetadata(new RequestSizeLimitAttribute(209715200));
            app.MapPost("/api/video/reverse", ReverseVideo)
                .DisableAntiforgery()
                .WithMetadata(new RequestSizeLimitAttribute(104857600));

            app.MapPost("/api/video/extract-audio", ExtractAudio)
                .DisableAntiforgery()
                .WithMetadata(new RequestSizeLimitAttribute(104857600));

            app.MapPost("/api/video/brightness-contrast", ApplyBrightnessContrast)
                .DisableAntiforgery()
                .WithMetadata(new RequestSizeLimitAttribute(104857600));

            app.MapPost("/api/video/change-speed", ChangeVideoSpeed)
                .DisableAntiforgery()
                .WithMetadata(new RequestSizeLimitAttribute(104857600)); 

            app.MapPost("/api/video/convert", ConvertFormat)
                .DisableAntiforgery()
                .WithMetadata(new RequestSizeLimitAttribute(104857600)); 

            app.MapPost("/api/video/audio-echo", AudioEcho)
                .DisableAntiforgery()
                .WithMetadata(new RequestSizeLimitAttribute(104857600)); 

            app.MapPost("/api/video/remove-audio", RemoveAudio)
                .DisableAntiforgery()
                .WithMetadata(new RequestSizeLimitAttribute(104857600));

            app.MapPost("/api/video/create-gif", CreateGif)
                .DisableAntiforgery()
                .WithMetadata(new RequestSizeLimitAttribute(104857600)); // 100 MB

        }

        private static async Task<IResult> RemoveAudio(
            HttpContext context,
            [FromForm] AudioRemovalDto dto)
        {
            var fileService = context.RequestServices.GetRequiredService<IFileService>();
            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

            var filesToCleanup = new List<string>();

            try
            {
                if (dto.VideoFile == null || dto.VideoFile.Length == 0)
                {
                    return Results.BadRequest("Video file is required.");
                }

                string inputFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);
                filesToCleanup.Add(inputFileName);

                string outputFileName = await fileService.GenerateUniqueFileNameAsync(".mp4");
                filesToCleanup.Add(outputFileName);

                string fullInputPath = fileService.GetFullInputPath(inputFileName);
                string fullOutputPath = fileService.GetFullOutputPath(outputFileName);

                var command = ffmpegService.CreateAudioRemovalCommand();

                var result = await command.ExecuteAsync(new AudioRemovalModel
                {
                    InputFile = fullInputPath,
                    OutputFile = fullOutputPath
                });

                if (!result.IsSuccess)
                {
                    logger.LogError("FFmpeg audio removal command failed: {ErrorMessage}, Command: {Command}",
                        result.ErrorMessage, result.CommandExecuted);

                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);
                    return Results.Problem("Failed to remove audio from video: " + result.ErrorMessage, statusCode: 500);
                }

                byte[] fileBytes = await fileService.GetOutputFileAsync(outputFileName);

                _ = fileService.CleanupTempFilesAsync(filesToCleanup);

                return Results.File(fileBytes, "video/mp4", dto.VideoFile.FileName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in RemoveAudio endpoint");
                _ = fileService.CleanupTempFilesAsync(filesToCleanup);
                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
            }

        }

        private static async Task<IResult> AddTextOverlay(
            HttpContext context,
            [FromForm] TextOverlayDto dto)
        {
            var fileService = context.RequestServices.GetRequiredService<IFileService>();
            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

            var filesToCleanup = new List<string>();

            try
            {
                if (dto.VideoFile == null || dto.VideoFile.Length == 0)
                {
                    return Results.BadRequest("Video file is required.");
                }

                if (string.IsNullOrWhiteSpace(dto.Text))
                {
                    return Results.BadRequest("Text content is required.");

                }

                string inputFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);
                filesToCleanup.Add(inputFileName);

                string outputFileName;
                if (string.IsNullOrWhiteSpace(dto.OutputFileName))
                {
                    // Keep original filename when not provided
                    outputFileName = Path.GetFileName(dto.VideoFile.FileName);
                }
                else
                {
                    // Use provided filename (just the name portion)
                    outputFileName = Path.GetFileName(dto.OutputFileName);
                    // ensure extension exists
                    if (string.IsNullOrWhiteSpace(Path.GetExtension(outputFileName)))
                    {
                        outputFileName += ".mp4";
                    }
                }

                filesToCleanup.Add(outputFileName);

                string fullInputPath = fileService.GetFullInputPath(inputFileName);
                string fullOutputPath = fileService.GetFullOutputPath(outputFileName);

                var command = ffmpegService.CreateTextOverlayCommand();

                var result = await command.ExecuteAsync(new TextOverlayModel
                {
                    InputFile = fullInputPath,
                    OutputFile = fullOutputPath,
                    Text = dto.Text,
                    FontColor = dto.FontColor,
                    FontSize = dto.FontSize,
                    XPosition = dto.XPosition,
                    YPosition = dto.YPosition,
                    Animate = dto.Animate,
                    Speed = dto.Speed
                });

                if (!result.IsSuccess)
                {
                    logger.LogError("FFmpeg text overlay command failed: {ErrorMessage}, Command: {Command}",
                        result.ErrorMessage, result.CommandExecuted);

                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);
                    return Results.Problem("Failed to add text overlay: " + result.ErrorMessage, statusCode: 500);
                }

                byte[] fileBytes = await fileService.GetOutputFileAsync(outputFileName);

                _ = fileService.CleanupTempFilesAsync(filesToCleanup);

                string downloadFileName = string.IsNullOrWhiteSpace(dto.OutputFileName)
                    ? dto.VideoFile.FileName
                    : dto.OutputFileName;

                return Results.File(fileBytes, "video/mp4", downloadFileName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in AddTextOverlay endpoint");
                _ = fileService.CleanupTempFilesAsync(filesToCleanup);
                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
            }
        }
        private static async Task<IResult> ChangeVideoSpeed(
            HttpContext context,
            [FromForm] ChangeVideoSpeedDto dto)
        {
            var fileService = context.RequestServices.GetRequiredService<IFileService>();
            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

            var filesToCleanup = new List<string>();

            try
            {
                if (dto.VideoFile == null || dto.VideoFile.Length == 0)
                {
                    return Results.BadRequest("Video file is required.");
                }

                if (dto.Speed <= 0)
                {
                    return Results.BadRequest("Speed must be greater than 0.");
                }

                string inputFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);
                filesToCleanup.Add(inputFileName);

                string outputFileName = await fileService.GenerateUniqueFileNameAsync(".mp4");
                filesToCleanup.Add(outputFileName);

                string fullInputPath = fileService.GetFullInputPath(inputFileName);
                string fullOutputPath = fileService.GetFullOutputPath(outputFileName);

                var command = ffmpegService.CreateChangeVideoSpeedCommand();

                var result = await command.ExecuteAsync(new ChangeVideoSpeedModel
                {
                    InputFile = fullInputPath,
                    OutputFile = fullOutputPath,
                    Speed = dto.Speed
                });

                if (!result.IsSuccess)
                {
                    logger.LogError("FFmpeg change speed command failed: {ErrorMessage}, Command: {Command}",
                        result.ErrorMessage, result.CommandExecuted);

                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);
                    return Results.Problem("Failed to change video speed: " + result.ErrorMessage, statusCode: 500);
                }

                byte[] fileBytes = await fileService.GetOutputFileAsync(outputFileName);

                _ = fileService.CleanupTempFilesAsync(filesToCleanup);

                return Results.File(fileBytes, "video/mp4", dto.VideoFile.FileName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in ChangeVideoSpeed endpoint");
                _ = fileService.CleanupTempFilesAsync(filesToCleanup);
                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
            }
        }

        private static async Task<IResult> ReverseVideo(
            HttpContext context,
            [FromForm] ReverseVideoDto dto)
        {
            var fileService = context.RequestServices.GetRequiredService<IFileService>();
            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

            var filesToCleanup = new List<string>();

            try
            {
                if (dto.VideoFile == null || dto.VideoFile.Length == 0)
                {
                    return Results.BadRequest("Video file is required.");
                }

                string inputFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);
                filesToCleanup.Add(inputFileName);

                string outputFileName = await fileService.GenerateUniqueFileNameAsync(".mp4");
                filesToCleanup.Add(outputFileName);

                string fullInputPath = fileService.GetFullInputPath(inputFileName);
                string fullOutputPath = fileService.GetFullOutputPath(outputFileName);

                var command = ffmpegService.CreateReverseVideoCommand();

                var result = await command.ExecuteAsync(new ReverseVideoModel
                {
                    InputFile = fullInputPath,
                    OutputFile = fullOutputPath
                });

                if (!result.IsSuccess)
                {
                    logger.LogError("FFmpeg reverse command failed: {ErrorMessage}, Command: {Command}",
                        result.ErrorMessage, result.CommandExecuted);

                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);
                    return Results.Problem("Failed to reverse video: " + result.ErrorMessage, statusCode: 500);
                }

                byte[] fileBytes = await fileService.GetOutputFileAsync(outputFileName);

                _ = fileService.CleanupTempFilesAsync(filesToCleanup);

                return Results.File(fileBytes, "video/mp4", dto.VideoFile.FileName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in ReverseVideo endpoint");
                _ = fileService.CleanupTempFilesAsync(filesToCleanup);
                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
            }
        }

        private static async Task<IResult> ApplyBrightnessContrast(
            HttpContext context,
            [FromForm] BrightnessContrastDto dto)
        {
            var fileService = context.RequestServices.GetRequiredService<IFileService>();
            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

            var filesToCleanup = new List<string>();

            try
            {
                if (dto.VideoFile == null || dto.VideoFile.Length == 0)
                {
                    return Results.BadRequest("Video file is required.");
                }

                string inputFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);
                filesToCleanup.Add(inputFileName);

                string outputExtension = Path.GetExtension(dto.OutputFileName);
                if (string.IsNullOrWhiteSpace(outputExtension))
                {
                    outputExtension = ".mp4";
                }

                string outputFileName = await fileService.GenerateUniqueFileNameAsync(outputExtension);
                filesToCleanup.Add(outputFileName);

                string fullInputPath = fileService.GetFullInputPath(inputFileName);
                string fullOutputPath = fileService.GetFullOutputPath(outputFileName);

                var command = ffmpegService.CreateBrightnessContrastCommand();

                var result = await command.ExecuteAsync(new BrightnessContrastModel
                {
                    InputFile = fullInputPath,
                    OutputFile = fullOutputPath,
                    Brightness = dto.Brightness,
                    Contrast = dto.Contrast
                });

                if (!result.IsSuccess)
                {
                    logger.LogError("FFmpeg brightness/contrast command failed: {ErrorMessage}, Command: {Command}",
                        result.ErrorMessage, result.CommandExecuted);

                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);
                    return Results.Problem("Failed to adjust brightness/contrast: " + result.ErrorMessage, statusCode: 500);
                }

                byte[] fileBytes = await fileService.GetOutputFileAsync(outputFileName);

                _ = fileService.CleanupTempFilesAsync(filesToCleanup);

                string downloadFileName = string.IsNullOrWhiteSpace(dto.OutputFileName)
                    ? dto.VideoFile.FileName
                    : dto.OutputFileName;

                return Results.File(fileBytes, "video/mp4", downloadFileName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in ApplyBrightnessContrast endpoint");
                _ = fileService.CleanupTempFilesAsync(filesToCleanup);
                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
            }
        }

        private static async Task<IResult> AddWatermark(
            HttpContext context,
            [FromForm] WatermarkDto dto)
        {
            var fileService = context.RequestServices.GetRequiredService<IFileService>();
            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

            try
            {
                if (dto.VideoFile == null || dto.WatermarkFile == null)
                {
                    return Results.BadRequest("Video file and watermark file are required");
                }

                string videoFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);
                string watermarkFileName = await fileService.SaveUploadedFileAsync(dto.WatermarkFile);

                string extension = Path.GetExtension(dto.VideoFile.FileName);
                string outputFileName = await fileService.GenerateUniqueFileNameAsync(extension);

                List<string> filesToCleanup = new List<string> { videoFileName, watermarkFileName, outputFileName };

                try
                {
                    var command = ffmpegService.CreateWatermarkCommand();
                    var result = await command.ExecuteAsync(new WatermarkModel
                    {
                        InputFile = videoFileName,
                        WatermarkFile = watermarkFileName,
                        OutputFile = outputFileName,
                        XPosition = dto.XPosition,
                        YPosition = dto.YPosition,
                        IsVideo = true,
                        VideoCodec = "libx264"
                    });

                    if (!result.IsSuccess)
                    {
                        logger.LogError("FFmpeg command failed: {ErrorMessage}, Command: {Command}",
                            result.ErrorMessage, result.CommandExecuted);
                        return Results.Problem("Failed to add watermark: " + result.ErrorMessage, statusCode: 500);
                    }

                    byte[] fileBytes = await fileService.GetOutputFileAsync(outputFileName);

                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);

                    return Results.File(fileBytes, "video/mp4", dto.VideoFile.FileName);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing watermark request");
                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);
                    throw;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in AddWatermark endpoint");
                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
            }
        }

        private static async Task<IResult> AudioEcho(
            HttpContext context,
            [FromForm] AudioEchoDto dto)
        {
            var fileService = context.RequestServices.GetRequiredService<IFileService>();
            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

            var filesToCleanup = new List<string>();

            try
            {
                if (dto.VideoFile == null || dto.VideoFile.Length == 0)
                {
                    return Results.BadRequest("Video file is required.");
                }

                if (dto.Duration <= 0)
                {
                    return Results.BadRequest("Duration must be greater than 0.");
                }

                string inputFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);
                filesToCleanup.Add(inputFileName);

                string outputExtension = Path.GetExtension(dto.OutputFileName);
                if (string.IsNullOrWhiteSpace(outputExtension))
                {
                    outputExtension = ".mp4";
                }

                string outputFileName = await fileService.GenerateUniqueFileNameAsync(outputExtension);
                filesToCleanup.Add(outputFileName);

                string fullInputPath = fileService.GetFullInputPath(inputFileName);
                string fullOutputPath = fileService.GetFullOutputPath(outputFileName);

                var command = ffmpegService.CreateAudioEchoCommand();

                var result = await command.ExecuteAsync(new AudioEchoModel
                {
                    InputFile = fullInputPath,
                    OutputFile = fullOutputPath,
                    Duration = dto.Duration
                });

                if (!result.IsSuccess)
                {
                    logger.LogError("FFmpeg audio echo command failed: {ErrorMessage}, Command: {Command}",
                        result.ErrorMessage, result.CommandExecuted);

                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);
                    return Results.Problem("Failed to apply audio echo: " + result.ErrorMessage, statusCode: 500);
                }

                byte[] fileBytes = await fileService.GetOutputFileAsync(outputFileName);

                _ = fileService.CleanupTempFilesAsync(filesToCleanup);

                string downloadFileName = string.IsNullOrWhiteSpace(dto.OutputFileName)
                    ? dto.VideoFile.FileName
                    : dto.OutputFileName;

                return Results.File(fileBytes, "video/mp4", downloadFileName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in AudioEcho endpoint");
                _ = fileService.CleanupTempFilesAsync(filesToCleanup);
                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
            }
        }

        private static async Task<IResult> MergeVideos(
            HttpContext context,
            [FromForm] MergeDto dto)
        {
        private static async Task<IResult> ExtractAudio(HttpContext context, [FromForm] ExtractAudioDto dto)
        {
            var fileService = context.RequestServices.GetRequiredService<IFileService>();
            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

            try
            {
                if (dto.VideoFile1 == null || dto.VideoFile2 == null)
                    return Results.BadRequest("Both video files are required");

                string file1 = await fileService.SaveUploadedFileAsync(dto.VideoFile1);
                string file2 = await fileService.SaveUploadedFileAsync(dto.VideoFile2);
                string output = await fileService.GenerateUniqueFileNameAsync(Path.GetExtension(dto.VideoFile1.FileName));

                var filesToCleanup = new List<string> { file1, file2, output };

                try
                {
                    var command = ffmpegService.CreateMergeCommand(); // ודאי שמתודה זו קיימת ב-Factory
                    var result = await command.ExecuteAsync(new MergeModel
                    {
                        InputFile1 = file1,
                        InputFile2 = file2,
                        OutputFile = output,
                        IsHorizontal = dto.IsHorizontal
                    });

                    if (!result.IsSuccess)
                        return Results.Problem("Merge failed: " + result.ErrorMessage);

                    byte[] fileBytes = await fileService.GetOutputFileAsync(output);
                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);

                    return Results.File(fileBytes, "video/mp4", "merged_video.mp4");
                }
                catch (Exception ex)
                {
                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);
                    throw;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in MergeVideos");
                return Results.Problem("An error occurred", statusCode: 500);
            var filesToCleanup = new List<string>();

            try
            {
                if (dto.VideoFile == null || dto.VideoFile.Length == 0)
                    return Results.BadRequest("Video file is required.");

                string inputFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);
                filesToCleanup.Add(inputFileName);

                string outputFileName = await fileService.GenerateUniqueFileNameAsync(".mp3");
                filesToCleanup.Add(outputFileName);

                var command = ffmpegService.CreateExtractAudioCommand();
                var result = await command.ExecuteAsync(new ExtractAudioModel
                {
                    InputFile = fileService.GetFullInputPath(inputFileName),
                    OutputFile = fileService.GetFullOutputPath(outputFileName)
                });

                if (!result.IsSuccess)
                {
                    logger.LogError("FFmpeg extract audio failed: {Error}", result.ErrorMessage);
                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);
                    return Results.Problem("Failed to extract audio: " + result.ErrorMessage);
                }

                byte[] fileBytes = await fileService.GetOutputFileAsync(outputFileName);
                _ = fileService.CleanupTempFilesAsync(filesToCleanup);

                return Results.File(fileBytes, "audio/mpeg", "audio.mp3");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in ExtractAudio");
                return Results.Problem("An error occurred: " + ex.Message);
            }
        }

        private static async Task<IResult> ConvertFormat(
            HttpContext context,
            [FromForm] FormatConversionDto dto)
        {
            var fileService = context.RequestServices.GetRequiredService<IFileService>();
            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

            var filesToCleanup = new List<string>();

            try
            {
                if (dto.VideoFile == null || dto.VideoFile.Length == 0)
                {
                    return Results.BadRequest("Video file is required.");
                }

                if (string.IsNullOrWhiteSpace(dto.TargetExtension))
                {
                    return Results.BadRequest("Target extension (e.g. '.avi') is required.");
                }

                string targetExt = dto.TargetExtension.StartsWith(".") ? dto.TargetExtension : "." + dto.TargetExtension;

                string inputFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);
                filesToCleanup.Add(inputFileName);

                string outputFileName = await fileService.GenerateUniqueFileNameAsync(targetExt);
                filesToCleanup.Add(outputFileName);

                string fullInputPath = fileService.GetFullInputPath(inputFileName);
                string fullOutputPath = fileService.GetFullOutputPath(outputFileName);

                var command = ffmpegService.CreateFormatConversionCommand();

                var result = await command.ExecuteAsync(new FormatConversionModel
                {
                    InputFile = fullInputPath,
                    OutputFile = fullOutputPath
                });

                if (!result.IsSuccess)
                {
                    logger.LogError("FFmpeg format conversion failed: {ErrorMessage}, Command: {Command}",
                        result.ErrorMessage, result.CommandExecuted);

                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);
                    return Results.Problem("Failed to convert video format: " + result.ErrorMessage, statusCode: 500);
                }

                byte[] fileBytes = await fileService.GetOutputFileAsync(outputFileName);

                _ = fileService.CleanupTempFilesAsync(filesToCleanup);

                string downloadFileName = Path.GetFileNameWithoutExtension(dto.VideoFile.FileName) + targetExt;

                return Results.File(fileBytes, "application/octet-stream", downloadFileName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in ConvertFormat endpoint");
                _ = fileService.CleanupTempFilesAsync(filesToCleanup);
                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
            }
        }


        private static async Task<IResult> CreateGif(
            HttpContext context,
            [FromForm] CreateGifDto dto)
                {
                    var fileService = context.RequestServices.GetRequiredService<IFileService>();
                    var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
                    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

                    var filesToCleanup = new List<string>();

                    try
                    {
                        if (dto.VideoFile == null || dto.VideoFile.Length == 0)
                        {
                            return Results.BadRequest("Video file is required.");
                        }

                        // שמירת הקובץ שהועלה
                        string inputFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);
                        filesToCleanup.Add(inputFileName);

                        // יצירת שם קובץ פלט מסוג gif
                        string outputFileName = await fileService.GenerateUniqueFileNameAsync(".gif");
                        filesToCleanup.Add(outputFileName);

                        string fullInputPath = fileService.GetFullInputPath(inputFileName);
                        string fullOutputPath = fileService.GetFullOutputPath(outputFileName);

                        // קריאה ל-Factory שיצרת בשלב הקודם
                        var command = ffmpegService.CreateCreateGifCommand();

                        var result = await command.ExecuteAsync(new CreateGifModel
                        {
                            InputFile = fullInputPath,
                            OutputFile = fullOutputPath
                        });

                        if (!result.IsSuccess)
                        {
                            logger.LogError("FFmpeg create gif command failed: {ErrorMessage}, Command: {Command}",
                                result.ErrorMessage, result.CommandExecuted);

                            _ = fileService.CleanupTempFilesAsync(filesToCleanup);
                            return Results.Problem("Failed to create GIF: " + result.ErrorMessage, statusCode: 500);
                        }

                        // החזרת הקובץ למשתמש
                        byte[] fileBytes = await fileService.GetOutputFileAsync(outputFileName);
                        _ = fileService.CleanupTempFilesAsync(filesToCleanup);

                        return Results.File(fileBytes, "image/gif", "output.gif");
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error in CreateGif endpoint");
                        _ = fileService.CleanupTempFilesAsync(filesToCleanup);
                        return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
                    }
                }
    }
}