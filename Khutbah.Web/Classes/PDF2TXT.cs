using Azure;
using Azure.AI.DocumentIntelligence;
using System.Text;
using System.Text.RegularExpressions;

namespace Khutbah.Web.Services
{
    public class PDF2TXT
    {

        public async Task<string> Convert(string selectedFilePath)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();
            System.Diagnostics.Debug.WriteLine($"Config build {sw.ElapsedMilliseconds} ms");
            string endpoint = config["AzureDocumentIntelligence:Endpoint"];
            string apiKey = config["AzureDocumentIntelligence:ApiKey"];

            var client = new DocumentIntelligenceClient(
                new Uri(endpoint),
                new AzureKeyCredential(apiKey));

            byte[] fileBytes = File.ReadAllBytes(selectedFilePath);
            BinaryData data = BinaryData.FromBytes(fileBytes);

            Operation<AnalyzeResult> operation = await client.AnalyzeDocumentAsync(
                WaitUntil.Completed,
                "prebuilt-read",
                data);

            string allText = operation.Value.Content;
            allText = Regex.Replace(allText, "[\u064B-\u0652\u0670]", "");
            allText = string.Join("\n", allText.Split('\n') .Where(line => !Regex.IsMatch(line, "[a-zA-Z]")));
            allText = allText.Replace("الحمد لله", ".الحمد لله");

            System.Diagnostics.Debug.WriteLine($"Generating the text: {sw.ElapsedMilliseconds} ms");
            // Debug only: dump the raw Arabic in txt file so extraction issues can be told
            // apart from translation issues.
            if (config.GetValue<bool>("Debug:SaveExtractedText"))
            {
                string dir = Path.GetDirectoryName(selectedFilePath)!;
                string name = Path.GetFileNameWithoutExtension(selectedFilePath);
                string arabicPath = Path.Combine(dir, name + "_ar.txt");

                File.WriteAllText(arabicPath, allText, new UTF8Encoding(true));
            }

            return allText;
        }
    }
}