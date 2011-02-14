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
            WebRequest requestToVideoPage = WebRequest.Create(Url);
            requestToVideoPage.Method = "GET";
            WebResponse response = requestToVideoPage.GetResponse();
            StreamReader sr = new StreamReader(response.GetResponseStream(), System.Text.Encoding.UTF8);
            string html = sr.ReadToEnd();

            //This regexp get a Introduration variable from page's html
            Match durationMatch = Regex.Match(html, @"(?<=introDuration:)\d+(?=,)", RegexOptions.IgnoreCase);
            if (durationMatch.Success)
            {
                IntroDuration = Convert.ToInt32(durationMatch.Value);
            }
            else
            {
                Console.WriteLine("IntroDuration value parsin error");
            }

            
            //Get a hight resolution mp4 file if avilable. If doesn't get normally resolution.
            StringBuilder videoLink = new StringBuilder();
            videoLink.Append("http://www.ted.com");

            MatchCollection links = Regex.Matches(html,@"/talks/download/video(.*?)(?="")" ,RegexOptions.IgnoreCase);
            if (links.Count == 3)
            {
                videoLink.Append(links[2].Value);
            } 
            else if (links.Count == 2 || links.Count == 1)
            {
                videoLink.Append(links[0].Value);
            }
            
            DownloadLink = videoLink.ToString();
            return;
        }

        private void GetId()
        {
            Id = (DownloadLink.Split('/'))[8];
        }


        private string GetJsonSubtitles(String id, String language)
        {
            string jsonSubtitles;

            SubtitlesUrl = "http://www.ted.com/talks/subtitles/id/" + id + "/lang/" + language;


            WebRequest request = WebRequest.Create(SubtitlesUrl);
            request.Method = "GET";
            WebResponse response = request.GetResponse();
            StreamReader sr = new StreamReader(response.GetResponseStream(), System.Text.Encoding.UTF8);
            jsonSubtitles = sr.ReadToEnd();
            sr.Close();
            response.Close();

            return jsonSubtitles;
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
            Console.WriteLine("---------------------------------------------------------------------------");
            return base.ToString();
        }

        public void SaveSrtToFile(string folder)
        {
            
            string srtFilename = Filename.Split('.')[0];
            string directoryPath = Directory.GetCurrentDirectory() + folder;
            Directory.CreateDirectory(directoryPath);
            if (Directory.Exists(directoryPath))
            {
                StreamWriter sw; // объект потока для записи
                using (sw = new StreamWriter(directoryPath + srtFilename + ".srt", true, Encoding.UTF8))
                {
                    sw.Write(SrtSubtitlesRus);
                }
                sw.Close();
            }
            else
            {
                Console.WriteLine("Error");   
            }
        }

        public void SaveDonwloadLinkToFile(string folder)
        {
            StreamWriter sw; // объект потока для записи
            
            using (sw = new StreamWriter(folder, true, Encoding.UTF8))
            {
                sw.Write(DownloadLink);
                sw.Write("\n");
            }
            sw.Close();
        }
   }

    
}

