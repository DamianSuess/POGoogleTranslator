/*
Note: You'll need to add these NuGet packages to your project:
- Argu
- Google.Cloud.Translation.V2
- Karambolo.PO

The C# implementation follows the same logic as the F# code but uses C# idioms and patterns. The main differences are:
1. Using classes instead of discriminated unions for the CLI arguments
2. Using switch expressions instead of pattern matching
3. Using LINQ for collection operations
4. More explicit type declarations
5. Different syntax for null handling and option types

The functionality remains identical to the original F# code.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Argu;
using Google.Cloud.Translation.V2;
using Karambolo.PO;

public class Program
{
    public static int Main(string[] args)
    {
        try
        {
            var parser = ArgumentParser.Create<CliArguments>();
            var results = parser.Parse(args);

            if (results.Contains(CliArguments.Verbose))
            {
                Console.WriteLine("Verbose mode enabled");
            }

            if (results.Contains(CliArguments.Set_Need_Translation) || results.Contains(CliArguments.Clear_Need_Translation))
            {
                if (!results.TryGetResult(CliArguments.Source, out var sourceFile) ||
                    !results.TryGetResult(CliArguments.Output, out var outputFile))
                {
                    Console.WriteLine("Source and output files must be specified for this operation");
                    return 1;
                }

                var catalog = ParseCatalog(sourceFile);

                if (results.Contains(CliArguments.Set_Need_Translation))
                {
                    catalog = SetNeedTranslation(catalog);
                }
                else
                {
                    catalog = ClearNeedTranslation(catalog);
                }

                using (var stream = new FileStream(outputFile, FileMode.Create))
                {
                    POWriter.Write(stream, catalog);
                }

                return 0;
            }

            if (!results.TryGetResult(CliArguments.Source, out var inputFile) ||
                !results.TryGetResult(CliArguments.Output, out var outputFile) ||
                !results.TryGetResult(CliArguments.Language, out var language) ||
                !results.TryGetResult(CliArguments.ApiKey, out var apiKey))
            {
                Console.WriteLine("Missing required arguments");
                return 1;
            }

            var firstNumbers = results.TryGetResult(CliArguments.First, out var first) ? first : (int?)null;

            var catalogToTranslate = ParseCatalog(inputFile);

            if (!results.Contains(CliArguments.Verbose) &&
                !Confirm($"Are you sure you want to translate {inputFile} to {language}?"))
            {
                return 0;
            }

            AutomaticallyTranslate(catalogToTranslate, apiKey, language, firstNumbers);

            using (var stream = new FileStream(outputFile, FileMode.Create))
            {
                POWriter.Write(stream, catalogToTranslate);
            }

            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    private static bool Confirm(string title)
    {
        Console.Write($"{title} [yes/y/no/n] ");
        var response = Console.ReadLine()?.Trim().ToLower();

        switch (response)
        {
            case "y":
            case "yes":
                return true;
            case "n":
            case "no":
                return false;
            default:
                Console.WriteLine("Invalid input. Please enter 'yes' or 'no'.");
                return Confirm(title);
        }
    }

    private static POCatalog ParseCatalog(string fileName)
    {
        var parser = new POParser();
        using (var stream = new FileStream(fileName, FileMode.Open))
        {
            return parser.Parse(stream);
        }
    }

    private static Func<string, string> CreateTranslator(string key, string targetLanguage)
    {
        var client = TranslationClient.CreateFromApiKey(key);
        return text => client.TranslateText(text, targetLanguage).TranslatedText;
    }

    private static void AutomaticallyTranslate(POCatalog catalog, string key, string targetLanguage, int? firstNumbers)
    {
        catalog.Language = targetLanguage;
        var translator = CreateTranslator(key, targetLanguage);

        var items = catalog.Where(x => string.IsNullOrEmpty(x[0]));

        if (firstNumbers.HasValue)
        {
            items = items.Take(firstNumbers.Value);
        }

        foreach (var item in items)
        {
            try
            {
                if (item is POSingularEntry pse)
                {
                    var translation = translator(item.Key.Id);
                    pse.Translation = translation;

                    var flagComment = pse.Comments.FirstOrDefault(c => c.Kind == POCommentKind.Flags) as POFlagsComment;

                    if (flagComment == null)
                    {
                        flagComment = new POFlagsComment
                        {
                            Flags = new HashSet<string>()
                        };
                        pse.Comments.Add(flagComment);
                    }

                    flagComment.Flags.Add("fuzzy");
                }
                else
                {
                    Console.WriteLine($"Only singular entries are supported. Key {item.Key.Id} skipped.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error translating key {item.Key.Id}: {ex.Message}");
            }
        }
    }

    private static POCatalog SetNeedTranslation(POCatalog catalog)
    {
        foreach (var item in catalog)
        {
            POFlagsComment flagComment = null;

            switch (item)
            {
                case POSingularEntry pse:
                    flagComment = pse.Comments.FirstOrDefault(c => c.Kind == POCommentKind.Flags) as POFlagsComment;
                    if (flagComment == null)
                    {
                        flagComment = new POFlagsComment
                        {
                            Flags = new HashSet<string>()
                        };
                        pse.Comments.Add(flagComment);
                    }
                    break;

                case POPluralEntry ppe:
                    flagComment = ppe.Comments.FirstOrDefault(c => c.Kind == POCommentKind.Flags) as POFlagsComment;
                    if (flagComment == null)
                    {
                        flagComment = new POFlagsComment
                        {
                            Flags = new HashSet<string>()
                        };
                        ppe.Comments.Add(flagComment);
                    }
                    break;

                default:
                    Console.WriteLine($"Only singular and plural entries are supported. Key {item.Key.Id} skipped.");
                    continue;
            }

            flagComment.Flags.Add("fuzzy");
        }

        return catalog;
    }

    private static POCatalog ClearNeedTranslation(POCatalog catalog)
    {
        foreach (var item in catalog)
        {
            POFlagsComment flagComment = null;

            switch (item)
            {
                case POSingularEntry pse:
                    flagComment = pse.Comments.FirstOrDefault(c => c.Kind == POCommentKind.Flags) as POFlagsComment;
                    break;

                case POPluralEntry ppe:
                    flagComment = ppe.Comments.FirstOrDefault(c => c.Kind == POCommentKind.Flags) as POFlagsComment;
                    break;

                default:
                    Console.WriteLine($"Only singular and plural entries are supported. Key {item.Key.Id} skipped.");
                    continue;
            }

            flagComment?.Flags.Remove("fuzzy");
        }

        return catalog;
    }
}

public class CliArguments
{
    [Argu.ArguParserConstructor]
    public CliArguments()
    {
        Verbose = false;
        Output = null;
        Source = null;
        Language = null;
        ApiKey = null;
        First = null;
        SetNeedTranslation = false;
        ClearNeedTranslation = false;
    }

    [AltCommandLine("-v")]
    public bool Verbose { get; set; }

    [AltCommandLine("-o")]
    public string Output { get; set; }

    [AltCommandLine("-s")]
    public string Source { get; set; }

    [AltCommandLine("-l")]
    public string Language { get; set; }

    [AltCommandLine("-k")]
    public string ApiKey { get; set; }

    [AltCommandLine("first")]
    public int? First { get; set; }

    [AltCommandLine("set-need-translation")]
    public bool SetNeedTranslation { get; set; }

    [AltCommandLine("clear-need-translation")]
    public bool ClearNeedTranslation { get; set; }
}
