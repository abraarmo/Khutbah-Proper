using Khutbah.Web.Services.DTO;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Text;
using System.Text.RegularExpressions;

namespace Khutbah.Web.Services.Classes
{
    public class Translation
    {
        public static List<string> SplitintoSentences(string text)
        {
            // STAGE 1: remove header/footer lines (anything with Latin letters) BEFORE splitting
            var cleanedLines = text
                .Split('\n')
                .Where(line => !Regex.IsMatch(line, "[a-zA-Z]"))
                .ToList();
            string cleaned = string.Join(" ", cleanedLines);

            // STAGE 2: split the cleaned Arabic into sentences
            return cleaned
                .Split(new[] { '.', '!', '?', '،' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();
        }
        public static async Task<string> TranslateTextRequest(string inputText)
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
                    Translate the following sentence faithfully.
                    - Preserve Islamic terminology (taqwa, shirk, dua) rather than flattening it.
                    - Render Qur'anic verses and hadith in a dignified register appropriate to scripture.
                    Output only the English translation. No commentary, no JSON, no quotes.
                """;

            var body = new
            {
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = inputText }
                },
                reasoning_effort = "low"
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

                var parsed = JsonConvert.DeserializeObject<Khutbah.Web.Services.DTO.OpenAiResponse>(result)!;
                string translatedText = parsed.Choices[0].Message.Content;

                System.Diagnostics.Debug.WriteLine($"OpenAI has just deserialized: {sw.ElapsedMilliseconds} ms");
                return translatedText;
            }
        }

        // ORCHESTRATOR: split -> translate every sentence in parallel -> build pairs by index
        public static async Task<List<SentencePair>> TranslateAll(string rawArabic)
        {
            List<string> sentences = SplitintoSentences(rawArabic);

            var tasks = new List<Task<string>>();
            foreach (var sentence in sentences)
                tasks.Add(TranslateTextRequest(sentence));

            string[] english = await Task.WhenAll(tasks);

            var pairs = new List<SentencePair>();
            for (int i = 0; i < sentences.Count; i++)
            {
                pairs.Add(new SentencePair
                {
                    AR = sentences[i],
                    EN = english[i]
                });
            }

            return pairs;
        }
    }
}
