
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.DAL.Resources;
using Api.DAL.Models;
using Api.DAL;
using Microsoft.EntityFrameworkCore;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Api.BLL
{
    public class DocsService : IDocsService
    {
        private readonly DocContext _context;
        private readonly IConfiguration _configuration;

        public DocsService(DocContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        public async Task UploadFileAsync(FileResource formFile)
        {
            bool isNew = false;

            await _context.Docs.Include(x => x.Versions).LoadAsync();
            var same = _context.Docs.FirstOrDefault(x => x.Name == formFile.FileName && x.Category == formFile.category);
            int release = 1;
            if (same != null)
                release = same.Versions.OrderBy(x => x.Release).Last().Release + 1;
            else
            {
                isNew = true;
                same = new Doc
                {
                    Category = formFile.category,
                    Name = formFile.FileName,
                };
                _context.Docs.Add(same);
            }
            string path = $"{_configuration.GetValue<string>("FilesPath")}/{formFile.category}_{release}_{formFile.FileName}";

            var version = new DAL.Models.Version
            {
                Doc = same,
                Path = path,
                Release = release,
                Size = formFile.Size,
                UploadDateTime = DateTime.Now,
            };

            _context.Versions.Add(version);

            try
            {
                await _context.SaveChangesAsync();
                if (!Directory.Exists(_configuration.GetValue<string>("FilesPath")))
                {
                    Directory.CreateDirectory(_configuration.GetValue<string>("FilesPath"));
                }
                using (var fileStream = new FileStream(path, FileMode.Create))
                {
                    await formFile.CopyToAsync(fileStream);
                }
            }
            catch (DbUpdateException db)
            {
                throw db;
            }
            catch (Exception fileStriem)
            {
                if (isNew)
                {
                    _context.Docs.Remove(same);
                }
                _context.Versions.Remove(version);
                throw fileStriem;
            }
        }

        public IList<Doc> GetDocs(Category? category)
        {
            List<Doc> docs;
            if (category != null)
                return docs = _context.Docs.Include(p => p.Versions).Where(p => p.Category == category).ToList();
            else
                return docs = _context.Docs.Include(p => p.Versions).ToList(); 
        }
        public IList<Doc> GetDoc(string name)
        {
            List<Doc> docs;
            docs = _context.Docs.Include(p => p.Versions).Where(p => p.Name == name).ToList();
            if (docs == null)
            {
                return null;
            }
            else
            {
                return docs;
            }
        }

        public async Task<byte[]> DownloadDocAsync(string name, int category, int? release)
        {
            byte[] Mass = null;
            var doc = await _context.Docs.Include(p => p.Versions).FirstOrDefaultAsync(p => (p.Name == name && p.Category == (Category)category));
            if (doc != null)
            {
                var version = doc.Versions.OrderBy(x => x.Release).LastOrDefault();
                if (release != null)
                    version = doc.Versions.FirstOrDefault(x => x.Release == release);

          
                Mass = await System.IO.File.ReadAllBytesAsync(version.Path);
                return Mass; 
            }
            return null;
        }

        public Doc ChangeCategory(string name, Category oldCategory, Category newCategory)
        {
            var doc = _context.Docs.Include(x => x.Versions).FirstOrDefault(p => (p.Name == name && p.Category == (Category)oldCategory));
            if (doc == null)
            {
                return null;
            }
            foreach (var v in doc.Versions)
            {
                string oldPath = v.Path;
                string newPath = $"{_configuration.GetValue<string>("FilesPath")}/{newCategory}_{v.Release}_{name}";
                System.IO.File.Move(oldPath, newPath, true);
                v.Path = newPath;
            }
            doc.Category = newCategory;
            try
            {
                _context.SaveChangesAsync();
                return doc;
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        public Doc DeleteDoc(string name, Category category)
        {
            var doc = _context.Docs.Include(p => p.Versions).FirstOrDefault(p => (p.Name == name && p.Category == (Category)category));
            if (doc == null)
            {
                return null;
            }
            _context.Docs.Remove(doc);

            try
            {
                _context.SaveChangesAsync();
                foreach(var ver in doc.Versions)
                {
                    File.Delete(ver.Path);
                }
            }
            catch (Exception db)
            {
                throw db;
            }
            return doc;
        }
    }
}
