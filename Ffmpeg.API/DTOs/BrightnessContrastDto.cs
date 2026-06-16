using Microsoft.AspNetCore.Http;

namespace FFmpeg.API.DTOs
{
    public class BrightnessContrastDto
    {
        public IFormFile VideoFile { get; set; }
        public string OutputFileName { get; set; } = "output.mp4";
        public double Brightness { get; set; } = 0.1;
        public double Contrast { get; set; } = 1.5;
    }
}
