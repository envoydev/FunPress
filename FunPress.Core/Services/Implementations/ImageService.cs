using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace FunPress.Core.Services.Implementations
{
    internal class ImageService : IImageService
    {
        private readonly ILogger<ImageService> _logger;
        private readonly IApplicationEnvironment _applicationEnvironment;
        private readonly IDelayService _delayService;

        public ImageService(
            ILogger<ImageService> logger, 
            IApplicationEnvironment applicationEnvironment, 
            IDelayService delayService
            )
        {
            _logger = logger;
            _applicationEnvironment = applicationEnvironment;
            _delayService = delayService;
        }

        public async Task<bool> GenerateImageByTemplateOneAsync(string imagePath, string generatedNewImagePath, CancellationToken cancellationToken)
        {
            try
            {
                await _delayService.WaitForConditionAsync(() => IsFileAvailable(imagePath), 
                    TimeSpan.FromMilliseconds(1000), 
                    cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogDebug("Invoke in {Method}. Cancellation is requested", nameof(GenerateImageByTemplateOneAsync));
                    
                    return false;
                }
                
                var templatePath = Path.Combine(_applicationEnvironment.GetTemplatesPath(), "template_1.jpg");

                var backgroundImage = new Bitmap(templatePath);
                var overlayImage = new Bitmap(imagePath);

                var combinedImage = (Bitmap)backgroundImage.Clone();
                backgroundImage.Dispose();

                const int overlayImageLeft = 20;
                const int overlayImageTop = 265;
                const int overlayImageHeight = 655 - overlayImageTop;
                const int overlayImageWidth = 625 - overlayImageLeft;
                
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
            catch (OperationCanceledException)
            {
                _logger.LogDebug("Invoke in {Method}. Operation cancelled", nameof(GenerateImageByTemplateOneAsync));

                return false;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Invoke in {Method}", nameof(GenerateImageByTemplateOneAsync));

                return false;
            }
        }

        #region Private methods

        private static bool IsFileAvailable(string filePath)
        {
            try
            {
                using (var _ = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    // If we can open the file with exclusive access, it's available.
                    return true;
                }
            }
            catch (IOException)
            {
                // If an IOException is thrown, the file is likely locked by another process.
                return false;
            }
        }

        #endregion
    }
}
