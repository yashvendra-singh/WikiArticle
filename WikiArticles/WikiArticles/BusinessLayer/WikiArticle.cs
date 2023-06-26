using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using WikiArticles.Data;
using WikiArticles.Data.Models;

namespace WikiArticles.BusinessLayer
{
    internal class WikiArticle : IWikiArticle
    {
        private readonly IArticleRepository _articleRepository;
        private int MinCount = int.MaxValue;
        private List<string> Result = new List<string>();
        
        public WikiArticle(IArticleRepository articleRepository)
        {
            _articleRepository = articleRepository ?? throw new ArgumentNullException(nameof(articleRepository));
        }

        public void SearchArticles(string sourcePath, string targetPath) 
        {
            //Searching HTML document upto a certain level, starting from source path and storing on DB
            Search(sourcePath, targetPath);

            //Get Source and Target path Ids from DB
            var sourceId = _articleRepository.GetArticleIdByUrl(sourcePath);
            var targetId = _articleRepository.GetArticleIdByUrl(targetPath);

            //Check if target exists
            if(targetId > 0)
            {
                FindShortestPath(sourceId, targetId);
            }
            else
            {
                Logger.WriteLine($"The target path has no routes from the given source path with a defined level of {Constants.SearchLevel}");
            }            
        }

        private void FindShortestPath(int sourceId, int targetId)
        {
            var response = _articleRepository.GetArticlesMapping(sourceId, targetId);
            
            foreach (var item in response)
            {
                var (count, path) = GetPath(item, targetId, item.SourceId, new List<string>());

                if(MinCount > count && path != null)
                {
                    MinCount = count;
                    Result = path;
                }
            }

            foreach(var item in Result)
            {
                Logger.WriteLine(item);
            }
        }

        private (int count, List<string> path) GetPath(ArticleMapping mapping, int targetId, int sourceId, List<string> path, int count = 0)
        {
            if(count > Constants.SearchLevel + 1)
            {
                return default;
            }

            if(mapping.TargetId == targetId)
            {
                return (count, path);
            }

            var response = _articleRepository.GetArticleMappingRoute(mapping.Id);

            var article = _articleRepository.GetArticleById(mapping.Id);

            foreach(var artMap in response)
            {
                artMap.Id = artMap.SourceId = artMap.TargetId;
                var res = GetPath(artMap, targetId, artMap.SourceId, path, ++count);

                if (res.path != null) {
                    path.Add($"Step {count + 1} : {article.Name} : {article.Url}");
                }
            }                       
            
            return (count, path);
        } 

        private void Search(string sourcePath, string targetPath, int level = 0)
        {
            if (level > Constants.SearchLevel)
            {
                return;
            }

            ////No need to go through the same path again as this path is already saved.
            //if(_articleRepository.GetArticleIdByUrl(sourcePath) > 0)
            //{
            //    return;
            //}

            Logger.WriteLine($"Level {level} : url {sourcePath}");

            var sourceArticleId = GetArticlePath(sourcePath.Split('/')[4], sourcePath);

            var anchorNodes = GetAnchorNodes(sourcePath);

            level++;

            foreach (var node in anchorNodes)
            {
                var (url, id) = SearchNode(node);
                if (string.IsNullOrWhiteSpace(url))
                {
                    continue;
                }

                if (id == sourceArticleId) 
                {
                    continue;
                }

                Console.WriteLine($"Level {level} : sub-url {node.Attributes["href"].Value}");

                //Save mapping
                _articleRepository.SaveArticleMapping(sourceArticleId, id);

                //Call SearchArticles recusively using DFS traversal
                Search(url, targetPath, level);
            }
        }

        private (string url, int id)  SearchNode(HtmlNode anchorNode)
        {            
            if (anchorNode.Attributes != null && anchorNode.Attributes["href"] != null 
                && anchorNode.Attributes["href"].Value.StartsWith(Constants.HrefStartUrl)
                && !anchorNode.Attributes["href"].Value.Contains(':')
                && anchorNode.Attributes["href"].Value != Constants.HrefMainWikiUrl)
            {
                var url = Constants.WikiBaseUrl + anchorNode.Attributes["href"].Value;
                var articleId = GetArticlePath(anchorNode.InnerHtml, url);


                return (url, articleId);
            }

            return default;
        }

        private int GetArticlePath(string name, string path)
        {            
            var articleId = _articleRepository.GetArticleIdByUrl(path);

            if (articleId == -1)
            {
                articleId = SaveArticle(name, path);
            }

            return articleId;
        }

        private int SaveArticle(string name, string url)
        {
            return _articleRepository.SaveArticle(name, url);
        }

        private HtmlNodeCollection GetAnchorNodes(string url)
        {
            string html;
            //Reading html content from the url
            using (var client = new HttpClient())
            {
                html = client.GetStringAsync(url).Result;
            }

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);
            return document.DocumentNode.SelectNodes("//a");
        }
    }

    internal interface IWikiArticle
    {
        void SearchArticles(string sourcePath, string targetPath);
    }
}
