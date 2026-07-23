using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Khutbah_Frontend.Classes
{
    public class Translation
    {
        public static async Task<string> Translator(string allText)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            string endpoint = config["AzureTranslationService:Endpoint"];
            string apiKey = config["AzureTranslationService:ApiKey"];
            string region = config["AzureTranslationService:Region"];

            string route = "translate?api-version=3.0&from=ar&to=en";

            object[] body = new object[] { new { Text = allText } };
            string requestBody = JsonConvert.SerializeObject(body);

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", apiKey);
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Region", region);
                client.BaseAddress = new Uri(endpoint);

                using (var request = new HttpRequestMessage())
                {
                    request.Method = HttpMethod.Post;
                    request.RequestUri = new Uri(client.BaseAddress, route);
                    request.Content = new StringContent(
                        requestBody, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.SendAsync(request);
                    string result = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                        throw new Exception(
                            $"Translator returned {(int)response.StatusCode}: {result}");

                    dynamic jsonResponse = JsonConvert.DeserializeObject(result);
                    return (string)jsonResponse[0].translations[0].text;
                }
            }
        }
    }
}