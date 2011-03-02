#define DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;



namespace TEDdownloader
{
    class VideoClass
    {

        public String Id { get; set; }
        public String Url { get; set; }
        public String Language { get; set; }
        public String LanguageCode { get; set; }
        public String Filename { get; set; }
        public String DownloadLink { get; set; }
        public String JsonSubtitles { get; set; }
        public String SrtSubtitles { get; set; }
        public String VideoFormat { get; set; }
        public String SubtitlesUrl { get; set; }
        public int IntroDuration { get; set; }

        public bool IsNull()
        {
            if (Filename == null || DownloadLink == null || SrtSubtitles == null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public VideoClass(string url, string language, string langCode)
        {
            Url = url;
            DownloadLink = null;
            JsonSubtitles = null;
            SrtSubtitles = null;
            Id = null;
            Filename = null;
            IntroDuration = 0;
            Language = language;
            LanguageCode = langCode;
        }

        public void GetInformation()
        {

            if (GetDownloadLink())
            {
                GetFilename();
                GetId();
                JsonSubtitles = GetJsonSubtitles(Id, LanguageCode);
                SrtSubtitles = ConvertJsonSubtitlesToSrt(JsonSubtitles, IntroDuration);
                Console.WriteLine("{0}: finished", Filename);
            }
            else
            {
                Console.WriteLine("GedDownloadLink(): error");
            }
        }

        private void GetFilename()
        {
            WebRequest request = WebRequest.Create(DownloadLink);
            try
            {
                using (WebResponse response = request.GetResponse())
                {
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
            }

            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            
        }


        private bool GetDownloadLink()
        {
            WebRequest requestToVideoPage = WebRequest.Create(Url);
            try
            {
                string html;
                using (WebResponse response = requestToVideoPage.GetResponse())
                {
                    using (StreamReader sr = new StreamReader(response.GetResponseStream(), System.Text.Encoding.UTF8))
                    {
                        html = sr.ReadToEnd();
                    }
                }

                //This regexp get a Introduration variable from page's html
                Match durationMatch = Regex.Match(html, @"(?<=introDuration:)\d+(?=,)", RegexOptions.IgnoreCase);
                if (durationMatch.Success)
                {
                    IntroDuration = Convert.ToInt32(durationMatch.Value);
                }
                else
                {
                    Console.WriteLine("IntroDuration value parsing error");
                }

                durationMatch = Regex.Match(html, @"(?<=adDuration:)\d+(?=,)", RegexOptions.IgnoreCase);
                if (durationMatch.Success)
                {
                    IntroDuration = IntroDuration - Convert.ToInt32(durationMatch.Value);
                }
                else
                {
                    Console.WriteLine("IntroDuration value parsing error");
                }


                //Get a hight resolution mp4 file if avilable. If doesn't get normally resolution.
                StringBuilder videoLink = new StringBuilder();
                videoLink.Append("http://www.ted.com");

                MatchCollection links = Regex.Matches(html, @"/talks/download/video(.*?)(?="")", RegexOptions.IgnoreCase);
                if (links.Count == 3)
                {
                    videoLink.Append(links[1].Value);
                }
                else if (links.Count == 2 || links.Count == 1)
                {
                    videoLink.Append(links[0].Value);
                }

                DownloadLink = videoLink.ToString();
                return true;
            }

            catch (TimeoutException e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
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
            using (WebResponse response = request.GetResponse())
            {
                using (StreamReader sr = new StreamReader(response.GetResponseStream(), System.Text.Encoding.UTF8))
                {
                    jsonSubtitles = sr.ReadToEnd();
                }
            }
            return jsonSubtitles;
        }

        private string ConvertJsonSubtitlesToSrt(String JsonSubtitles, int introDuration)
        {

            JObject jsonSrt = JObject.Parse(JsonSubtitles);
            StringBuilder resultSubtitles = new StringBuilder();
            int captionIndex = 1;

            JToken jsonCaption  = jsonSrt["captions"];

            if (jsonCaption != null)
            {
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
            }
            else
            {
                Console.WriteLine("Can't find subtitles on ted.com");
                resultSubtitles.Append("null");
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
            
            if (JsonSubtitles == null)
                Console.WriteLine("jsonSubtitlesLength={0}", "");
            else
                Console.WriteLine("jsonSubtitlesLength={0}", JsonSubtitles.Length.ToString());

            if (SrtSubtitles == null)
                Console.WriteLine("srtSubtitles={0}", "");
            else
                Console.WriteLine("srtSubtitles={0}", SrtSubtitles.Length.ToString());

            
            Console.WriteLine("id={0}", Id);
            Console.WriteLine("duration={0}", IntroDuration.ToString());
            Console.WriteLine("---------------------------------------------------------------------------");
            return base.ToString();
        }

        public void SaveSrtToFile(string folder)
        {
            
            string srtFilename = Filename.Split('.')[0];
            string directoryPath = Directory.GetCurrentDirectory() + folder + LanguageCode + "/";
            Directory.CreateDirectory(directoryPath);
            if (Directory.Exists(directoryPath))
            {
                using (StreamWriter sw = new StreamWriter(directoryPath + srtFilename + ".srt", false, Encoding.UTF8))
                {
                    sw.Write(SrtSubtitles);
                }
            }
            else
            {
                Console.WriteLine("Error");   
            }
        }

        public void SaveDownloadLinkToFile(string folder)
        {
            string directoryPath = Directory.GetCurrentDirectory() + folder + LanguageCode + "/";
            using (StreamWriter sw = new StreamWriter(directoryPath + "/downloadList.txt", true, Encoding.UTF8))
            {
                lock (sw)
                {
                    sw.Write(DownloadLink);
                    sw.Write("\n");
                }
            }
        }


   }

    
}

