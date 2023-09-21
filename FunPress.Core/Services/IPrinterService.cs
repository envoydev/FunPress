using System.Collections.Generic;

namespace FunPress.Core.Services
{
    public interface IPrinterService
    {
        IEnumerable<string> GetPrinterNames();

        bool PrintImage(string printerName, string imagePath);
    }
}
