using Khutbah_Frontend.Classes;
using Khutbah_Frontend.DTO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;

namespace Khutbah_Frontend
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public async void btn_browse_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileexplorer = new OpenFileDialog
            {
                InitialDirectory = @"D:\Downloads",
                Title = "Browse PDF Files",
                CheckFileExists = true,
                CheckPathExists = true,
                DefaultExt = "pdf",
                Filter = "PDF files (*.pdf)|*.pdf|All files (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true
            };

            if (fileexplorer.ShowDialog() != DialogResult.OK)
                return;

            string selectedFilePath = fileexplorer.FileName;

            try
            {
                PDF2TXT pdf2txt = new PDF2TXT();

                string arabic = await pdf2txt.Convert(selectedFilePath);
                string[] lines = arabic.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                var chunks = lines.Chunk(5);
                List<SentencePair> pairs;

                
                List<Task<List<SentencePair>>> tasks = new List<Task<List<SentencePair>>>();
                // marker found — translate both halves in parallel
                foreach (string[] group in chunks)
                {
                    string chunkText = string.Join("\n", group);
                    tasks.Add(Translation.TranslateTextRequest(chunkText));
                }
                await Task.WhenAll(tasks);

                pairs = new List<SentencePair>();
                foreach (var task in tasks)
                {
                    pairs.AddRange(task.Result);
                }
                

                new TranslationUI(pairs).Show();

                //string dir = Path.GetDirectoryName(selectedFilePath)!;
                //string name = Path.GetFileNameWithoutExtension(selectedFilePath);
                //string englishPath = Path.Combine(dir, name + "_Ten.txt");

                //File.WriteAllText(englishPath, english, new UTF8Encoding(true));

                //MessageBox.Show($"Done. Saved to:\n{englishPath}", "Translation complete");

                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Something went wrong");
            }
        }
    }
}
