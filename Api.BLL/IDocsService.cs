using System.Collections.Generic;
using System.Threading.Tasks;
using Api.DAL.Resources;
using Api.DAL.Models;

namespace Api.BLL
{
    public interface IDocsService
    {
        Task UploadFileAsync(FileResource formFile);
        IList<Doc> GetDocs(Category? category);
        IList<Doc> GetDoc(string name);
        Task<byte[]> DownloadDocAsync(string name, int category, int? release);
        Doc ChangeCategory(string name, Category oldCategory, Category newCategory);
        Doc DeleteDoc(string name, Category category);
    }
}