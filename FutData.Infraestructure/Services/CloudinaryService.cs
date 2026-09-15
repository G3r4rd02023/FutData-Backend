using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using FutData.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FutData.Infraestructure.Services
{
    public class CloudinaryService : IImageService
    {
        private readonly Cloudinary? _cloudinary;
        private readonly ILogger<CloudinaryService> _logger;

        public CloudinaryService(IConfiguration configuration, ILogger<CloudinaryService> logger)
        {
            _logger = logger;

            var cloudName = configuration["Cloudinary:CloudName"];
            var apiKey = configuration["Cloudinary:ApiKey"];
            var apiSecret = configuration["Cloudinary:ApiSecret"];

            if (string.IsNullOrEmpty(cloudName) || cloudName.StartsWith("TU_") ||
                string.IsNullOrEmpty(apiKey) || apiKey.StartsWith("TU_") ||
                string.IsNullOrEmpty(apiSecret) || apiSecret.StartsWith("TU_"))
            {
                _logger.LogWarning("Cloudinary no está configurado. Los logos se guardarán como base64.");
                return;
            }

            var account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account);
        }

        public async Task<string> UploadImageAsync(Stream fileStream, string fileName)
        {
            if (_cloudinary == null)
                return await ConvertToBase64(fileStream, fileName);

            try
            {
                var uploadParams = new ImageUploadParams
                {
                    Folder = "futdata/logos",
                    File = new FileDescription(fileName, fileStream),
                    Transformation = new Transformation()
                        .Width(300)
                        .Height(300)
                        .Crop("fill")
                        .Gravity("face"),
                    Format = "png"
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    _logger.LogError("Cloudinary upload failed: {Error}", uploadResult.Error?.Message);
                    return await ConvertToBase64(fileStream, fileName);
                }

                return uploadResult.SecureUrl.AbsoluteUri;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al subir imagen a Cloudinary");
                return await ConvertToBase64(fileStream, fileName);
            }
        }

        public async Task DeleteImageAsync(string imageUrl)
        {
            if (_cloudinary == null || string.IsNullOrEmpty(imageUrl) || imageUrl.StartsWith("data:"))
                return;

            try
            {
                var publicId = ExtractPublicId(imageUrl);
                if (string.IsNullOrEmpty(publicId))
                    return;

                var deleteParams = new DeletionParams(publicId);
                await _cloudinary.DestroyAsync(deleteParams);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar imagen de Cloudinary");
            }
        }

        private async Task<string> ConvertToBase64(Stream fileStream, string fileName)
        {
            using var memoryStream = new MemoryStream();
            await fileStream.CopyToAsync(memoryStream);
            var extension = Path.GetExtension(fileName).TrimStart('.');
            if (string.IsNullOrEmpty(extension)) extension = "png";
            return $"data:image/{extension};base64,{Convert.ToBase64String(memoryStream.ToArray())}";
        }

        private static string? ExtractPublicId(string imageUrl)
        {
            try
            {
                var uri = new Uri(imageUrl);
                var pathParts = uri.AbsolutePath.Split('/');
                var startIndex = Array.IndexOf(pathParts, "upload") + 1;
                if (startIndex >= pathParts.Length) return null;

                var publicIdParts = pathParts.Skip(startIndex).ToArray();
                var publicId = string.Join("/", publicIdParts);

                var lastDotIndex = publicId.LastIndexOf('.');
                if (lastDotIndex > 0)
                    publicId = publicId[..lastDotIndex];

                return publicId;
            }
            catch
            {
                return null;
            }
        }
    }
}
