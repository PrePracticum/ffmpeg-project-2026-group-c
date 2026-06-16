namespace FFmpeg.Core.Models
{
    public class BrightnessContrastModel
    {
        public string InputFile { get; set; }
        public string OutputFile { get; set; }
        public double Brightness { get; set; } = 0.1;
        public double Contrast { get; set; } = 1.5;
    }
}
