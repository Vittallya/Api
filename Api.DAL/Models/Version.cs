using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Api.DAL.Models
{
    public class Version
    {
        public int Id { get; set; }
        public long Size { get; set; }
        public DateTime UploadDateTime { get; set; }
        public int Release { get; set; }
        public string Path { get; set; }
        public int DocId { get; set; }
        public Doc Doc { get; set; }
    }
}
