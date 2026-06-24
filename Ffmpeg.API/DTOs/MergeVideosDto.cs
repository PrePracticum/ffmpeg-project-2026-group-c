using Microsoft.AspNetCore.Http;

namespace FFmpeg.API.DTOs
{
    public class MergeDto
    {
        public IFormFile VideoFile1 { get; set; }
        public IFormFile VideoFile2 { get; set; }
        public bool IsHorizontal { get; set; } = true;
    }
}