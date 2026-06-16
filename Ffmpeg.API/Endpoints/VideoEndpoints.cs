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
using Microsoft.Extensions.Logging;

namespace FFmpeg.API.Endpoints
{
    public static class VideoEndpoints
    {
        public static void MapEndpoints(this WebApplication app)
        {
            app.MapPost("/api/video/watermark", AddWatermark)
                .DisableAntiforgery()
                .WithMetadata(new RequestSizeLimitAttribute(104857600)); // 100 MB

            app.MapPost("/api/video/reverse", ReverseVideo)
            .DisableAntiforgery()
            .WithMetadata(new RequestSizeLimitAttribute(104857600)); // הגבלת גודל ל-100 MB

<<<<<<< HEAD
            app.MapPost("/api/video/extract-frame", ExtractFrame)
=======
            app.MapPost("/api/video/extract-audio", ExtractAudio)
      .DisableAntiforgery()
      .WithMetadata(new RequestSizeLimitAttribute(104857600));


            app.MapPost("/api/video/brightness-contrast", ApplyBrightnessContrast)
>>>>>>> main
                .DisableAntiforgery()
                .WithMetadata(new RequestSizeLimitAttribute(104857600)); // 100 MB
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
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>(); // or a specific logger type

            try
            {
                // Validate request
                if (dto.VideoFile == null || dto.WatermarkFile == null)
                {
                    return Results.BadRequest("Video file and watermark file are required");
                }

                // Save uploaded files
                string videoFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);
                string watermarkFileName = await fileService.SaveUploadedFileAsync(dto.WatermarkFile);

                // Generate output filename
                string extension = Path.GetExtension(dto.VideoFile.FileName);
                string outputFileName = await fileService.GenerateUniqueFileNameAsync(extension);

                // Track files to clean up
                List<string> filesToCleanup = new List<string> { videoFileName, watermarkFileName, outputFileName };

                try
                {
                    // Create and execute the watermark command
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

                    // Read the output file
                    byte[] fileBytes = await fileService.GetOutputFileAsync(outputFileName);

                    // Clean up temporary files
                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);

                    // Return the file
                    return Results.File(fileBytes, "video/mp4", dto.VideoFile.FileName);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing watermark request");
                    // Clean up on error
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

<<<<<<< HEAD
        private static async Task<IResult> ExtractFrame(
            HttpContext context,
            [FromForm] ExtractFrameDto dto)
        {
=======
        private static async Task<IResult> ExtractAudio(HttpContext context, [FromForm] ExtractAudioDto dto)
        {
            // סידור משתנים בראש הפונקציה למניעת שגיאת CS0841
>>>>>>> main
            var fileService = context.RequestServices.GetRequiredService<IFileService>();
            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

            var filesToCleanup = new List<string>();

            try
            {
<<<<<<< HEAD
                // Validate request
                if (dto.VideoFile == null || dto.VideoFile.Length == 0)
                {
                    return Results.BadRequest("Video file is required.");
                }

                if (string.IsNullOrEmpty(dto.TimeStamp))
                {
                    return Results.BadRequest("TimeStamp is required (format: HH:MM:SS).");
                }

                if (string.IsNullOrEmpty(dto.OutputImageName))
                {
                    return Results.BadRequest("OutputImageName is required.");
                }

                // Save uploaded video file
                string inputFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);
                filesToCleanup.Add(inputFileName);

                // Determine image extension from OutputImageName or use .png as default
                string imageExtension = Path.GetExtension(dto.OutputImageName);
                if (string.IsNullOrEmpty(imageExtension))
                {
                    imageExtension = ".png";
                }

                // Generate output filename for the image
                string outputFileName = await fileService.GenerateUniqueFileNameAsync(imageExtension);
                filesToCleanup.Add(outputFileName);

                // Create and execute the extract frame command
                var command = ffmpegService.CreateExtractFrameCommand();
                var result = await command.ExecuteAsync(new ExtractFrameModel
                {
                    InputFile = inputFileName,
                    TimeStamp = dto.TimeStamp,
                    OutputFile = outputFileName
=======
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
>>>>>>> main
                });

                if (!result.IsSuccess)
                {
<<<<<<< HEAD
                    logger.LogError("FFmpeg extract frame command failed: {ErrorMessage}, Command: {Command}",
                        result.ErrorMessage, result.CommandExecuted);

                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);
                    return Results.Problem("Failed to extract frame: " + result.ErrorMessage, statusCode: 500);
                }

                // Read the output image file
                byte[] fileBytes = await fileService.GetOutputFileAsync(outputFileName);

                // Clean up temporary files
                _ = fileService.CleanupTempFilesAsync(filesToCleanup);

                // Determine MIME type based on extension
                string mimeType = imageExtension.ToLower() switch
                {
                    ".png" => "image/png",
                    ".jpg" => "image/jpeg",
                    ".jpeg" => "image/jpeg",
                    ".bmp" => "image/bmp",
                    _ => "image/png"
                };

                // Return the image file
                return Results.File(fileBytes, mimeType, Path.GetFileName(dto.OutputImageName));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in ExtractFrame endpoint");
                _ = fileService.CleanupTempFilesAsync(filesToCleanup);
                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
=======
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
>>>>>>> main
            }
        }
    }
}