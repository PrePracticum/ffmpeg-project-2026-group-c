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

            app.MapPost("/api/video/merge", MergeVideos)
                .DisableAntiforgery()
                .WithMetadata(new RequestSizeLimitAttribute(209715200));
            app.MapPost("/api/video/reverse", ReverseVideo)
            .DisableAntiforgery()
            .WithMetadata(new RequestSizeLimitAttribute(104857600)); // הגבלת גודל ל-100 MB
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

        private static async Task<IResult> MergeVideos(
            HttpContext context,
            [FromForm] MergeDto dto)
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
            }
        }
    }
}