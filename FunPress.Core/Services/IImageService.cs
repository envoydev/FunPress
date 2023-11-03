using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace FunPress.Core.Services
{
    public interface IImageService
    {
        Task<bool> GenerateImageByTemplateOneAsync(string imagePath, string generatedNewImagePath, CancellationToken cancellationToken);
        BitmapImage GetBitmapImageByPath(string imagePath, bool threadSafe);
    }
}
