using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;



namespace TEDdownloader
{
    class VideoClass
    {

        private String Id { get; set; }
        private String Url { get; set; }
        private String Filename { get; set; }
        private String DownloadLink {get; set; }
        private String JsonSubtitlesRus { get; set; }
        private String SrtSubtitlesRus { get; set; }
        private String JsonSubtitlesEng { get; set; }
        private String SrtSubtitlesEng { get; set; }
        private String VideoFormat { get; set; }
        private String SubtitlesUrl { get; set; }
        
        private int IntroDuration { get; set; }


        public VideoClass(String url)
        {
            Url = url;
            DownloadLink = null;
            JsonSubtitlesRus = null;
            SrtSubtitlesRus = null;
            JsonSubtitlesEng = null;
            SrtSubtitlesEng = null;
            Id = null;
            Filename = null;
            IntroDuration = 0;
        }

        public void GetInformation()
        {
            

            GetDonwloadLink();
            GetFilename();
            GetId();
            
            JsonSubtitlesRus = GetJsonSubtitles(Id, "rus");
            JsonSubtitlesEng = GetJsonSubtitles(Id, "eng");
            SrtSubtitlesRus = ConvertJsonSubtitlesToSrt(JsonSubtitlesRus, IntroDuration);
            SrtSubtitlesEng = ConvertJsonSubtitlesToSrt(JsonSubtitlesEng, IntroDuration);

            string srtFileName = Url.Split('/')[6].Remove(Url.Split('/')[6].Length - 5) + ".srt";

            StreamWriter sw; // объект потока для записи
            if (!File.Exists(srtFileName))
            {

                //using (sw = new StreamWriter(srtFileName, true, Encoding.UTF8))
                //{

                //    sw.Write(SrtSubtitlesRus.ToString());
                //    // запись сформированного списка строк
                //    // сбрасываем буфера и даем доступ к файлу
                //    sw.Close();
                //}

            }
            else
            {
                Console.WriteLine("Skip \n");



                //using (sw = new StreamWriter("DownloadList", true, Encoding.UTF8)) ////УДАЛИТЬ КУСОК
                //{

                //    sw.Write(DownloadLink);
                //    sw.Write("\n");
                //    // запись сформированного списка строк
                //    // сбрасываем буфера и даем доступ к файлу
                //    sw.Close();
                //} ///END
            }

           



        }

        private void GetFilename()
        {
            WebRequest request = WebRequest.Create(DownloadLink);
            request.Method = "GET";
            WebResponse response = request.GetResponse();
            if (response.ResponseUri.Segments.Length == 4)
            {
                Filename = response.ResponseUri.Segments[3];
            }
            else if (response.ResponseUri.Segments.Length == 3)
            {
                Filename = response.ResponseUri.Segments[2];
            }
            else if (response.ResponseUri.Segments.Length == 2)
            {
                Filename = response.ResponseUri.Segments[1];
            }

            

        }


        private void GetDonwloadLink()
        {
            HtmlWeb TED = new HtmlWeb();
            StringBuilder videoLink = new StringBuilder();

            HtmlDocument videoPage = TED.Load(Url);
            List<HtmlNode> videoLinks = null;
            videoLinks = (from HtmlNode node in videoPage.DocumentNode.SelectNodes("//a[@href]")
                          where node.Name == "a"
                          && node.Attributes["href"].Value.StartsWith("/talks/download/video")
                          select node).ToList();
            videoLink.Append("http://www.ted.com");
            if (videoLinks.Count > 3)    //Shit!!!!!!!!!!!!!!!!!
            {
                videoLink.Append(videoLinks[2].Attributes["href"].Value);

            }
            else
            {
                videoLink.Append(videoLinks[1].Attributes["href"].Value);
            }

            DownloadLink = videoLink.ToString();
            videoLink.Clear();


            videoLinks = (from HtmlNode node in videoPage.DocumentNode.SelectNodes("//script[@type]")
                          where node.Name == "script"
                          && node.Attributes["type"].Value.StartsWith("text/javascript")
                          select node).ToList();
            IntroDuration = Convert.ToInt32(Regex.Split(videoLinks[20].InnerHtml, ";introDuration=")[1].Split('&')[0]);
            return;


        }

        private void GetId()
        {
            Id = (DownloadLink.Split('/'))[8];
        }


        private string GetJsonSubtitles(String id, String language)
        {
            string _jsonSubtitles;

            SubtitlesUrl = "http://www.ted.com/talks/subtitles/id/" + id + "/lang/" + language;


            WebRequest request = WebRequest.Create(SubtitlesUrl);
            request.Method = "GET";
            WebResponse response = request.GetResponse();
            StreamReader sr = new StreamReader(response.GetResponseStream(), System.Text.Encoding.UTF8);
            _jsonSubtitles = sr.ReadToEnd();
            sr.Close();
            response.Close();

            return _jsonSubtitles;
        }

        private string ConvertJsonSubtitlesToSrt(String JsonSubtitles, int introDuration)
        {

            JObject jsonSrt = JObject.Parse(JsonSubtitles);
            StringBuilder resultSubtitles = new StringBuilder();
            int captionIndex = 1;

            List<JObject> resultObjects = jsonSrt["captions"].Children<JObject>().ToList();
            foreach (JObject str in resultObjects)
            {
                resultSubtitles.Append(captionIndex.ToString());
                resultSubtitles.Append("\n");
                resultSubtitles.Append(formatTime(introDuration + Convert.ToInt32(str["startTime"].ToString()))); //Start phrase time
                resultSubtitles.Append(" --> ");
                resultSubtitles.Append(formatTime(introDuration + Convert.ToInt32(str["startTime"].ToString()) + Convert.ToInt32(str["duration"].ToString()))); //End phrase time
                resultSubtitles.Append("\n");
                resultSubtitles.Append((String)str["content"]);
                resultSubtitles.Append("\n");
                captionIndex++;
            }

            return resultSubtitles.ToString();
        }

        private string formatTime(int time)
        {
            //String milliseconds = "0";
            StringBuilder formattedTime = new StringBuilder();

            formattedTime.Append((time / 3600000).ToString("00")); //Hours
            formattedTime.Append(":");
            formattedTime.Append((time / 60000).ToString("00")); //Minutes
            formattedTime.Append(":");
            formattedTime.Append(((time / 1000) % 60).ToString("00")); //Seconds
            formattedTime.Append(",");
            formattedTime.Append((time % 1000).ToString("000")); //Minutes
            //formattedTime.Append("000"); //Milliseconds
            return formattedTime.ToString();
        }

        public override string ToString()
        {
            Console.WriteLine("url={0}", Url);
            Console.WriteLine("downloadLink={0}", DownloadLink);
            //Console.WriteLine("language={0}", Language);
            Console.WriteLine("SubtitlesUrl={0}", SubtitlesUrl);
            if (JsonSubtitlesRus == null)
                Console.WriteLine("jsonSubtitlesLength={0}", "");
            else
                Console.WriteLine("jsonSubtitlesLength={0}", JsonSubtitlesRus.Length.ToString());
            if (SrtSubtitlesRus == null)
                Console.WriteLine("srtSubtitles={0}", "");
            else
                Console.WriteLine("srtSubtitles={0}", SrtSubtitlesRus.Length.ToString());

            Console.WriteLine("id={0}", Id);
            Console.WriteLine("duration={0}", IntroDuration.ToString());

            return base.ToString();
        }

        public void SaveSrtToFile(string path)
        {
            StreamWriter sw; // объект потока для записи
            string srtFilename = Filename.Split('.')[0];

            using (sw = new StreamWriter(path + srtFilename + ".srt", true, Encoding.UTF8))
            {
                 sw.Write(SrtSubtitlesRus);
            }
            sw.Close();
        }

        public void SaveDonwloadLinkToFile(string path)
        {
            StreamWriter sw; // объект потока для записи
            
            using (sw = new StreamWriter(path, true, Encoding.UTF8))
            {
                sw.Write(DownloadLink);
                sw.Write("\n");
            }
            sw.Close();
        }
   }

    
}

