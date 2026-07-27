using Khutbah_Frontend.DTO;
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
        static public async Task<List<SentencePair>> TranslateTextRequest(string inputText)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();
            System.Diagnostics.Debug.WriteLine($"Config build {sw.ElapsedMilliseconds} ms");

            string endpoint = config["AzureTranslationService:Endpoint"];
            string apiKey = config["AzureTranslationService:ApiKey"];

            string route = config["AzureTranslationService:Route"];

            string systemPrompt =
                """
                    You are an expert translator of classical and Qur'anic Arabic into English.

                    The input is a Friday khutbah (sermon), extracted by OCR, with full diacritics.

                    Split the Arabic into natural sentences and translate each one. Return ONLY a JSON array, one object per sentence:

                    [
                      { "ar": "<the Arabic sentence>", "en": "<its English translation>" }
                    ]

                    Rules:
                    - Every Arabic sentence must have exactly one matching English translation.
                    - Preserve the Arabic exactly as given, including diacritics. Do not correct or normalise it.
                    - Preserve Islamic terminology (taqwa, shirk, dua) rather than flattening it.
                    - Render Qur'anic verses and hadith in a dignified register appropriate to scripture. If not possible, dont translate and keep the original.
                    - Ignore page numbers, headers, and footers. Do not include them.
                    - Output no commentary, no markdown, no code fences — only the raw JSON array.
                """;

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
                client.Timeout = TimeSpan.FromMinutes(5);
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(new Uri(endpoint), route);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                request.Headers.Add("api-key", apiKey);

                HttpResponseMessage response = await client.SendAsync(request);
                string result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    throw new Exception($"OpenAI returned {(int)response.StatusCode}: {result}");
                System.Diagnostics.Debug.WriteLine($"OpenAI has just responded: {sw.ElapsedMilliseconds} ms");

                var parsed = JsonConvert.DeserializeObject<Khutbah_Frontend.DTO.OpenAiResponse>(result)!;
                string translatedText = parsed.Choices[0].Message.Content;
                List<SentencePair> sentencePairs = JsonConvert.DeserializeObject<List<SentencePair>>(translatedText)!;

                System.Diagnostics.Debug.WriteLine($"OpenAI has just deserialized: {sw.ElapsedMilliseconds} ms");
                return sentencePairs;
            }
        }
    }
}