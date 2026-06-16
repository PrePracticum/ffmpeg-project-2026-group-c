using Microsoft.AspNetCore.Http;

namespace FFmpeg.API.DTOs
{
    public class ExtractAudioDto
    {
        public IFormFile VideoFile { get; set; }
    }
}