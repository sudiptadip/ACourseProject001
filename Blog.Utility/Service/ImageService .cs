using Blog.Utility.Service.IService;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Utility.Service
{
    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ImageService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        // Upload a single image
        public async Task<string> UploadSingleImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return null;

            string wwwRootPath = _webHostEnvironment.WebRootPath;
            string fileName = Path.GetFileNameWithoutExtension(file.FileName);
            string extension = Path.GetExtension(file.FileName);
            fileName = fileName + "_" + Path.GetRandomFileName() + extension;
            string path = Path.Combine(wwwRootPath + "/uploads", fileName);

            if (!Directory.Exists(Path.Combine(wwwRootPath, "uploads")))
            {
                Directory.CreateDirectory(Path.Combine(wwwRootPath, "uploads"));
            }

            using (var fileStream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return $"/uploads/{fileName}";
        }

        // Upload multiple images
        public async Task<List<string>> UploadMultipleImagesAsync(IFormFileCollection files)
        {
            List<string> uploadedFileUrls = new List<string>();

            foreach (var file in files)
            {
                string imageUrl = await UploadSingleImageAsync(file);
                if (imageUrl != null)
                {
                    uploadedFileUrls.Add(imageUrl);
                }
            }

            return uploadedFileUrls;
        }

        // Delete an image
        public async Task<bool> DeleteImageAsync(string fileName)
        {
            string wwwRootPath = _webHostEnvironment.WebRootPath;
            string filePath = Path.Combine(wwwRootPath + "/uploads", fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return true;
            }

            return false;
        }
    }
}
