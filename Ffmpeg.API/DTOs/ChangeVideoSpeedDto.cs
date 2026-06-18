using Microsoft.AspNetCore.Http;

namespace FFmpeg.API.DTOs
{
    public class ChangeVideoSpeedDto
    {
        public IFormFile VideoFile { get; set; }
        public double Speed { get; set; } = 1.0;
    }
}
