using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using System.Net;
using System.IO;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Collections;



namespace TEDdownloader
{

    class Program
    {

        static void Main(string[] args)
        {

            if (args.Length > 0)
            {
                
                LanguagesClass languageList = new LanguagesClass();
                string language = languageList.GetLanguageName(args[0]);

                if (language != null)
                {
                    Console.WriteLine("Current language: {0}", language);
                    List<String> pageList = new List<string>();
                    pageList = GeneratePageList(args[0], language);

                    foreach (string str in pageList)
                    {
                        Console.WriteLine(str.ToString());
                        VideoClass vc = new VideoClass(str);
                        vc.GetInformation();
                        vc.SaveSrtToFile(@"\srt\");
                        vc.SaveDonwloadLinkToFile(@".\srt\downloadList.txt");

                        vc.ToString();
                    }

                }
                else
                {
                    Console.WriteLine("Didn't find language, please input code language in correct format \n");
                    Console.WriteLine("You can find all languages codes on page: http://www.ted.com/pages/view/id/286");
                }

                Console.WriteLine("Finished. Press any key.");
                Console.ReadKey(true);
                return;
            }
            else
            {
                Console.WriteLine("Please launch application with language code");
                Console.ReadKey(true);
                return;
            }

            //This is main case for save content to mongo repository
            //------------------------------------------------------------------------------------------------------------------------------            
            //RepositoryClass repository = new RepositoryClass();
            //repository.Db();

        }



        static List<String> GeneratePageList(string langCode, string language)
        {
            // Load the html document
            HtmlWeb TED = new HtmlWeb();
            String url = "http://www.ted.com";
            StringBuilder pageLink = new StringBuilder();
            List<String> pageList = new List<string>();

            HtmlDocument linkPage = TED.Load("http://www.ted.com/talks?lang=" + langCode + "&page=1");

            int countOfFiles = Convert.ToInt16((linkPage.DocumentNode.SelectNodes("//div[contains(@class, 'browser')]")[0].ChildNodes[1].InnerText).Split(' ', '\t')[11]);

            Console.WriteLine("Count of video files in {0} language are {1}", language, countOfFiles); 
            int countOfPage;
            if (countOfFiles % 10 == 0)
                countOfPage = countOfFiles / 10;
            else
                countOfPage = (countOfFiles / 10) + 1;
            
            countOfPage = 1;
            
            for (int i = 1; i <= countOfPage; i++)
            {
                Console.WriteLine("Current page:{0} from: {1}, remaining: {2}", i, countOfPage, countOfPage-i);
                linkPage = TED.Load("http://www.ted.com/talks?lang=" + langCode + "&page=" + i.ToString());

                List<HtmlNode> pageLinks = null;
                pageLinks = (from HtmlNode node in linkPage.DocumentNode.SelectNodes("//dt[contains(@class, 'thumbnail')]")
                             where node.ChildNodes[1].Attributes["href"].Value.StartsWith("/talks/")
                             select node).ToList();

                foreach (HtmlNode node in pageLinks)
                {
                    pageLink.Append(url);
                    pageLink.Append(node.ChildNodes[1].Attributes["href"].Value);
                    pageList.Add(pageLink.ToString());
                    Console.WriteLine(pageLink.ToString());
                    pageLink.Clear();

                }

            }

            return pageList;
        }

        
    }
}
