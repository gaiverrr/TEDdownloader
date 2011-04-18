#define MONGO
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MongoDB.Bson;
using TEDdownloader.Model;


namespace TEDdownloader
{

    class Program
    {
        static int _countOfErrors = 0;
        static readonly List<VideoClass> ErrorVideoList = new List<VideoClass>();

        static void Main(string[] args)
        {
            //string connectionString = "mongodb://localhost/?safe=true";
            if (args.Length > 0)
            {
                if (Language.IsValidLanguageCode(args[0]))
                {
                    string languageCode = args[0];
                    string language;

                    if (languageCode == "all")
                        language = languageCode;
                    else
                        language = Language.GetLanguage(languageCode);

                    Console.WriteLine("Current language: {0}", language);
                    var pageList = new List<string>();

#if DEBUG
                    string directoryPath = Directory.GetCurrentDirectory();
                    using (StreamReader sr = new StreamReader(directoryPath + "/urls_test.txt", Encoding.UTF8))
                    {
                        string url;
                        while ((url = sr.ReadLine()) != null)
                        {
                            pageList.Add(url);
                        }
                    }
#else
                    pageList = GeneratePageList(languageCode);
                    //string directoryPath = Directory.GetCurrentDirectory();
                    //foreach (string str in pageList)
                    //{
                    //    using (StreamWriter sw = new StreamWriter(directoryPath + "/urls4.txt", true, Encoding.UTF8))
                    //    {
                    //        sw.Write(str);
                    //        sw.Write("\n");
                    //return;
                    
#endif
                    //MongoServer server = MongoServer.Create(connectionString);
                    //server.Connect();
                    //MongoDatabase db = server.GetDatabase("db");
                    //MongoCollection<string> employees = db.GetCollection<string>("test");

                    //MongoCollection<BsonDocument> books = db.GetCollection<BsonDocument>("test");
                    //BsonDocument book = new BsonDocument {
                    //    { "author", "Ernest Hemingway" },
                    //    { "title", "For Whom the Bell Tolls" }
                    //};
                    //books.Insert(book);



                    var vcList = new BlockingCollection<VideoClass>();
                    foreach (string url in pageList)
                    {
                        vcList.Add(new VideoClass(url, languageCode));
                    }


                    var session = new MongoSession();

                    //var categories = session.All<VideoClass>().AsEnumerable();
                    //foreach (var videoClass in categories)
                    //{
                    //    Console.WriteLine(videoClass.ToString());
                    //}



                    var parallelOptions = new ParallelOptions {MaxDegreeOfParallelism = 10};

                    Parallel.ForEach(vcList, parallelOptions, video =>
                    {
                        Console.WriteLine("GetInformation()");
                        video.GetInformation();
                        var bd = new BsonDocument();
                        bd = video.ToBsonDocument();
                            
                        if (!video.IsNull())
                        {
                            video.SaveSrtToFile(@"\srt\");
                            video.SaveDownloadLinkToFile(@".\srt\");

                            /*                            session.DeleteAll<VideoClass>();

                                                        session.Save<VideoClass>(video);
                                                        using (var db = Norm.Mongo.Create("mongodb://localhost/test"))
                                                        {
                                                            //var vc = db.GetCollection<VideoClass>().AsQueryable().ToList();
                                                            categories = session.All<VideoClass>().AsEnumerable();


                                                            //foreach (var videoclass in vc.

                                                        }
                           

                                                        List<VideoClass> ls = new List<VideoClass>();
                                                        ls = db.GetCollection<VideoClass>().Find();
                                                        ls = db.GetCollection<VideoClass>().AsQueryable()
                                                            .Where(vd => vd.DownloadLink == video.DownloadLink)
                                                            .ToList();
                            */
                        }
                        else
                        {
                            ErrorVideoList.Add(video);
                            _countOfErrors++;

                            using (var sw = new StreamWriter("ErrorList.txt", true, Encoding.UTF8))
                            {
                                lock (sw)
                                {
                                    sw.Write(video.Url);
                                    sw.Write("\n");
                                }
                            }
                            Console.WriteLine("Empty object");
                        }
                    });

                    if (ErrorVideoList.Count > 0)
                    {
                        foreach (VideoClass video in ErrorVideoList)
                        {
                            video.GetInformation();

                            if (!video.IsNull())
                            {
                                video.SaveSrtToFile(@"\srt\");
                                video.SaveDownloadLinkToFile(@".\srt\");
                            }
                        }
                    }

                }
                else
                {
                    Console.WriteLine("Didn't find language, please input code language in correct format \n");
                    Console.WriteLine("You can find all languages codes on page: http://www.ted.com/pages/view/id/286");
                }

                Console.WriteLine("Count of errors: {0}", _countOfErrors);
                Console.WriteLine("Finished. Press any key.");
                Console.ReadKey(true);
                return;
            }
            else
            {
                Console.WriteLine("Please launch application with language code parametr");
                Console.ReadKey(true);
                return;
            }

            //This is main case for save content to mongo repository
            //------------------------------------------------------------------------------------------------------------------------------            
            //RepositoryClass repository = new RepositoryClass();
            //repository.Db();

        }



        static List<String> GeneratePageList(string languageCode)
        {
            WebRequest request;
            if (languageCode != "all")
                request = WebRequest.Create("http://www.ted.com/talks?lang=" + languageCode + "&page=1");
            else
                request = WebRequest.Create("http://www.ted.com/talks?page=1");
            string html = null;
            using (WebResponse response = request.GetResponse())
            {
                if (response != null)
                    using (var sr = new StreamReader(response.GetResponseStream(), System.Text.Encoding.UTF8))
                    {
                        html = sr.ReadToEnd();
                    }
            }

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
            Console.WriteLine("Count of video files in {0} language are {1}", Language.GetLanguage(languageCode), countOfFiles);


            int countOfPage;
            if (countOfFiles % 10 == 0)
                countOfPage = countOfFiles / 10;
            else
                countOfPage = (countOfFiles / 10) + 1;

            var pageLink = new StringBuilder();
            var pageList = new List<string>();
            const string url = "http://www.ted.com";
            
            for (int i = 1; i <= countOfPage; i++)
            {
                Console.WriteLine("Current page:{0} from: {1}, remaining: {2}", i, countOfPage, countOfPage - i);
                request = WebRequest.Create("http://www.ted.com/talks?lang=" + languageCode + "&page=" + i);
                using (WebResponse response = request.GetResponse())
                {
                    if (response != null)
                        using (var sr = new StreamReader(response.GetResponseStream(), System.Text.Encoding.UTF8))
                        {
                            html = sr.ReadToEnd();
                        }
                }

                if (html != null)
                {
                    MatchCollection matchLinks = Regex.Matches(html, @"(?<=<dt class=""thumbnail"">\s+<a\s+title="".+href="").+(?=""><img\s+class=""play_icon"")", RegexOptions.IgnoreCase);

                    foreach (Match match in matchLinks)
                    {
                        pageLink.Append(url);
                        pageLink.Append(match.Value);
                        pageList.Add(pageLink.ToString());
                        //Console.WriteLine(pageLink.ToString());
                        pageLink.Clear();

                    }
                }
            }

            return pageList;
        }


    }
}
