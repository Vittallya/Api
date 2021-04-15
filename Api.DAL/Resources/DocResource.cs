using Api.DAL.Models;
using System.Collections.Generic;

namespace Api.DAL.Resources
{
    public class DocResource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Category Category { get; set; }
        public ICollection<VersionResource> Versions { get; set; }
    }
}
