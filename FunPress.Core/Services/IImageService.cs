using System.Threading;
using System.Threading.Tasks;

namespace FunPress.Core.Services
{
    public interface IImageService
    {
        Task<bool> GenerateImageByTemplateOneAsync(string imagePath, string generatedNewImagePath, CancellationToken cancellationToken);
    }
}
