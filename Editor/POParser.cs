using System;
using System.Collections.Generic;
using System.IO;
using Karambolo.PO;

public static class POParser
{
  private static readonly Dictionary<string, string> Iso639Languages = GetIso639Languages();

  private static Dictionary<string, string> GetIso639Languages()
  {
    return new Dictionary<string, string>(StringComparer.Ordinal)
    {
      { "af", "Afrikaans" },
      { "sq", "Albanian" },
      { "am", "Amharic" },
      { "ar", "Arabic" },
      { "hy", "Armenian" },
      { "az", "Azerbaijani" },
      { "eu", "Basque" },
      { "be", "Belarusian" },
      { "bn", "Bengali" },
      { "bs", "Bosnian" },
      { "bg", "Bulgarian" },
      { "ca", "Catalan" },
      { "ceb", "Cebuano" },
      { "zh", "Chinese" },
      { "co", "Corsican" },
      { "hr", "Croatian" },
      { "cs", "Czech" },
      { "da", "Danish" },
      { "nl", "Dutch" },
      { "en", "English" },
      { "eo", "Esperanto" },
      { "et", "Estonian" },
      { "fi", "Finnish" },
      { "fr", "French" },
      { "gl", "Galician" },
      { "ka", "Georgian" },
      { "de", "German" },
      { "el", "Greek" },
      { "gu", "Gujarati" },
      { "ht", "Haitian Creole" },
      { "ha", "Hausa" },
      { "he", "Hebrew" },
      { "hi", "Hindi" },
      { "hu", "Hungarian" },
      { "is", "Icelandic" },
      { "id", "Indonesian" },
      { "ga", "Irish" },
      { "it", "Italian" },
      { "ja", "Japanese" },
      { "kn", "Kannada" },
      { "kk", "Kazakh" },
      { "km", "Khmer" },
      { "ko", "Korean" },
      { "ku", "Kurdish" },
      { "ky", "Kyrgyz" },
      { "lo", "Lao" },
      { "lv", "Latvian" },
      { "lt", "Lithuanian" },
      { "lb", "Luxembourgish" },
      { "mk", "Macedonian" },
      { "mg", "Malagasy" },
      { "ms", "Malay" },
      { "ml", "Malayalam" },
      { "mt", "Maltese" },
      { "mi", "Maori" },
      { "mr", "Marathi" },
      { "mn", "Mongolian" },
      { "ne", "Nepali" },
      { "no", "Norwegian" },
      { "or", "Odia" },
      { "ps", "Pashto" },
      { "fa", "Persian" },
      { "pl", "Polish" },
      { "pt", "Portuguese" },
      { "pa", "Punjabi" },
      { "ro", "Romanian" },
      { "ru", "Russian" },
      { "sm", "Samoan" },
      { "gd", "Scottish Gaelic" },
      { "sr", "Serbian" },
      { "st", "Sesotho" },
      { "sn", "Shona" },
      { "sd", "Sindhi" },
      { "si", "Sinhala" },
      { "sk", "Slovak" },
      { "sl", "Slovenian" },
      { "so", "Somali" },
      { "es", "Spanish" },
      { "su", "Sundanese" },
      { "sw", "Swahili" },
      { "sv", "Swedish" },
      { "tl", "Tagalog" },
      { "tg", "Tajik" },
      { "ta", "Tamil" },
      { "tt", "Tatar" },
      { "te", "Telugu" },
      { "th", "Thai" },
      { "tr", "Turkish" },
      { "tk", "Turkmen" },
      { "uk", "Ukrainian" },
      { "ur", "Urdu" },
      { "ug", "Uyghur" },
      { "uz", "Uzbek" },
      { "vi", "Vietnamese" },
      { "cy", "Welsh" },
      { "xh", "Xhosa" },
      { "yi", "Yiddish" },
      { "yo", "Yoruba" },
      { "zu", "Zulu" },
    };
  }

  public class Translation
  {
    public string Source { get; set; }
    public string TranslationText { get; set; }

    public Translation(IPOEntry entry)
    {
      Source = entry.Key.Id;
      TranslationText = entry[0];
    }
  }

  public static POCatalog ParseCatalog(string fileName)
  {
    var parser = new POParser();
    using (var stream = new FileStream(fileName, FileMode.Open))
    {
      return parser.Parse(stream);
    }
  }

  public static void WritePOCatalog(string fileName, POCatalog catalog)
  {
    var generator = new POGenerator();

    if (catalog.Encoding == null)
    {
      catalog.Encoding = "utf-8";
    }

    using (var writer = new StreamWriter(fileName))
    {
      generator.Generate(writer, catalog);
    }
  }
}
