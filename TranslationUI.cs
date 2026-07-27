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
        public TranslationUI(List<SentencePair> sentencePairs)
        {
            InitializeComponent();
            string richTextContent = string.Join("\n", sentencePairs.Select(p => p.EN));
            richTextBox1.Text = richTextContent;
        }

    }
}
