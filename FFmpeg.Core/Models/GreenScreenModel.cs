namespace FFmpeg.Core.Models
{
    public class GreenScreenModel
    {
        // נתיב מלא לסרטון עם הרקע הירוק
        public string InputFile { get; set; }

        // נתיב מלא לקובץ הרקע החדש
        public string BackgroundFile { get; set; }

        // נתיב מלא לקובץ הפלט
        public string OutputFile { get; set; }
    }
}
