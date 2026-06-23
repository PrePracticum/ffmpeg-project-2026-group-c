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
        // מתודת הרחבה לרישום ה-Endpoint ב-Program.cs
        public static void MapGreenScreenEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/video/replace-green-screen", async ([FromForm] GreenScreenDto dto, [FromServices] GreenScreenCommand command) =>
            {
                // 1. בדיקה שהועלו קבצים
                if (dto.VideoFile == null || dto.BackgroundFile == null)
                {
                    return Results.BadRequest("חובה להעלות גם סרטון מקור וגם קובץ רקע.");
                }

                try
                {
                    // 2. יצירת תיקייה זמנית בשרת לשמירת הקבצים לעיבוד
                    var tempFolder = Path.Combine(Directory.GetCurrentDirectory(), "TempUploads");
                    if (!Directory.Exists(tempFolder))
                    {
                        Directory.CreateDirectory(tempFolder);
                    }

                    // שמירת סרטון המקור (הרקע הירוק) לדיסק
                    var inputPath = Path.Combine(tempFolder, Guid.NewGuid() + Path.GetExtension(dto.VideoFile.FileName));
                    using (var stream = new FileStream(inputPath, FileMode.Create))
                    {
                        await dto.VideoFile.CopyToAsync(stream);
                    }

                    // שמירת סרטון הרקע החדש לדיסק
                    var backgroundPath = Path.Combine(tempFolder, Guid.NewGuid() + Path.GetExtension(dto.BackgroundFile.FileName));
                    using (var stream = new FileStream(backgroundPath, FileMode.Create))
                    {
                        await dto.BackgroundFile.CopyToAsync(stream);
                    }

                    // הגדרת נתיב לקובץ הפלט המוגמר
                    var outputPath = Path.Combine(tempFolder, "output_" + Guid.NewGuid() + ".mp4");

                    // 3. בניית המודל עבור התשתית
                    var model = new GreenScreenModel
                    {
                        InputFile = inputPath,
                        BackgroundFile = backgroundPath,
                        OutputFile = outputPath
                    };

                    // 4. הרצת ה-Command של ה-FFmpeg
                    var result = await command.ExecuteAsync(model);

                    // 5. החזרת תשובה לפי תוצאת הריצה (בהנחה של-CommandResult יש פרופרטי IsSuccess או דומה)
                    // אם אצלכן האובייקט שונה, התאימי את התנאי לפיו
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
            .DisableAntiforgery() // מונע שגיאות אבטחה בעת העלאת קבצי Form ב-.NET חדיש
            .WithTags("Video Processing");
        }
    }
}