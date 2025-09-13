/*
Note: You'll need to add these NuGet packages to your project:
- `Karambolo.PO`
- `Argu` (for command line parsing)
- `System.Text.Json` (for JSON handling)

The code assumes the existence of a `Shared` class with the `writePOCatalog` functionality, which I've implemented as `POFileWriter.WritePOCatalog` in C#. The implementation uses Karambolo.PO's built-in methods to save the catalog.
*/
using System;
using System.IO;
using System.Text.Json.Nodes;
using Argu;
using Karambolo.PO;

namespace JsonConversion
{
	public class JsonConversionCliArguments
	{
		[Argu.ArguSwitch]
		[AltCommandLine("-v")]
		public bool Verbose { get; set; }

		[Argu.ArguParameter]
		[AltCommandLine("-o")]
		public string Output { get; set; }

		[Argu.ArguParameter]
		[AltCommandLine("-s")]
		public string Source { get; set; }

		[Argu.ArguParameter]
		[AltCommandLine("-l")]
		public string Language { get; set; }
	}

	public static class Program
	{
		private static void PopulateNode(JsonNode node, string key, POCatalog catalog)
		{
			switch (node)
			{
				case JsonObject obj:
					foreach (var kvp in obj)
					{
						var value = kvp.Value;
						var newPrefix = string.IsNullOrEmpty(key) ? kvp.Key : $"{key}.{kvp.Key}";
						PopulateNode(value, newPrefix, catalog);
					}
					break;

				case JsonValue obj:
					var entry = new POSingularEntry(new POKey(key));
					entry[0] = obj.ToString();
					catalog.Add(entry);
					break;

				case JsonArray _:
					throw new NotImplementedException("JsonArray is not implemented");

				default:
					throw new NotImplementedException("Unknown JSON value is not implemented");
			}
		}

		public static void ProcessCli(string[] argv)
		{
			var errorHandler = new ProcessExiter(colorizer: code =>
				code == ErrorCode.HelpText ? null : (ConsoleColor?)ConsoleColor.Red);

			var parser = ArgumentParser.Create<JsonConversionCliArguments>(
				programName: "robotranslator fromjson",
				errorHandler: errorHandler);

			var results = parser.ParseCommandLine(argv, ignoreUnrecognized: true);

			if (results == null)
				return;

			var sourceFileName = results.GetResult(JsonConversionCliArguments._, a => a.Source);
			var root = JsonNode.Parse(File.ReadAllText(sourceFileName));
			var catalog = new POCatalog();

			PopulateNode(root, "", catalog);

			var outputFileName = results.GetResult(JsonConversionCliArguments._, a => a.Output);

			if (results.TryGetResult(JsonConversionCliArguments._, a => a.Language, out var language))
			{
				catalog.Language = language;
			}

			Directory.CreateDirectory(Path.GetDirectoryName(outputFileName));
			Shared.POFileWriter.WritePOCatalog(outputFileName, catalog);
		}

		public static void Main(string[] args)
		{
			ProcessCli(args);
		}
	}
}

// Shared.cs (assumed to be in the same namespace)
public static class Shared
{
	public static class POFileWriter
	{
		public static void WritePOCatalog(string outputFileName, POCatalog catalog)
		{
			// Implementation of writePOCatalog from the original F# Shared module
			// This would use Karambolo.PO's built-in methods to write the catalog
			using (var writer = new StreamWriter(outputFileName))
			{
				catalog.Save(writer);
			}
		}
	}
}
