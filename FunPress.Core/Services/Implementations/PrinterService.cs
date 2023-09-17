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
                var printDocument = new PrintDocument();
                printDocument.PrinterSettings.PrinterName = printerName;
                printDocument.PrintPage += (sender, args) =>
                {
                    var image = Image.FromFile(imagePath);

                    args.Graphics.DrawImage(image, 0, 0, args.PageBounds.Width, args.PageBounds.Height);

                    image.Dispose();

                    imagePrintingCompletionSource.SetResult(true);
                };

                printDocument.Print();

                imagePrintingCompletionSource.Task.Wait(5000);

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
