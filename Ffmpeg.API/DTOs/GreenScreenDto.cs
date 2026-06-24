using Microsoft.AspNetCore.Http;

namespace FFmpeg.API.DTOs
{
    public class GreenScreenDto
    {
        public IFormFile VideoFile { get; set; }

        public IFormFile BackgroundFile { get; set; }
    }
}
