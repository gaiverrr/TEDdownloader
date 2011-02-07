using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using System.Net;
using System.IO;
using MongoDB.Driver;
using MongoDB.Bson;



namespace TEDdownloader
{

    class Program
    {
        static void Main(string[] args)
        {



            //string connectionString = "mongodb://localhost";

            MongoServerSettings mssettings = new MongoServerSettings();
            mssettings.Server = new MongoServerAddress("localhost");
            
            MongoServer ms = new MongoServer(mssettings);
            ms.Connect();

            ms.Disconnect();



            //mssettings.Server = 

            //MongoServer ms = new MongoServer;

            //ms.Connect();

            System.Console.WriteLine("s");


            
            
            //List<String> pageList = new List<string>();
            //pageList = generatePageList();

            //foreach (string str in pageList)
            //{
            //    Console.WriteLine(str.ToString());
            //    VideoClass vc = new VideoClass(str);
            //    vc.GetInformation();
            //    vc.SaveSrtToFile(@"c:\Users\gaiver\Documents\My Dropbox\Projects\TEDdownloader\TEDdownloader\bin\Debug\srt\");
            //    vc.SaveDonwloadLinkToFile(@"c:\Users\gaiver\Documents\My Dropbox\Projects\TEDdownloader\TEDdownloader\bin\Debug\srt\downloadList.txt");

            //    vc.ToString();
            //}



            //List<VideoClass> ListOfVideClass = new List<VideoClass>();



            //This is main case for save content to mongo repository
            //------------------------------------------------------------------------------------------------------------------------------            
            //RepositoryClass repository = new RepositoryClass();
            //repository.Db();
            

            
            //Mongo mongo = new Mongo();
            //mongo.Connect(); //Connect to localhost on the default port
            //Database db = mongo.GetDatabase("test");
            //IMongoCollection tests = db.GetCollection("coll");

            //Document doc = new Document();
            //doc["i1"] = "i1";
            //doc["i2"] = "i2";
            //doc["i3"] = "i3";

            //tests.Insert(doc);

            
            //mongo.Disconnect();



//This is test case for generation text file with videolinks, which have russian subtitles
//------------------------------------------------------------------------------------------------------------------------------
            //StreamWriter sw; // объект потока для записи


            //using (sw = new StreamWriter(@"file.txt", true, Encoding.UTF8))
            //{
            //    foreach (string str in pageList)
            //    {
            //        sw.Write(str.ToString());
            //        sw.Write("\n");
            //    }
            //     // запись сформированного списка строк
            //     //сбрасываем буфера и даем доступ к файлу
            //    sw.Close();
            //}

            //List<String> pageList = new List<string>();
//------------------------------------------------------------------------------------------------------------------------------            


            //This is test case for converting subtitles from text file and save SRT to separate file
            //------------------------------------------------------------------------------------------------------------------------------            
            //StreamReader sr;
            //List<String> pageList = new List<string>();

            //using (sr = new StreamReader(@"file.txt"))
            //{
            //    String line;
            //    // читаем строки до конца файла
            //    while ((line = sr.ReadLine()) != null)
            //    {
            //        pageList.Add(line);
            //    }
            //    sr.Close();
            //}
            //List<String> pg = new List<string>();

            //pg.Add(pageList[0]);
            //pg.Add(pageList[1]);
            //List<VideoClass> ListOfVideClass = new List<VideoClass>();
            //foreach (string str in pageList)
            //{
            //    Console.WriteLine(str.ToString());
            //    VideoClass vc = new VideoClass(str, "rus");
            //    vc.GetInformation();
            //    vc.ToString();




            //}
            //Console.WriteLine(pageList.Count());

            //------------------------------------------------------------------------------------------------------------------------------

            
            
            
            
            
            //List<String> pageList = new List<string>();
            //List<String> pageList_for_testing = new List<String>();
            //List<String> downloadLinkList = new List<string>();

            ////List<VideoClass> videoList = new List<VideoClass>();

            
            
            //pageList = generatePageList();

            //pageList_for_testing.Add(pageList[0]);
            ////pageList_for_testing.Add(pageList[1]);

            //downloadLinkList = generateDownloadList(pageList_for_testing);
            //Console.WriteLine(downloadLinkList[0].ToString());
            ////Console.WriteLine(downloadLinkList[1].ToString());

            //List<String> JSONSubtitles = new List<string>();
            //foreach (string downloadLink in downloadLinkList)
            //{
            //    JSONSubtitles.Add(getTEDSubtitles((downloadLink.Split('/'))[8], "rus"));
                
            //}
        }



        static List<String> generatePageList()
        {
            // Load the html document
            HtmlWeb TED = new HtmlWeb();
            String url = "http://www.ted.com";
            StringBuilder pageLink = new StringBuilder();
            List<String> pageList = new List<string>();

            HtmlDocument linkPage = TED.Load("http://www.ted.com/talks?lang=rus&page=1");

            int countOfFiles = Convert.ToInt16((linkPage.DocumentNode.SelectNodes("//div[contains(@class, 'browser')]")[0].ChildNodes[1].InnerText).Split(' ', '\t')[11]);

            int countOfPage;
            if (countOfFiles % 10 == 0)
                countOfPage = countOfFiles / 10;
            else
                countOfPage = (countOfFiles / 10) + 1;
            
            //countOfPage = 1;
            
            for (int i = 39; i <= countOfPage; i++)
            {
                Console.WriteLine("Current page:{0} from: {1}, remaining: {2}", i, countOfPage, countOfPage-i);
                linkPage = TED.Load("http://www.ted.com/talks?lang=rus&page=" + i.ToString());

                List<HtmlNode> pageLinks = null;
                pageLinks = (from HtmlNode node in linkPage.DocumentNode.SelectNodes("//dt[contains(@class, 'thumbnail')]")
                             where node.ChildNodes[1].Attributes["href"].Value.StartsWith("/talks/")
                             select node).ToList();



                foreach (HtmlNode node in pageLinks)
                {
                    pageLink.Append(url);
                    pageLink.Append(node.ChildNodes[1].Attributes["href"].Value);
                    pageList.Add(pageLink.ToString());


                    //VideoClass vc = new VideoClass(pageLink.ToString(), "rus");
                    //vc.GetInformation();
                    //vc.ToString();


                    Console.WriteLine(pageLink.ToString());
                    pageLink.Clear();

                }

            }

            return pageList;
        }

        //static List<String> generateDownloadList(List<String> pageList)
        //{
        //    List<String> downloadList = new List<string>();
        //    HtmlWeb TED = new HtmlWeb();
        //    String url = "http://www.ted.com";
        //    StringBuilder videoLink = new StringBuilder();

        //    foreach (string href in pageList)
        //    {
        //        HtmlDocument videoPage = TED.Load(href);
        //        List<HtmlNode> videoLinks = null;
        //        videoLinks = (from HtmlNode node in videoPage.DocumentNode.SelectNodes("//a[@href]")
        //                      where node.Name == "a"
        //                      && node.Attributes["href"].Value.StartsWith("/talks/download/video")
        //                      select node).ToList();
        //        videoLink.Append(url);
        //        videoLink.Append(videoLinks[2].Attributes["href"].Value);
        //        downloadList.Add(videoLink.ToString());
        //        videoLink.Clear();
        //    }
        //    return downloadList; 
        //}

        
        //static string getTEDSubtitles(string id, string language)
        //{
        //    Console.WriteLine("id={0}", id);
        //    Console.WriteLine("language={0}", language);

        //    WebRequest request = WebRequest.Create("http://www.ted.com/talks/subtitles/id/" + id + "/lang/" + language);
        //    request.Method = "GET";
        //    WebResponse response = request.GetResponse();
        //    StreamReader sr = new StreamReader(response.GetResponseStream(), System.Text.Encoding.UTF8);
        //    string JSONSubtitles = sr.ReadToEnd();
        //    sr.Close();
        //    response.Close();

        //    Console.WriteLine("Result={0}", JSONSubtitles);

        //    return JSONSubtitles;

        //}

        //static string convertSubtitles(string JSONSubtitles)
        //{
        //    String SRTSubtitles = "";


        //    return SRTSubtitles;
        //}
        //static void convertTEDSubtitlesToSRTSubtitles()
    }
}
