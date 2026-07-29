using Khutbah_Frontend.DTO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Khutbah_Frontend
{
    public partial class TranslationUI : Form
    {
        public TranslationUI(List<SentencePair> translatedText)
        {
            InitializeComponent();
            richTextBox1.Text = string.Join("\n", translatedText.Select(s => s.EN));
        }

    }
}
