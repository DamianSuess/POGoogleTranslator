using System.IO;
using Karambolo.PO;

namespace Shared
{
	public static class Shared
	{
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
}
