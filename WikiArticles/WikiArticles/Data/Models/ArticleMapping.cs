using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WikiArticles.Data.Models
{
    internal class ArticleMapping
    {
        public int Id { get; set; }
        public int SourceId { get; set; }
        public int TargetId { get; set; }
    }
}
