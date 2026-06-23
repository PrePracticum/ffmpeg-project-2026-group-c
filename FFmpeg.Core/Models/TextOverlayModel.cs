using System;

namespace FFmpeg.Core.Models
{
    public class TextOverlayModel
    {
        public string InputFile { get; set; }
        public string OutputFile { get; set; }
        public string Text { get; set; }
        public string FontColor { get; set; } = "white";
        public int FontSize { get; set; } = 24;
        public int XPosition { get; set; } = 100;
        public int YPosition { get; set; } = 50;
        public bool Animate { get; set; } = false;
        public double Speed { get; set; } = 50.0; // pixels per second when animating
    }
}
