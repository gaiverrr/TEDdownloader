using System;
using System.Collections.Generic;
using System.Collections;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;

namespace TEDdownloader
{
    public class Language
    {
        private static Hashtable languageHash = new Hashtable();

        public static string GetLanguage(string languageCode)
        {
            if (languageHash.ContainsKey(languageCode))
            {
                return languageHash[languageCode].ToString();
            }
            else return null;
        }

        public static BlockingCollection<string> GetAllLanguageCodes()
        {
            BlockingCollection<string> languagesList = new BlockingCollection<string>();
            foreach (string code in languageHash.Keys)
            {
                languagesList.Add(code);
            }
            return languagesList;
        }
            
        public static bool IsValidLanguageCode(string languageCode)
        {
            if (languageHash.ContainsKey(languageCode))
            {
                return true;
            }
            else return false;
        }

        
        static Language()
        {
            languageHash.Add("afr", "Afrikaans");
            languageHash.Add("alb", "Albanian");
            languageHash.Add("ara", "Arabic");
            languageHash.Add("arm", "Armenian");
            languageHash.Add("asm", "Assamese");
            languageHash.Add("aze", "Azerbaijani");
            languageHash.Add("baq", "Basque");
            languageHash.Add("ben", "Bengali");
            languageHash.Add("bis", "Bislama");
            languageHash.Add("bos", "Bosnian");
            languageHash.Add("bul", "Bulgarian");
            languageHash.Add("bur", "Burmese (Myanmar)");
            languageHash.Add("cat", "Catalan");
            languageHash.Add("chi_hans", "Chinese (Simplified)");
            languageHash.Add("chi_hant", "Chinese (Traditional)");
            languageHash.Add("yue", "Chinese, Yue (Cantonese)");
            languageHash.Add("scr", "Croatian");
            languageHash.Add("cze", "Czech");
            languageHash.Add("dan", "Danish");
            languageHash.Add("dut", "Dutch");
            languageHash.Add("eng", "English");
            languageHash.Add("epo", "Esperanto");
            languageHash.Add("est", "Estonian");
            languageHash.Add("fil", "Filipino (Pilipino)");
            languageHash.Add("fin", "Finnish");
            languageHash.Add("fre_ca", "French (Canada)");
            languageHash.Add("fre_fr", "French (France)");
            languageHash.Add("glg", "Galician");
            languageHash.Add("geo", "Georgian");
            languageHash.Add("ger", "German");
            languageHash.Add("gre", "Greek");
            languageHash.Add("guj", "Gujarati");
            languageHash.Add("hau", "Hausa");
            languageHash.Add("heb", "Hebrew");
            languageHash.Add("hin", "Hindi");
            languageHash.Add("hun", "Hungarian");
            languageHash.Add("ice", "Icelandic");
            languageHash.Add("ind", "Indonesian");
            languageHash.Add("ita", "Italian");
            languageHash.Add("jpn", "Japanese");
            languageHash.Add("kan", "Kannada");
            languageHash.Add("kaz", "Kazakh");
            languageHash.Add("khm", "Khmer (Cambodian)");
            languageHash.Add("kir", "Kirghiz (Kyrgyz)");
            languageHash.Add("kor", "Korean");
            languageHash.Add("lav", "Latvian");
            languageHash.Add("lit", "Lithuanian");
            languageHash.Add("mac", "Macedonian");
            languageHash.Add("may", "Malay");
            languageHash.Add("mal", "Malayalam");
            languageHash.Add("mlt", "Maltese");
            languageHash.Add("mar", "Marathi");
            languageHash.Add("mon", "Mongolian");
            languageHash.Add("nep", "Nepali");
            languageHash.Add("nor", "Norwegian");
            languageHash.Add("nob", "Norwegian, Bokmal");
            languageHash.Add("nno", "Norwegian, Nynorsk");
            languageHash.Add("per", "Persian (Farsi)");
            languageHash.Add("pol", "Polish");
            languageHash.Add("por_br", "Portuguese (Brazil)");
            languageHash.Add("por_pt", "Portuguese (Portugal)");
            languageHash.Add("rum", "Romanian");
            languageHash.Add("rup", "Romanian, Macedo (Aromanian)");
            languageHash.Add("rus", "Russian");
            languageHash.Add("scc", "Serbian");
            languageHash.Add("hbs", "Serbo-Croatian");
            languageHash.Add("slo", "Slovak");
            languageHash.Add("slv", "Slovenian");
            languageHash.Add("spa", "Spanish");
            languageHash.Add("swa", "Swahili");
            languageHash.Add("swe", "Swedish");
            languageHash.Add("tgl", "Tagalog");
            languageHash.Add("tam", "Tamil");
            languageHash.Add("tel", "Telugu");
            languageHash.Add("tha", "Thai");
            languageHash.Add("tur", "Turkish");
            languageHash.Add("ukr", "Ukrainian");
            languageHash.Add("urd", "Urdu");
            languageHash.Add("uzb", "Uzbek");
            languageHash.Add("vie", "Vietnamese");
        }
    }
}


