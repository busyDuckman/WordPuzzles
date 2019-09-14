using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WD_toolbox.Data.DataStructures;
using WD_toolbox.Rendering;
using WDLang.Words;
using WDLibApplicationFramework.AplicationFramework.Menus;
using WDLibApplicationFramework.ViewAndEdit2D;
using Busyducks;
using WordPuzzles.WordPuzzles;
using WD_toolbox;
using System.Reflection;

namespace WordPuzzles
{
    public partial class FrmMain : Form
    {
        CrossWordPuzzle puzzle;
        LanguageDictionary dictionary;
        CrossWordEditableView cwView;
      
        public FrmMain()
        {
            InitializeComponent();
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            try
            {       
                dictionary = LanguageDictionary.loadInternalEnglish();
            }
            catch (Exception ex)
            {

            }

            if(dictionary == null)
            {
                MessageBox.Show("Fatal error, unable to load dictionary.");
                Application.Exit();
            }

            //MenuHelper.visitAllUnimplementedMenuItems(mnuMain.GetMenuItemWrapper(), M => M.Enabled = false);

            //disalbe all menu items that are not implemented
            string clickEndig = "_click";
            List<string> clickMethods = (from info in typeof(FrmMain).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public) 
                                         where info.Name.ToLower().EndsWith(clickEndig) 
                                         select info.Name.Substring(0, info.Name.Length-clickEndig.Length)).ToList();
            MenuHelper.visitAllMenuItems(mnuMain.GetMenuItemWrapper(), 
                                            M => M.Enabled = false,
                                            M => !clickMethods.Contains(M.Name) && (!M.HasSubMenus));
        }

        private void generateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (puzzle == null)
            {
                NewCrossWordFromDialoge();
            }
            if (puzzle == null)
            {
                return;
            }

            int width = puzzle.Width; //20
            int height = puzzle.Height;
            int blocksize = 32;

            puzzle.Clear();
            //puzzle.seeedRandomGenerator(123);
            this.UseWaitCursor = true;
            this.Refresh();
            puzzle.Generate(dictionary, -1); //125
            this.UseWaitCursor = false;

            //render
            IRenderer r = IRendererFactory.GetPreferredRenderer(width * blocksize, height * blocksize);
            //puzzle.Render(r, new Rectangle(0, 0, width * blocksize, height * blocksize));
            //vbMain.View = new RasterView2D(r.RenderTargetAsGDIBitmap());*/
            cwView = new CrossWordEditableView(puzzle, blocksize);
            vbMain.View = cwView;
            vbMain.Refresh();



            //chow clues
            rtClues.Text = puzzle.generateClues();
        }

        private void newCrossWordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NewCrossWordFromDialoge();
        }

        private void NewCrossWordFromDialoge()
        {
            FrmNewCrossWord frm = new FrmNewCrossWord();
            frm.Value = new CrossWordOptions(20, 20);
            if (puzzle != null)
            {
                frm.Value = new CrossWordOptions(puzzle.Width, puzzle.Height);
            }
            if (frm.ShowDialog(this) == DialogResult.OK)
            {
                puzzle = CrossWordPuzzle.FromSize(frm.Value.Width, frm.Value.Height);
            }
            pgMain.SelectedObject = puzzle;
        }

        private void findSpotForWordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (puzzle == null)
            {
                return;
            }
            if (dictionary == null)
            {
                return;
            }
            WordSelectDialoge wsd = new WordSelectDialoge(dictionary);
            if (wsd.ShowDialog() == DialogResult.OK)
            {
            }
        }

        private void validateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (puzzle != null)
            {
                Why valid = puzzle.Valid();
                MessageBox.Show(valid ? "Crossword is valid" : ("Crossword not valid\r\n"+valid.Reason));
            }

            if (dictionary != null)
            {
                
            }
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //puzzle = null;
            generateToolStripMenuItem_Click(sender, e);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Busyducks.AboutBoxBusyDucks about = new AboutBoxBusyDucks("", "Copyright (C) 2019, Dr Warren Creemers", ProductStatus.Apha);
            about.Acknowledge("Patrick J. Cassidy and Sergey Poznyakoff", "GNU Collaborative International Dictionary of English", "http://gcide.gnu.org.ua/");
            about.ShowDialog();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}
