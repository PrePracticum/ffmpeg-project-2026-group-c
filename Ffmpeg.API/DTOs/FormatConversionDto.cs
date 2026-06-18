using Microsoft.AspNetCore.Http;

namespace FFmpeg.API.DTOs
{
    public class FormatConversionDto
    {
        public IFormFile VideoFile { get; set; }
        public string TargetExtension { get; set; } 
    }
}