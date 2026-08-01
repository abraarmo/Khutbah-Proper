using Khutbah.Web.Services.DTO;        // where SentencePair lives
using Khutbah.Web.Services.Classes;            // where Translation + PDF2TXT live
using Microsoft.AspNetCore.Mvc.RazorPages;
using Khutbah.Web.Services;

namespace Khutbah.Web.Pages
{
    public class KhutbahModel : PageModel
    {
        public List<string> Sentences { get; set; } = new();
        public List<SentencePair> Pairs { get; set; } = new();

        public async Task OnGet()
        {
            string testPdf = @"C:\Users\My Login\Downloads\Jumuah-Khutbah.pdf";  // <-- your real test PDF path

            var pdf2txt = new PDF2TXT();
            string arabic = await pdf2txt.Convert(testPdf);

            List<SentencePair> pairs = await Translation.TranslateTextRequest(arabic);
            Sentences = pairs.Select(p => p.EN).ToList();
            Pairs = pairs;
            var test = Translation.SplitIntoSentences(arabic);
            foreach (var s in test)
                System.Diagnostics.Debug.WriteLine($"[{s}]");
        }
    }
}