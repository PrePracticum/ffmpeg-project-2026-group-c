namespace FFmpeg.API.DTOs
{
    public class ExtractFrameDto
    {
        public IFormFile VideoFile { get; set; }
        public string TimeStamp { get; set; } // Format: HH:MM:SS
        public string OutputImageName { get; set; } // e.g., "frame.png" or "frame.jpg"
    }
}
