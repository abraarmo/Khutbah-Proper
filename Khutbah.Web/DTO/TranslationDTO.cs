using System.Collections.Generic;

namespace Khutbah.Web.Services.DTO
{
    public class OpenAiResponse
    {
        public List<Choice> Choices { get; set; }
    }

    public class Choice
    {
        public Message Message { get; set; }
    }

    public class Message
    {
        public string Content { get; set; }
    }

    public class SentencePair
    {
        public string AR{ get; set; }
        public string EN{ get; set; }
    }
}