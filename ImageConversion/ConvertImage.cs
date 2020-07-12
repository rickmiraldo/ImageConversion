using ImageConversion.Enums;
using ImageConversion.Helpers;
using ImageConversion.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageConversion
{
    class ConvertImage
    {
        public static void StartProcessing(string inputFile, string outputFolderPath, ProcessingConfiguration processingConfiguration)
        {
            // Prepara caminho para salvar imagem
            string filename = Path.GetFileName(inputFile);
            string outputFilePath = outputFolderPath + "\\" + filename;

            // Abre imagem
            Bitmap inputBitmap = new Bitmap(inputFile);

            // Rodar imagem (se necessário)
            rotateImage(inputBitmap, processingConfiguration.RotateFinalImage);

            // Cortar imagem (se necessário)
            if (processingConfiguration.ShouldCropImage)
            {
                inputBitmap = cropImage(inputBitmap, processingConfiguration.MaxCroppedWidth, processingConfiguration.MaxCroppedHeight);
            }

            // Salvar imagem final
            saveImage(outputFilePath, inputBitmap, processingConfiguration.SaveFormat);
            inputBitmap.Dispose();
        }

        private static void saveImage(string outputFilePath, Bitmap image, SaveFormatEnum saveFormat)
        {
            switch (saveFormat)
            {
                case SaveFormatEnum.TIFF:
                    outputFilePath = Path.ChangeExtension(outputFilePath, "tif");
                    SaveHelper.SaveTiff(outputFilePath, image, saveFormat);
                    break;
                case SaveFormatEnum.TIFFLZW:
                    outputFilePath = Path.ChangeExtension(outputFilePath, "tif");
                    SaveHelper.SaveTiff(outputFilePath, image, saveFormat);
                    break;
                case SaveFormatEnum.JPG90:
                    outputFilePath = Path.ChangeExtension(outputFilePath, "jpg");
                    SaveHelper.SaveJpeg(outputFilePath, image, 90L);
                    break;
                case SaveFormatEnum.JPG100:
                    outputFilePath = Path.ChangeExtension(outputFilePath, "jpg");
                    SaveHelper.SaveJpeg(outputFilePath, image, 100L);
                    break;
                default:
                    break;
            }
        }

        private static void rotateImage(Bitmap image, RotateFinalImageEnum rotate)
        {
            switch (rotate)
            {
                case RotateFinalImageEnum.NO:
                    return;
                case RotateFinalImageEnum.R90CCW:
                    image.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    return;
                case RotateFinalImageEnum.R90CW:
                    image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    return;
                case RotateFinalImageEnum.R180:
                    image.RotateFlip(RotateFlipType.Rotate180FlipNone);
                    return;
                default:
                    return;
            }
        }

        private static Bitmap cropImage(Bitmap image, int newMaxWidth, int newMaxHeight)
        {
            var deltaWidth = image.Width - newMaxWidth;
            var deltaHeight = image.Height - newMaxHeight;

            // Se valor da imagem nova for maior do que a imagem original, então não corta nada e devolve a imagem original
            if ((deltaWidth < 0) || (deltaHeight < 0))
            {
                return image;
            }

            var topLeftX = (int)Math.Round((double)(deltaWidth / 2));
            var topLeftY = (int)Math.Round((double)(deltaHeight / 2));

            var topLeftCorner = new Point(topLeftX, topLeftY);
            var size = new Size(newMaxWidth, newMaxHeight);

            Rectangle cropRectangle = new Rectangle(topLeftCorner, size);

            Bitmap cropped = image.Clone(cropRectangle, image.PixelFormat);

            return cropped;
        }
    }
}
