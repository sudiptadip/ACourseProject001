using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Utility.Service.IService
{
    public interface IImageService
    {
        Task<string> UploadSingleImageAsync(IFormFile file);
        Task<List<string>> UploadMultipleImagesAsync(IFormFileCollection files);
        Task<bool> DeleteImageAsync(string fileName);
    }
}
