#define DEBUG
using System;
using System.Collections.Generic;
using System.Collections;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;


namespace TEDdownloader
{
    class VideoClass
    {

        public String Id { get; set; }
        public String Url { get; set; }
        public int IntroDuration { get; set; }
        public String Filename { get; set; }
        public String DownloadLink { get; set; }
        
        //public String Language { get; set; }    //
        //public String LanguageCode { get; set; }//
        //public String JsonSubtitles { get; set; }  //
        //public String SrtSubtitles { get; set; }  //
        //public String SubtitlesUrl { get; set; } //
        public String VideoFormat { get; set; }

        public BlockingCollection<SubtitlesClass> Subtitles { get; set; }

        
        public bool IsNull()
        {
            if (Filename == null || DownloadLink == null || Subtitles.Count == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public VideoClass(string url)
        {
            Url = url;
            Subtitles = new BlockingCollection<SubtitlesClass>();
        //    DownloadLink = null;
        //    JsonSubtitles = null;
        //    SrtSubtitles = null;
        //    Id = null;
        //    Filename = null;
        //    IntroDuration = 0;
        //    Language = TEDdownloader.Language.GetLanguage(languageCode);
        //    LanguageCode = languageCode;
        }

        public void GetInformation()
        {
            if (GetDownloadLink())
            {
                GetFilename();
                if (GetId())
                {
                    List<string> languageCodes =  TEDdownloader.Language.GetAllLanguageCodes();
                    string json,srt;
                    ParallelOptions parallelOptions = new ParallelOptions();
                    parallelOptions.MaxDegreeOfParallelism = 30;
                    Parallel.ForEach(languageCodes, parallelOptions, languageCode =>
                    {
                        json = GetJsonSubtitles(Id, languageCode);
                        if (!Regex.Match(json, @"There is no translation in", RegexOptions.IgnoreCase).Success)
                        {
                            srt = ConvertJsonSubtitlesToSrt(json, IntroDuration);
                            this.Subtitles.Add(new SubtitlesClass() { Subtitles = srt, LanguageCode = languageCode });
                            Console.WriteLine("id: {0}, langCode: {1}, status: saved", Id, languageCode);
                        }
                        else
                        {
                            Console.WriteLine("id: {0}, langCode: {1}, status: no language", Id, languageCode);
                        }

                    });
                    //foreach (string languageCode in languageCodes)
                    //{
                    //}
                    
//                    JsonSubtitles = GetJsonSubtitles(Id, LanguageCode);
//                    SrtSubtitles = ConvertJsonSubtitlesToSrt(JsonSubtitles, IntroDuration);
                }
                else
                {
                    Console.WriteLine("GetId(): error");
                }
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

                //durationMatch = Regex.Match(html, @"(?<=adDuration:)\d+(?=,)", RegexOptions.IgnoreCase);
                //if (durationMatch.Success)
                //{
                //    IntroDuration = IntroDuration - Convert.ToInt32(durationMatch.Value);
                //}
                //else
                //{
                //    Console.WriteLine("IntroDuration value parsing error");
                //}


                //Get a high resolution mp4 file if avilable. If doesn't get normally resolution.
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

        private bool GetId()
        {
            try
            {
                Id = (DownloadLink.Split('/'))[8];
                return true;
            }

            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            
        }


        private string GetJsonSubtitles(String id, String language)
        {
            string jsonSubtitles;

            SubtitlesUrl = "http://www.ted.com/talks/subtitles/id/" + id + "/lang/" + language;
            try
            {
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

            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        private string ConvertJsonSubtitlesToSrt(string JsonSubtitles, int introDuration)
        {

            if (JsonSubtitles != null && JsonSubtitles.Length != 0)
            {
                JObject jsonSrt = JObject.Parse(JsonSubtitles);
                StringBuilder resultSubtitles = new StringBuilder();
                int captionIndex = 1;

                JToken jsonCaption = jsonSrt["captions"];

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
                    return null;
                    //resultSubtitles.Append("null");
                }
                return resultSubtitles.ToString();

            }
            else
            {
                Console.WriteLine("ConvertJsonSubtitlesToSrt(): JsonSubtitles is null object");
                return null;
            }
            
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
            try
            {
                string srtFilename = Filename.Split('.')[0];
                string directoryPath = Directory.GetCurrentDirectory() + folder + LanguageCode + "/";
                Directory.CreateDirectory(directoryPath);
                if (Directory.Exists(directoryPath))
                {
                    using (StreamWriter sw = new StreamWriter(directoryPath + srtFilename + ".srt", false, Encoding.UTF8))
                    {
                        lock (sw)
                        {
                            sw.Write(SrtSubtitles);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Error");
                }
            }

            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void SaveDownloadLinkToFile(string folder)
        {
            try
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
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

            }
        }
   }

    
}

