using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WikiArticles.Data.Models;

namespace WikiArticles.Data
{
    internal class ArticleRepository : IArticleRepository
    {
        public int GetArticleIdByUrl(string url)
        {
            using (var context = new WikiDbContext())
            {
                var result = context.Articles.Where(m => m.Url.Equals(url, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
                return result != null ? result.Id : -1;
            }
        }

        public Article GetArticleById(int articleId)
        {
            using (var context = new WikiDbContext())
            {                
                return context.Articles.FirstOrDefault(m => m.Id == articleId);
            }            
        }

        public int SaveArticle(string name, string url)
        {
            using (var context = new WikiDbContext())
            {
                var article = new Article
                {
                    Name = name,
                    Url = url
                };

                context.Articles.Add(article);
                context.SaveChanges();

                return article.Id;
            }
        }

        public void SaveArticleMapping(int sourceId, int targetId)
        {
            using (var context = new WikiDbContext())
            {
                if(!context.MappingArticles.Any(m => m.SourceId == sourceId && m.TargetId == targetId))
                {
                    var articleMapping = new ArticleMapping
                    {
                        SourceId = sourceId,
                        TargetId = targetId
                    };

                    context.MappingArticles.Add(articleMapping);
                    context.SaveChanges();
                }                
            }
        }

        public List<ArticleMapping> GetArticlesMapping(int sourceId, int targetId)
        {
            using (var context = new WikiDbContext())
            {                
                var sourceResult = context.MappingArticles.Where(m => m.SourceId == sourceId).ToList();
                var destResult = context.MappingArticles.Where(m => m.TargetId == targetId).ToList();

                if (destResult == null) 
                {
                    return default;
                }

                return sourceResult;
            }
        }

        public List<ArticleMapping> GetArticleMappingRoute(int id)
        {
            using (var context = new WikiDbContext())
            {
                var response = context.MappingArticles.Where(m => m.Id == id).ToList();
                                    
                if (response == null)
                {
                    return default;
                }

                return response;
            }
        }
    }

    internal interface IArticleRepository
    {
        int SaveArticle(string name, string url);
        int GetArticleIdByUrl(string url);
        Article GetArticleById(int articleId);
        List<ArticleMapping> GetArticlesMapping(int sourceId, int targetId);
        void SaveArticleMapping(int sourceId, int targetId);
        List<ArticleMapping> GetArticleMappingRoute(int id);
    }
}
