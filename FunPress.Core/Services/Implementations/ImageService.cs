using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using Microsoft.Extensions.Logging;

namespace FunPress.Core.Services.Implementations
{
    internal class ImageService : IImageService
    {
        private readonly ILogger<ImageService> _logger;
        private readonly IApplicationEnvironment _applicationEnvironment;

        public ImageService(
            ILogger<ImageService> logger, 
            IApplicationEnvironment applicationEnvironment
            )
        {
            _logger = logger;
            _applicationEnvironment = applicationEnvironment;
        }

        public bool GenerateImageByTemplateOne(string imagePath, string generatedNewImagePath)
        {
            try
            {
                var templatePath = Path.Combine(_applicationEnvironment.GetTemplatesPath(), "template_1.JPG");

                var backgroundImage = new Bitmap(templatePath);

                const int overlayImageLeft = 130;
                const int overlayImageTop = 770;
                const int overlayImageHeight = 2000 - 770;
                var overlayImageWidth = backgroundImage.Width - 130 * 2;

                var overlayImage = new Bitmap(imagePath);

                var combinedImage = (Bitmap)backgroundImage.Clone();
                backgroundImage.Dispose();

                using (var combinedImageGraphics = Graphics.FromImage(combinedImage))
                {
                    // Calculate the aspect ratio
                    var ratioX = (double)overlayImageWidth / overlayImage.Width;
                    var ratioY = (double)overlayImageHeight / overlayImage.Height;
                    var ratio = Math.Min(ratioX, ratioY);

                    // New width and height based on aspect ratio
                    var newWidth = (int)(overlayImage.Width * ratio);
                    var newHeight = (int)(overlayImage.Height * ratio);

                    var bmp = new Bitmap(newWidth, newHeight);
                    using (var overlayImageGraphics = Graphics.FromImage(bmp))
                    {
                        // Set InterpolationMode and PixelOffsetMode for better quality resizing
                        overlayImageGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        overlayImageGraphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        overlayImageGraphics.SmoothingMode = SmoothingMode.HighQuality;
                        overlayImageGraphics.CompositingQuality = CompositingQuality.HighQuality;

                        var colorMatrix = new ColorMatrix(
                            new[]
                            {
                                new[] { 0.3f, 0.3f, 0.3f, 0f, 0f },
                                new[] { 0.59f, 0.59f, 0.59f, 0f, 0f },
                                new[] { 0.11f, 0.11f, 0.11f, 0f, 0f },
                                new[] { 0f, 0f, 0f, 1f, 0f },
                                new[] { 0f, 0f, 0f, 0f, 1f }
                            });

                        var attributes = new ImageAttributes();
                        attributes.SetColorMatrix(colorMatrix);

                        overlayImageGraphics.DrawImage(
                            overlayImage, 
                            new Rectangle(0, 0, newWidth, newHeight),
                            0, 
                            0, 
                            overlayImage.Width, 
                            overlayImage.Height,
                            GraphicsUnit.Pixel, 
                            attributes
                            );
                    }

                    combinedImageGraphics.DrawImage(bmp, overlayImageLeft, overlayImageTop, 
                        overlayImageWidth, overlayImageHeight);

                    bmp.Dispose();
                }

                combinedImage.Save(generatedNewImagePath);
                
                overlayImage.Dispose();
                combinedImage.Dispose();

                return true;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Invoke in {Method}", nameof(GenerateImageByTemplateOne));

                return false;
            }
        }
    }
}
