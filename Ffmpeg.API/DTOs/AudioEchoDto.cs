using Microsoft.AspNetCore.Http;

namespace FFmpeg.API.DTOs
{
    public class AudioEchoDto
    {
        public IFormFile VideoFile { get; set; }
        public double Duration { get; set; }
        public string? OutputFileName { get; set; }
    }
}
