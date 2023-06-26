using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WikiArticles.Data.Models;

namespace WikiArticles.Data
{
    internal class WikiDbContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase(databaseName: "WikiDb");
        }

        public DbSet<Article> Articles { get; set; }
        public DbSet<ArticleMapping> MappingArticles { get; set; }
    }
}
