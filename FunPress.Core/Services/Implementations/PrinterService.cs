using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace FunPress.Core.Services.Implementations
{
    internal class PrinterService : IPrinterService
    {
        private readonly ILogger<PrinterService> _logger;

        public PrinterService(ILogger<PrinterService> logger)
        {
            _logger = logger;
        }

        public IEnumerable<string> GetPrinterNames()
        {
            try
            {
                return PrinterSettings.InstalledPrinters.Cast<string>();
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Invoke in {Method}", nameof(PrintImage));

                return Enumerable.Empty<string>();
            }
        }

        public bool PrintImage(string printerName, string imagePath)
        {
            var imagePrintingCompletionSource = new TaskCompletionSource<bool>();

            try
            {
                var printerSettings = new PrinterSettings
                {
                    PrinterName = printerName
                };

                _logger.LogInformation("Page size: {PageSize}", printerSettings.DefaultPageSettings.PaperSize);
                
                var printDocument = new PrintDocument();
                printDocument.PrinterSettings = printerSettings;
                printDocument.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
                printDocument.OriginAtMargins = true;
                printDocument.PrintPage += (sender, args) =>
                {
                    using (var img = Image.FromFile(imagePath))
                    {
                        // Page dimensions (taking into account the margins)
                        var pageWidth = args.MarginBounds.Width;
                        var pageHeight = args.MarginBounds.Height;

                        // Image dimensions
                        var imgWidth = img.Width;
                        var imgHeight = img.Height;

                        // Calculate aspect ratios
                        var pageRatio = (float)pageWidth / pageHeight;
                        var imgRatio = (float)imgWidth / imgHeight;

                        // Determine the target dimensions (initialized to image dimensions)
                        int targetWidth;
                        int targetHeight;

                        if (imgRatio > pageRatio)
                        {
                            // If the image's width-to-height ratio is greater than the page's, scale based on width
                            targetWidth = pageWidth;
                            targetHeight = (int)(imgHeight * (pageWidth / (float)imgWidth));
                        }
                        else
                        {
                            // Otherwise, scale based on height
                            targetHeight = pageHeight;
                            targetWidth = (int)(imgWidth * (pageHeight / (float)imgHeight));
                        }

                        // Center the image on the page
                        var x = args.MarginBounds.Left + (pageWidth - targetWidth) / 2;
                        var y = args.MarginBounds.Top + (pageHeight - targetHeight) / 2;

                        // Draw the image
                        args.Graphics.DrawImage(img, new Rectangle(x, y, targetWidth, targetHeight));
                    }
                };
                printDocument.EndPrint += (sender, args) =>
                {
                    printDocument.Dispose();
                    
                    imagePrintingCompletionSource.SetResult(true);
                };

                printDocument.Print();

                imagePrintingCompletionSource.Task.Wait();

                var result = imagePrintingCompletionSource.Task.Result;

                return result;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Invoke in {Method}", nameof(PrintImage));

                return false;
            }
        }
    }
}
