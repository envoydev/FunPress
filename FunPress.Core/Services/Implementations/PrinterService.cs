using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace FunPress.Core.Services.Implementations
{
    internal class PrinterService : IPrinterService
    {
        private readonly ILogger<PrinterService> _logger;

        private TaskCompletionSource<bool> _imagePrintingCompletionSource;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isPrintingProcessStarted;

        public PrinterService(ILogger<PrinterService> logger)
        {
            _logger = logger;

            _cancellationTokenSource = new CancellationTokenSource();
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
            if (_isPrintingProcessStarted)
            {
                return false;
            }

            if (_cancellationTokenSource.IsCancellationRequested)
            {
                _logger.LogInformation("Invoke in {Method}. Operation cancelled", nameof(PrintImage));

                return false;
            }

            _isPrintingProcessStarted = true;
            _imagePrintingCompletionSource = new TaskCompletionSource<bool>();

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

                printDocument.PrintPage += (sender, args) =>
                {
                    using (var image = Image.FromFile(imagePath))
                    {
                        args.Graphics.DrawImage(image, 0, 0, args.PageSettings.PrintableArea.Width,
                            args.PageSettings.PrintableArea.Height);
                    }
                };

                var isPrintingEnded = false;
                printDocument.EndPrint += (sender, args) =>
                {
                    if (_imagePrintingCompletionSource == null || isPrintingEnded)
                    {
                        return;
                    }

                    printDocument.Dispose();

                    _imagePrintingCompletionSource.SetResult(true);

                    isPrintingEnded = true;
                };

                printDocument.Print();

                _imagePrintingCompletionSource.Task.Wait(_cancellationTokenSource.Token);

                var result = _imagePrintingCompletionSource.Task.Result;

                return result;
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Invoke in {Method}. Operation cancelled", nameof(PrintImage));

                return false;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Invoke in {Method}", nameof(PrintImage));

                return false;
            }
            finally
            {
                _imagePrintingCompletionSource = null;
                _isPrintingProcessStarted = false;
            }
        }

        public void CancelCurrentPrinting()
        {
            try
            {
                _imagePrintingCompletionSource?.SetCanceled();
                _cancellationTokenSource.Cancel();

                _cancellationTokenSource = new CancellationTokenSource();
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Invoke in {Method}", nameof(CancelCurrentPrinting));
            }
        }
    }
}
