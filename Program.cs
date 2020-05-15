using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using ScrapySharp.Extensions;
using ScrapySharp.Network;
using System.IO;
using System.Globalization;
using CsvHelper;

namespace scraper
{
    class Program
    {
        static ScrapingBrowser _browser = new ScrapingBrowser();
       

        static void Main(string[] args)
        {
            Console.WriteLine("Please enter a search term:");
            var searchTerm = Console.ReadLine();
            var mainPageLinks = GetMainPageLinks("https://newyork.craigslist.org/d/computer-gigs/search/cpg");
            var lstGigs = GetPageDetails(mainPageLinks, searchTerm);
            ExportGigsToCSV(lstGigs, searchTerm);
            Console.WriteLine("Hello World!");
        }


        static List<string> GetMainPageLinks(string url)
        {
            var homePageLinks = new List<string>();
            var html = GetHtml(url);
            var links = html.CssSelect("a");

            foreach (var link in links)
            {
                if (link.Attributes["href"].Value.Contains(".html"))
                {
                    homePageLinks.Add(link.Attributes["href"].Value);
                }
            }
            return homePageLinks;
        }

        static HtmlNode GetHtml(string url)
        {
            WebPage webpage = _browser.NavigateToPage(new Uri(url));
            return webpage.Html;
        }

        

        static List<PageDetails> GetPageDetails(List<string> urls, string searchTerm)
        {
            var lstPageDetails = new List<PageDetails>();
            foreach (var url in urls)
            {
                var htmlNode = GetHtml(url);
                var pageDetails = new PageDetails();

                pageDetails.title = htmlNode.OwnerDocument.DocumentNode.SelectNodes("//html/head/title")[0].InnerText;
                var description = htmlNode.OwnerDocument.DocumentNode.SelectNodes("//html/body/section/section/section/section")[0].InnerText;

                pageDetails.description = description
                 .Replace("\n        \n            QR Code Link to This Post\n            \n        \n", "");

                pageDetails.url = url;

                var searchTermInTitle = pageDetails.title.ToLower().Contains(searchTerm.ToLower());
                var searchTermInDescription = pageDetails.description.ToLower().Contains(searchTerm.ToLower());

                if (searchTermInTitle || searchTermInDescription)
                {
                    lstPageDetails.Add(pageDetails);
                }
            }

            return lstPageDetails;
        }
        
        static void ExportGigsToCSV(List<PageDetails> lstPageDetails, string searchTerm)
        {
            using (var writer = new StreamWriter("scrap.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(lstPageDetails);
            }
        }

    }
    public class PageDetails
    {
        public string title { get; set; }
        public string description { get; set; }
        public string url { get; set; }
    }
}
