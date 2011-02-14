using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

namespace TEDdownloader
{
    class LanguagesClass
    {
        private Hashtable languageList = new Hashtable();
        
        public LanguagesClass()
        {
            languageList.Add("afr", "Afrikaans");
            languageList.Add("alb", "Albanian");
            languageList.Add("ara", "Arabic");
            languageList.Add("arm", "Armenian");
            languageList.Add("asm", "Assamese");
            languageList.Add("aze", "Azerbaijani");
            languageList.Add("baq", "Basque");
            languageList.Add("ben", "Bengali");
            languageList.Add("bis", "Bislama");
            languageList.Add("bos", "Bosnian");
            languageList.Add("bul", "Bulgarian");
            languageList.Add("bur", "Burmese (Myanmar)");
            languageList.Add("cat", "Catalan");
            languageList.Add("chi_hans", "Chinese (Simplified)");
            languageList.Add("chi_hant", "Chinese (Traditional)");
            languageList.Add("yue", "Chinese, Yue (Cantonese)");
            languageList.Add("scr", "Croatian");
            languageList.Add("cze", "Czech");
            languageList.Add("dan", "Danish");
            languageList.Add("dut", "Dutch");
            languageList.Add("eng", "English");
            languageList.Add("epo", "Esperanto");
            languageList.Add("est", "Estonian");
            languageList.Add("fil", "Filipino (Pilipino)");
            languageList.Add("fin", "Finnish");
            languageList.Add("fre_ca", "French (Canada)");
            languageList.Add("fre_fr", "French (France)");
            languageList.Add("glg", "Galician");
            languageList.Add("geo", "Georgian");
            languageList.Add("ger", "German");
            languageList.Add("gre", "Greek");
            languageList.Add("guj", "Gujarati");
            languageList.Add("hau", "Hausa");
            languageList.Add("heb", "Hebrew");
            languageList.Add("hin", "Hindi");
            languageList.Add("hun", "Hungarian");
            languageList.Add("ice", "Icelandic");
            languageList.Add("ind", "Indonesian");
            languageList.Add("ita", "Italian");
            languageList.Add("jpn", "Japanese");
            languageList.Add("kan", "Kannada");
            languageList.Add("kaz", "Kazakh");
            languageList.Add("khm", "Khmer (Cambodian)");
            languageList.Add("kir", "Kirghiz (Kyrgyz)");
            languageList.Add("kor", "Korean");
            languageList.Add("lav", "Latvian");
            languageList.Add("lit", "Lithuanian");
            languageList.Add("mac", "Macedonian");
            languageList.Add("may", "Malay");
            languageList.Add("mal", "Malayalam");
            languageList.Add("mlt", "Maltese");
            languageList.Add("mar", "Marathi");
            languageList.Add("mon", "Mongolian");
            languageList.Add("nep", "Nepali");
            languageList.Add("nor", "Norwegian");
            languageList.Add("nob", "Norwegian, Bokmal");
            languageList.Add("nno", "Norwegian, Nynorsk");
            languageList.Add("per", "Persian (Farsi)");
            languageList.Add("pol", "Polish");
            languageList.Add("por_br", "Portuguese (Brazil)");
            languageList.Add("por_pt", "Portuguese (Portugal)");
            languageList.Add("rum", "Romanian");
            languageList.Add("rup", "Romanian, Macedo (Aromanian)");
            languageList.Add("rus", "Russian");
            languageList.Add("scc", "Serbian");
            languageList.Add("hbs", "Serbo-Croatian");
            languageList.Add("slo", "Slovak");
            languageList.Add("slv", "Slovenian");
            languageList.Add("spa", "Spanish");
            languageList.Add("swa", "Swahili");
            languageList.Add("swe", "Swedish");
            languageList.Add("tgl", "Tagalog");
            languageList.Add("tam", "Tamil");
            languageList.Add("tel", "Telugu");
            languageList.Add("tha", "Thai");
            languageList.Add("tur", "Turkish");
            languageList.Add("ukr", "Ukrainian");
            languageList.Add("urd", "Urdu");
            languageList.Add("uzb", "Uzbek");
            languageList.Add("vie", "Vietnamese");
        }

        public string GetLanguageName(string langCode)
        {

            if (languageList.ContainsKey(langCode))
            {
                return languageList[langCode].ToString();
            }
            else return null;
            
        }
        
        
    }
}
