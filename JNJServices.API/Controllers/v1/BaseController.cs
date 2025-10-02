using JNJServices.Models.ApiResponseModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace JNJServices.API.Controllers.v1
{
    [ApiController]
    [Authorize]
    public class BaseController : ControllerBase
    {
        [NonAction]
        public async Task<string> CompressAndSaveImage(IFormFile file, string innerDirectory)
        {
            const long MaxFileSize = 10 * 1024 * 1024; // 10 MB

            // Ensure that file is not null
            if (file == null)
            {
                ArgumentNullException.ThrowIfNull(file);
            }

            // Load the image using ImageSharp
            using (var image = await Image.LoadAsync(file.OpenReadStream()))
            {
                string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), innerDirectory);

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string fileName = $"{Path.GetFileNameWithoutExtension(file.FileName)}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}".Replace(" ", "_");
                string filePath = Path.Combine(directoryPath, fileName);

                // Start with an initial quality
                int quality = 75;

                using (var memoryStream = new MemoryStream())
                {
                    // Compress and check file size
                    var encoder = new JpegEncoder { Quality = quality };

                    await image.SaveAsync(memoryStream, encoder);

                    // Reduce quality until the file size is under the limit
                    while (memoryStream.Length > MaxFileSize && quality > 10)
                    {
                        memoryStream.SetLength(0); // Clear the memory stream

                        // Reduce quality
                        quality -= 10;
                        encoder = new JpegEncoder { Quality = quality };

                        // Save again with reduced quality
                        await image.SaveAsync(memoryStream, encoder);
                    }

                    // Save the final image to disk
                    using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                    {
                        memoryStream.Seek(0, SeekOrigin.Begin);
                        await memoryStream.CopyToAsync(fileStream);
                    }
                }

                return Path.Combine(innerDirectory, fileName);
            }
        }

        [NonAction]
        protected async Task<string> SaveImageAsync(IFormFile file, string mediaDirectory)
        {
            string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), mediaDirectory);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string fileName = $"{Path.GetFileNameWithoutExtension(file.FileName)}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}".Replace(" ", "_");
            string filePath = Path.Combine(directoryPath, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                await file.CopyToAsync(fileStream);
            }

            return $"/{Path.Combine(mediaDirectory, fileName)}".Replace("\\", "/");
        }

        [NonAction]
        protected async Task<UploadedFileInfo> SaveDocumentAsync(IFormFile file, string mediaDirectory)
        {
            string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), mediaDirectory);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // Keep the original file name (unchanged)
            string originalFileName = file.FileName;

            // Generate a unique file name to avoid conflicts
            string uniqueFileName = $"{Path.GetFileNameWithoutExtension(file.FileName)}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}".Replace(" ", "_");
            string filePath = Path.Combine(directoryPath, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                await file.CopyToAsync(fileStream);
            }

            return new UploadedFileInfo
            {
                FileName = originalFileName, // Keep original file name
                ContentType = file.ContentType,
                FilePath = $"/{Path.Combine(mediaDirectory, uniqueFileName)}".Replace("\\", "/")
            };
        }

        [NonAction]
        protected async Task<string> ConvertToBase64Async(IFormFile file)
        {
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                byte[] fileBytes = memoryStream.ToArray();
                return Convert.ToBase64String(fileBytes);
            }
        }

    }
}
