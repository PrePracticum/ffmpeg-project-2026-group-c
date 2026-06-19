using Microsoft.AspNetCore.Http;

namespace FFmpeg.API.DTOs
{
    public class CreateGifDto
    {
        public IFormFile VideoFile { get; set; }
    }
}