using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WikiArticles.BusinessLayer;
using static WikiArticles.Enum;

namespace WikiArticles
{
    internal class Request : IRequest
    {
        private readonly IWikiArticle _wikiArticle;
        public Request(IWikiArticle wikiArticle)
        {
            _wikiArticle = wikiArticle ?? throw new ArgumentNullException(nameof(wikiArticle));
        }


        /// <summary>
        /// Method to process input
        /// </summary>
        public void Process()
        {
            Logger.WriteLine("Please enter source and target path:");

            var sourcePath = GetPath(WikiPathType.SourcePath);
            var targetPath = GetPath(WikiPathType.TargetPath);

            if(ValidateSourceTargetUrl(sourcePath, targetPath))
            {
                return;
            }

            Logger.WriteLine("Searching for shortest path...");

            _wikiArticle.SearchArticles(sourcePath, targetPath);
        }

        private bool ValidateSourceTargetUrl(string sourceUrl, string targetUrl)
        {
            if(sourceUrl.Equals(targetUrl, StringComparison.CurrentCultureIgnoreCase))
            {
                Logger.WriteLine("Source and Target url are same");
                return true;
            }

            return false;
        } 

        private string GetPath(WikiPathType type)
        {
            switch(type)
            {
                case WikiPathType.SourcePath:
                    Logger.Write("Source Path => ");
                    break;
                case WikiPathType.TargetPath:
                    Logger.Write("Target Path => ");
                    break;
            }

            var inputPath = Console.ReadLine();

            if (!ValidateUrl(inputPath))
            {
                Logger.LogError((type == WikiPathType.SourcePath ? "Source Path is invalid." : "Target Path is invalid.") + " Please enter again.");                
                GetPath(type);
            }
                
            return inputPath;
        }

        private bool ValidateUrl(string path)
        {
            if(string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            bool result = Uri.TryCreate(path, UriKind.Absolute, out Uri uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps) 
                && uriResult.Host.Equals(Constants.WikiHostUrl, StringComparison.InvariantCultureIgnoreCase);

            var separatedPath = path.Split('/');
        
            return result && separatedPath.Length == 5 && separatedPath[3].Equals("wiki", StringComparison.CurrentCultureIgnoreCase);
        }
    }

    internal interface IRequest
    {
        void Process();        
    }
}
