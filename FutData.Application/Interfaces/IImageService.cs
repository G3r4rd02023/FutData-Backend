namespace FutData.Application.Interfaces
{
    public interface IImageService
    {
        Task<string> UploadImageAsync(Stream fileStream, string fileName);
        Task DeleteImageAsync(string imageUrl);
    }
}
