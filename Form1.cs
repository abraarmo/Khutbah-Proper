using System.Security.Cryptography.X509Certificates;

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
                FilterIndex = 2,
                RestoreDirectory = true,
                    ReadOnlyChecked = true,
                    ShowReadOnly = true
            };

            if (fileexplorer.ShowDialog() == DialogResult.OK)
            {
                string selectedFilePath = fileexplorer.FileName;
                PDF2TXT pdf2txt = new PDF2TXT();
                await pdf2txt.Convert(selectedFilePath);
            }
        }
    }
}
