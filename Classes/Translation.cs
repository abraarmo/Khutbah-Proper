using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Khutbah_Frontend.Classes
{
    public class Translation
    {
        static public async Task<string> TranslateTextRequest(string inputText, string selectedFilePath)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            string endpoint = config["AzureTranslationService:Endpoint"];
            string apiKey = config["AzureTranslationService:ApiKey"];

            
            string route = config["AzureTranslationService:Route"];

            string systemPrompt =
                "You are an expert translator of classical and Qur'anic Arabic into English. " +
                "The input is a Friday khutbah (sermon) with full diacritics. Translate it into clear, " +
                "faithful English. Preserve Islamic terminology, and render Qur'anic verses and hadith " +
                "in a dignified register appropriate to scripture. Keep the structure of the original. " +
                "Output only the English translation, with no commentary.";

            var body = new
            {
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user",   content = inputText }
                }
            };
            var requestBody = JsonConvert.SerializeObject(body);

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(new Uri(endpoint), route);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                request.Headers.Add("api-key", apiKey);

                HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
                string result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    throw new Exception($"OpenAI returned {(int)response.StatusCode}: {result}");

                var parsed = JsonConvert.DeserializeObject<Khutbah_Frontend.DTO.OpenAiResponse>(result);
                string translatedText = parsed.Choices[0].Message.Content;

                string dir = Path.GetDirectoryName(selectedFilePath);
                string name = Path.GetFileNameWithoutExtension(selectedFilePath);
                string englishPath = Path.Combine(dir, name + "_en.txt");

                File.WriteAllText(englishPath, translatedText, new UTF8Encoding(true));

                return translatedText;
            }
        }
    }
}