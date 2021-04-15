using Microsoft.EntityFrameworkCore;
using Api.DAL.Models;

namespace Api.DAL
{
    public class DocContext : DbContext
    {
        public DocContext(DbContextOptions<DocContext> options)
            : base(options){}
        //для файлов
        public DbSet<Doc> Docs { get; set; }
        public DbSet<Models.Version> Versions { get; set; }
    }
}
