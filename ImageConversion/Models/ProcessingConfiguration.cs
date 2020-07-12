using ImageConversion.Enums;

namespace ImageConversion.Models
{
    public class ProcessingConfiguration
    {
        public SaveFormatEnum SaveFormat { get; }

        public RotateFinalImageEnum RotateFinalImage { get; }

        public bool ShouldCropImage { get; }

        public int MaxCroppedHeight { get; }

        public int MaxCroppedWidth { get; }

        public ProcessingConfiguration(SaveFormatEnum saveFormat, RotateFinalImageEnum rotateFinalImage, bool shouldCropImage, int maxHeight, int maxWidth)
        {
            SaveFormat = saveFormat;
            RotateFinalImage = rotateFinalImage;
            ShouldCropImage = shouldCropImage;
            MaxCroppedHeight = maxHeight;
            MaxCroppedWidth = maxWidth;
        }
    }
}
