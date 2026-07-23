using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Azure;
using Azure.AI.DocumentIntelligence;
using Microsoft.Extensions.Configuration;

namespace Khutbah_Frontend
{
    public class PDF2TXT
    {
        public async Task<string> Convert(string selectedFilePath)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

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

            // Debug only: dump the raw Arabic so extraction issues can be told
            // apart from translation issues.
            if (config.GetValue<bool>("Debug:SaveExtractedText"))
            {
                string dir = Path.GetDirectoryName(selectedFilePath);
                string name = Path.GetFileNameWithoutExtension(selectedFilePath);
                string arabicPath = Path.Combine(dir, name + "_ar.txt");

                File.WriteAllText(arabicPath, allText, new UTF8Encoding(true));
            }

            return allText;
        }
    }
}