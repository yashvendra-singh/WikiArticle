using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using WikiArticles;
using WikiArticles.BusinessLayer;
using WikiArticles.Data;

namespace WikiArticles
{
    internal class Program
    {        
        static void Main(string[] args)
        {            
            #region Setup Dependency Injection
            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<IRequest, Request>()
                .AddTransient<IArticleRepository, ArticleRepository>()
                .AddTransient<IWikiArticle, WikiArticle>()                
                .BuildServiceProvider();

            #endregion

            var _request = serviceProvider.GetService<IRequest>();

            if(_request == null)
            {
                Logger.LogError("Something went wrong. Application is unable to take the request.");                
            }
            else
            {
                _request?.Process();
                Logger.WriteLine("Do you wish to continue? Type 'y' or 'n'.");
                var input = Console.ReadLine();
                if (input != null && input.Length == 1 && input[0].Equals('y'))
                {
                    _request?.Process();
                }
            }            
        }        
    }
}