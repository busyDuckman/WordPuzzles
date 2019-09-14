using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WDLang.Words;

namespace WordPuzzles
{
    public partial class WordSelectDialoge : Form
    {
        public WordSelectDialoge(LanguageDictionary dic)
        {
            InitializeComponent();
            this.wordSelectionBox1.Dictionary = dic;
        }
    }
}
