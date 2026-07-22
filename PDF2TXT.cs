using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Azure;
using Azure.AI.DocumentIntelligence;
using Microsoft.Extensions.Configuration;

namespace Khutbah_Frontend
{
    public class PDF2TXT
    {
        public async Task Convert(string selectedFilePath)
        {
            // Read Azure settings from appsettings.json
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            string endpoint = config["AzureDocumentIntelligence:Endpoint"];
            string apiKey = config["AzureDocumentIntelligence:ApiKey"];

            var client = new DocumentIntelligenceClient(
                new Uri(endpoint),
                new AzureKeyCredential(apiKey));

            // Read the file and hand the bytes to Azure
            byte[] fileBytes = File.ReadAllBytes(selectedFilePath);
            BinaryData data = BinaryData.FromBytes(fileBytes);

            // "prebuilt-read" is the OCR / plain-text model — handles Arabic + RTL
            Operation<AnalyzeResult> operation = await client.AnalyzeDocumentAsync(
                WaitUntil.Completed,
                "prebuilt-read",
                data);

            AnalyzeResult result = operation.Value;
            string allText = result.Content;   // full text, logical reading order

            // Save alongside the original, as <originalname>.txt
            string outputPath = Path.ChangeExtension(selectedFilePath, ".txt");
            File.WriteAllText(outputPath, allText, new UTF8Encoding(true));

            MessageBox.Show($"Done. Saved to:\n{outputPath}", "Extraction complete");
        }
    }
}