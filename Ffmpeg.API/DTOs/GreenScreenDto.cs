using Microsoft.AspNetCore.Http;

namespace FFmpeg.API.DTOs
{
    public class GreenScreenDto
    {
        // הסרטון עם הרקע הירוק
        public IFormFile VideoFile { get; set; }

        // הסרטון / תמונה שתחליף את הרקע הירוק
        public IFormFile BackgroundFile { get; set; }
    }
}
