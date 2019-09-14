using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WordPuzzles
{
    public partial class FrmNewCrossWord : Form
    {
        private CrossWordOptions value = new CrossWordOptions(20, 20);

        public CrossWordOptions Value
        {
            get { return this.value; }
            set { this.value = value; }
        }

        public FrmNewCrossWord()
        {
            InitializeComponent();
        }

        private void FrmNewCrossWord_Load(object sender, EventArgs e)
        {
            numWidth.DataBindings.Add("Value", value, "Width");
            numHeight.DataBindings.Add("Value", value, "Height");
        }
    }
}
