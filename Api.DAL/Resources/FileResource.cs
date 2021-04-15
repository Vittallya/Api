using Api.DAL.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

namespace Api.DAL.Resources
{
    public class FileResource
    {
        private readonly IFormFile _formFile;
        public Category category;

        public FileResource(IFormFile formFile, Category _category)
        {
            _formFile = formFile;
            category = _category;
        }

        public string FileName => _formFile.FileName;
        public long Size => _formFile.Length;
        public async Task CopyToAsync(Stream stream)
        {
            await _formFile.CopyToAsync(stream);
        }
    }
}
