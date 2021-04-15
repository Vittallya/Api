using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System;
using Api.DAL.Models;
using Api.DAL.Resources;
using Api.BLL;

namespace Api.Web.Controllers
{


    [Route("Files")]
    [ApiController]
    public class DocController : Controller
    {
       // DocContext _context;
       // IWebHostEnvironment _appEnvironment;
        private readonly IDocsService _services;
        //private readonly ILogger<DocController> _logger;
        //cистема внедрения зависимостей использует конструкторы классов для передачи всех зависимостей.
        //Соответственно в конструкторе контроллера мы можем получить зависимость. 
        //Конструкторы являются наиболее предпочтительным вариантом для получения зависимостей

        public DocController(IDocsService services)
        {
            _services = services;
        }
        [Route("Upload")]
        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile uploadeDoc, Category category)
        {
            //_logger.LogInformation(DateTime.Now.ToShortDateString() + "\r\n" +
            //                       DateTime.Now.ToLongTimeString() + ": Try to upload file");


            // if ((int)category < 1 || (int)category > 3)
            if (!Enum.IsDefined(typeof(Category), category))
                return BadRequest("Upload was not successful.\r\n Invalid category.\r\n Please, select correct category");
            if (uploadeDoc == null)
                return BadRequest("Upload was not successful.\r\n You didn't choose a file.\r\n Please, select the file you want to upload");
           
            try
            {
                await _services.UploadFileAsync(new FileResource(uploadeDoc, category));//обращение
                //_logger.LogInformation(DateTime.Now.ToShortDateString() + "\r\n" +
                //                    DateTime.Now.ToLongTimeString() + ": File was upload");
                return CreatedAtAction("Success! File was upload", uploadeDoc);
            }
            catch (Exception exc)
            {
                return BadRequest($"Error: {exc.Message}");
            }
        }

        [Route("All")]
        [HttpGet]
        public IActionResult GetDocs(Category? category)
        {
            //if ((category == null) || ((int)category >= 1 && (int)category <= 3))
            if ((category == null) || (Enum.IsDefined(typeof(Category), category)))
            {
                var docs = _services.GetDocs(category);
                //_logger.LogInformation(DateTime.Now.ToShortDateString() + "\r\n" +
                //                    DateTime.Now.ToLongTimeString() + ": Get all files");
                return Ok(Map(docs));
            }
            else
                return BadRequest("Сategory was selected incorrectly.\r\n Please select one of the available options:\r\n Presentation (category = 1),\r\n Application(2),\r\n Other(3)");
        }
     

        [HttpGet("Name")]
        public IActionResult GetDoc(string name)
        {
            var doc = _services.GetDoc(name);
            if (doc == null)
            {
                return NotFound("The file with the specified name was not found");
            }
            //_logger.LogInformation(DateTime.Now.ToShortDateString() + "\r\n" +
            //                        DateTime.Now.ToLongTimeString() + ": Get a file");
            return Ok(Map(doc));
        }

        private ICollection<DocResource> Map(ICollection<Doc> docs)
        {
            var docsR = new List<DocResource>(docs.Count);

            foreach (var doc in docs)
            {
                docsR.Add(new DocResource
                {
                    Id = doc.Id,
                    Category = doc.Category,
                    Name = doc.Name,
                    Versions = doc.Versions.Select(x => new VersionResource
                    {
                        Id = x.Id,
                        DocId = x.DocId,
                        Path = x.Path,
                        Release = x.Release,
                        Size = x.Size,
                        UploadDateTime = x.UploadDateTime,
                    }).ToList(),
                });
            }
            return docsR;
        }

        private DocResource MapForOne(Doc doc)
        {
            return new DocResource
            {
                Id = doc.Id,
                Category = doc.Category,
                Name = doc.Name,
                Versions = doc.Versions.Select(x => new VersionResource
                {
                    Id = x.Id,
                    DocId = x.DocId,
                    Path = x.Path,
                    Release = x.Release,
                    Size = x.Size,
                    UploadDateTime = x.UploadDateTime,
                }).ToList(),
            };
        }

        [Route("Download")]
        [HttpGet]
        public async Task<IActionResult> DownloadDocAsync(string name, int category, int? release)
        {
            byte[] Mass = null;
            Mass = await _services.DownloadDocAsync(name, category, release);
            if (Mass != null)
                try
                {
                    //_logger.LogInformation(DateTime.Now.ToShortDateString() + "\r\n" +
                    //                DateTime.Now.ToLongTimeString() + ": File was download");
                    return File(Mass, "application/octet-stream", name); //Основной подтип 'Application/Octet-Stream'
                    // Используется для обозначения того, что тело содержит бинарные данные.
                    // MIME тип состоит из типа и подтипа — двух строк разделённых наклонной чертой (/), без использования пробелов.
                    //application Список IANA
                    //Любой вид бинарных данных, явно не попадающих ни в одну другу группу типов. Данные, которые будут выполняться или как 
                    //- либо интерпретироваться, или данные для выполнения, которых необходимо отдельное приложение
                    //Для указания базового типа бинарных данных(данных без определённого типа) используют тип application / octet - stream
                    //Этот тип является базовым для бинарных данных.В связи с тем, что он подразумевает неопределённые бинарные данные, 
                    //браузеры, как правило, не будут пытаться его обработать каком - либо образом, а вызовут для него диалоговое окно «Сохранить Как»,
                }
                catch (Exception e)
                {
                    return BadRequest($"File does not read: {e.Message}");
                }
            return BadRequest("File was not found");
        }

        [Route("Category")]
        [HttpPatch]
        public ActionResult<DocResource> ChangeCategory(string name, Category oldCategory, Category newCategory)
        {
            var doc = _services.ChangeCategory(name, oldCategory, newCategory);

            if (doc == null)
            {
                return BadRequest();
            }
            //_logger.LogInformation(DateTime.Now.ToShortDateString() + "\r\n" +
            //                        DateTime.Now.ToLongTimeString() + ": Category was changed");
            return MapForOne(doc);
        }

        [HttpDelete("Name")]
        public ActionResult<Doc> DeleteDoc(string name, Category category)
        {
            var doc = _services.DeleteDoc(name, category);
            if (doc == null)
            {
                return NotFound("This file was not found");
            }
            //_logger.LogInformation(DateTime.Now.ToShortDateString() + "\r\n" +
            //                        DateTime.Now.ToLongTimeString() + ": File was deleted");
            return doc;
        }
    }
}