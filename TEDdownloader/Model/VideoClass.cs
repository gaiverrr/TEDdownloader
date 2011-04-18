#define DEBUG
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Norm;



namespace TEDdownloader.Model
{
    class VideoClass
    {

        //[MongoIdentifier]
        //public ObjectId ID { get; set; }

        [MongoIdentifier]
        public string VideoId { get; set; }

        public String Url { get; set; }
        public int IntroDuration { get; set; }
        public String Filename { get; set; }
        public String DownloadLink { get; set; }
        
        public String Language { get; set; }    
        public String LanguageCode { get; set; }
        public String VideoFormat { get; set; }
        public String TitleName { get; set; }
        public BlockingCollection<SubtitlesClass> Subtitles { get; set; }

        
        public bool IsNull()
        {
            if (Filename == null || DownloadLink == null || Subtitles.Count == 0)
                return true;
            return false;
        }

        public VideoClass()
        {
            Subtitles = new BlockingCollection<SubtitlesClass>();        
        }

        public VideoClass(string url, string languageCode)
        {
            Url = url;
            Subtitles = new BlockingCollection<SubtitlesClass>();
            Language = TEDdownloader.Language.GetLanguage(languageCode);
            LanguageCode = languageCode;
        }

        public void GetInformation()
        {
            if (GetDownloadLink())
            {
                GetFilename();
                if (GetId())
                {
                    var languageCodes = new BlockingCollection<string>();
                    if (LanguageCode == "all")
                        languageCodes = TEDdownloader.Language.GetAllLanguageCodes();
                    else
                        languageCodes.Add(LanguageCode);

                    var parallelOptions = new ParallelOptions {MaxDegreeOfParallelism = 10};
                    Parallel.ForEach(languageCodes, parallelOptions, languageCode =>
                                                                         {
                                                                             string json = GetJsonSubtitles(VideoId, languageCode);
                                                                             if (json != null)
                                                                             {
                                                                                 if (!Regex.Match(json, @"There is no translation in", RegexOptions.IgnoreCase).Success)
                                                                                 {
                                                                                     string srt = ConvertJsonSubtitlesToSrt(json, IntroDuration);
                                                                                     Subtitles.Add(new SubtitlesClass { Subtitles = srt, LanguageCode = languageCode });
                                                                                     Console.WriteLine("id: {0}, langCode: {1}, status: saved", VideoId, languageCode);
                                                                                 }
                                                                             }
                                                                             else
                                                                                 Console.WriteLine("id: {0}, langCode: {1}, status: no language", VideoId, languageCode);
                                                                         });
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
                    if (response != null)
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
                string html = null;
                using (WebResponse response = requestToVideoPage.GetResponse())
                {
                    if (response != null)
                        using (var sr = new StreamReader(response.GetResponseStream(), System.Text.Encoding.UTF8))
                        {
                            html = sr.ReadToEnd();
                        }
                }

                //This regexp get a Introduration variable from page's html
                if (html != null)
                {
                    Match durationMatch = Regex.Match(html, @"(?<=introDuration:)\d+(?=,)", RegexOptions.IgnoreCase);
                    if (durationMatch.Success)
                        IntroDuration = Convert.ToInt32(durationMatch.Value);
                    else
                        Console.WriteLine("IntroDuration value parsing error");

                    //durationMatch = Regex.Match(html, @"(?<=adDuration:)\d+(?=,)", RegexOptions.IgnoreCase);
                    //if (durationMatch.Success)
                    //    IntroDuration = IntroDuration - Convert.ToInt32(durationMatch.Value);
                    //else
                    //    Console.WriteLine("IntroDuration value parsing error");

                    durationMatch = Regex.Match(html, @"(?<=<span id=""altHeadline"">)(.*?)(?=</span></h1>)", RegexOptions.IgnoreCase);
                    if (durationMatch.Success)
                        TitleName = durationMatch.Value;
                    else
                        Console.WriteLine("Title value parsing error");
                }

                //Get a high resolution mp4 file if avilable. If doesn't get normally resolution.
                var videoLink = new StringBuilder();
                videoLink.Append("http://www.ted.com");

                if (html != null)
                {
                    MatchCollection links = Regex.Matches(html, @"/talks/download/video(.*?)(?="")", RegexOptions.IgnoreCase);
                    if (links.Count == 3)
                    {
                        videoLink.Append(links[1].Value);
                    }
                    else if (links.Count == 2 || links.Count == 1)
                    {
                        videoLink.Append(links[0].Value);
                    }
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
                VideoId = (DownloadLink.Split('/'))[8];
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
            string jsonSubtitles = null;

            string subtitlesUrl = "http://www.ted.com/talks/subtitles/id/" + id + "/lang/" + language;
            try
            {
                WebRequest request = WebRequest.Create(subtitlesUrl);
                using (WebResponse response = request.GetResponse())
                {
                    if (response != null)
                        using (var sr = new StreamReader(response.GetResponseStream(), System.Text.Encoding.UTF8))
                        {
                            jsonSubtitles = sr.ReadToEnd();
                        }
                }
                if (jsonSubtitles.Length == 0)
                    return null;
                return jsonSubtitles;
            }

            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        private string ConvertJsonSubtitlesToSrt(string jsonSubtitles, int introDuration)
        {

            if (!string.IsNullOrEmpty(jsonSubtitles))
            {
                var jsonSrt = JObject.Parse(jsonSubtitles);
                var resultSubtitles = new StringBuilder();
                var captionIndex = 1;

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
            Console.WriteLine("id: {0}, langCode: {1}, status: not saved, JsonSubtitles is null object", VideoId, LanguageCode);
            //Console.WriteLine("ConvertJsonSubtitlesToSrt(): JsonSubtitles is null object");
            return null;
        }

        private string formatTime(int time)
        {
            //String milliseconds = "0";
            var formattedTime = new StringBuilder();

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
            Console.WriteLine("id={0}", VideoId);
            Console.WriteLine("duration={0}", IntroDuration);
            Console.WriteLine("---------------------------------------------------------------------------");
            return base.ToString();
        }

        public void SaveSrtToFile(string folder)
        {
            try
            {
                string srtFilename = Filename.Split('.')[0];


                foreach (SubtitlesClass subtitles in Subtitles)
                {
                    if (subtitles != null)
                    {
                        string directoryPath = Directory.GetCurrentDirectory() + folder + subtitles.LanguageCode + "/";
                        Directory.CreateDirectory(directoryPath);

                        if (Directory.Exists(directoryPath))
                        {
                            using (var sw = new StreamWriter(directoryPath + srtFilename + ".srt", false, Encoding.UTF8))
                            {
                                lock (sw)
                                {
                                    sw.Write(subtitles.Subtitles);
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("Error");
                        }
                    }
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
                string directoryPath;
                if (LanguageCode == "all")
                    directoryPath = Directory.GetCurrentDirectory() + folder + "/";
                else 
                    directoryPath = Directory.GetCurrentDirectory() + folder + LanguageCode + "/";
                if (Directory.Exists(directoryPath))
                {
                    using (var sw = new StreamWriter(directoryPath + "/downloadList.txt", true, Encoding.UTF8))
                    {
                        lock (sw)
                        {
                            sw.Write(DownloadLink);
                            sw.Write("\n");
                        }
                    }
                }
                else Console.WriteLine("Directory doesn't exist");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

            }
        }

   }

    
}

