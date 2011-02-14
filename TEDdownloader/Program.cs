using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Collections;
using System.Text.RegularExpressions;




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

            WebRequest requestToLanguagePage = WebRequest.Create("http://www.ted.com/talks?lang=" + langCode + "&page=1");
            requestToLanguagePage.Method = "GET";
            WebResponse response = requestToLanguagePage.GetResponse();
            StreamReader sr = new StreamReader(response.GetResponseStream(), System.Text.Encoding.UTF8);
            string html = sr.ReadToEnd();

            Match countMatch = Regex.Match(html, @"(?<=Showing.+of\s+)\d+(?=\s+)", RegexOptions.IgnoreCase);
            int countOfFiles;
            if (countMatch.Success)
            {
                countOfFiles = Convert.ToInt32(countMatch.Value);
            }
            else
            {
                countOfFiles = 0;
                Console.WriteLine("countOfFiles value parsing error");
            }
            Console.WriteLine("Count of video files in {0} language are {1}", language, countOfFiles);

            
            int countOfPage;
            if (countOfFiles % 10 == 0)
                countOfPage = countOfFiles / 10;
            else
                countOfPage = (countOfFiles / 10) + 1;

            countOfPage = 2; //For testing needs

            StringBuilder pageLink = new StringBuilder();
            List<String> pageList = new List<string>();
            String url = "http://www.ted.com";

            for (int i = 1; i <= countOfPage; i++)
            {
                Console.WriteLine("Current page:{0} from: {1}, remaining: {2}", i, countOfPage, countOfPage - i);
                
                requestToLanguagePage = WebRequest.Create("http://www.ted.com/talks?lang=" + langCode + "&page=" + i.ToString());
                response = requestToLanguagePage.GetResponse();
                using (sr = new StreamReader(response.GetResponseStream(), System.Text.Encoding.UTF8))
                {
                    html = sr.ReadToEnd();
                }
                sr.Close();

                MatchCollection matchLinks = Regex.Matches(html, @"(?<=<dt class=""thumbnail"">\s+<a\s+title="".+href="").+(?=""><img\s+class=""play_icon"")", RegexOptions.IgnoreCase);

                foreach (Match match in matchLinks)
                {
                    pageLink.Append(url);
                    pageLink.Append(match.Value);
                    pageList.Add(pageLink.ToString());
                    Console.WriteLine(pageLink.ToString());
                    pageLink.Clear();

                }

            }

            return pageList;
        }

        
    }
}
