using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.IO;
using System.Threading.Tasks;
using FFmpeg.API.DTOs;
using FFmpeg.Core.Models;
using FFmpeg.Infrastructure.Commands;

namespace FFmpeg.API.Endpoints
{
    public static class GreenScreenEndpoint
    {
        public static void MapGreenScreenEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/video/replace-green-screen", async ([FromForm] GreenScreenDto dto, [FromServices] GreenScreenCommand command) =>
            {
                if (dto.VideoFile == null || dto.BackgroundFile == null)
                {
                    return Results.BadRequest("חובה להעלות גם סרטון מקור וגם קובץ רקע.");
                }

                try
                {
                    var tempFolder = Path.Combine(Directory.GetCurrentDirectory(), "TempUploads");
                    if (!Directory.Exists(tempFolder))
                    {
                        Directory.CreateDirectory(tempFolder);
                    }

                    var inputPath = Path.Combine(tempFolder, Guid.NewGuid() + Path.GetExtension(dto.VideoFile.FileName));
                    using (var stream = new FileStream(inputPath, FileMode.Create))
                    {
                        await dto.VideoFile.CopyToAsync(stream);
                    }

                    var backgroundPath = Path.Combine(tempFolder, Guid.NewGuid() + Path.GetExtension(dto.BackgroundFile.FileName));
                    using (var stream = new FileStream(backgroundPath, FileMode.Create))
                    {
                        await dto.BackgroundFile.CopyToAsync(stream);
                    }

                    var outputPath = Path.Combine(tempFolder, "output_" + Guid.NewGuid() + ".mp4");

                    var model = new GreenScreenModel
                    {
                        InputFile = inputPath,
                        BackgroundFile = backgroundPath,
                        OutputFile = outputPath
                    };

                    var result = await command.ExecuteAsync(model);

                    if (result)
                    {
                        return Results.Ok(new { Message = "הרקע הירוק הוחלף בהצלחה!", FilePath = outputPath });
                    }

                    return Results.StatusCode(500);
                }
                catch (Exception ex)
                {
                    return Results.Problem($"אירעה שגיאה בעיבוד הקובץ: {ex.Message}");
                }
            })
            .DisableAntiforgery()
            .WithTags("Video Processing");
        }
    }
}