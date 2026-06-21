using Microsoft.AspNetCore.Http;

namespace FFmpeg.API.DTOs
{
    public class TextOverlayDto
    {
        public IFormFile VideoFile { get; set; }
        public string Text { get; set; }
        public string FontColor { get; set; } = "white";
        public int FontSize { get; set; } = 24;
        public int XPosition { get; set; } = 100;
        public int YPosition { get; set; } = 50;
        public bool Animate { get; set; } = false;
        public double Speed { get; set; } = 50.0;
        public string OutputFileName { get; set; } = "output.mp4";
    }
}
