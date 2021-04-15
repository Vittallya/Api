using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.DAL.Models
{
    public enum Category
    {
        
        PRESENTATION = 1,
        APPLICATION,
        OTHER
    }
    public class Doc
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Category Category { get; set; }
        public ICollection<Version> Versions { get; set; }
    }
}
