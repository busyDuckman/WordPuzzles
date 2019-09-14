/*  ---------------------------------------------------------------------------------------------------------------------------------------
 *  (C) 2019, Dr Warren Creemers.
 *  This file is subject to the terms and conditions defined in the included file 'LICENSE.txt'
 *  ---------------------------------------------------------------------------------------------------------------------------------------
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WD_toolbox;
using WD_toolbox.Maths.Space;
using WD_toolbox.Rendering;

namespace WordPuzzles.WordPuzzles
{
    public class WordGrid : IWordGrid
    {
        public enum OutlineStyleEnum { NoOutline=0, Square, Beveled}
        public struct GridRenderSetting
        {
            public bool letterVisible;
            public Color letterColor;
            public Color guideColor;
            public Color fillColor;
            public Color outlineColor;

            public FontStyle FontStyle;
            public string guideText;

            public OutlineStyleEnum outlineStyle;
            

            public void init()
            {
                letterVisible = true;
                letterColor = Color.Red;
                guideColor = Color.Gray;
                fillColor = Color.LightGray;
                outlineColor = Color.Blue;
                FontStyle = FontStyle.Regular;
                guideText = null;
                outlineStyle = OutlineStyleEnum.Square;
            }

            public static GridRenderSetting Default()
            {
                GridRenderSetting gs = new GridRenderSetting();
                gs.init();
                return gs;
            }
        }
        //----------------------------------------------------------------------------------
        // Instance data
        //----------------------------------------------------------------------------------
        public Size Size { get; protected set; }
        char?[,] data;

        public int Width { get { return Size.Width; } }
        public int Height { get { return Size.Height; } }

        //[NonSerialized]
        public Func<int, int, char?, GridRenderSetting> customiseCell;

        //----------------------------------------------------------------------------------
        // Constructors and factory methods
        //----------------------------------------------------------------------------------
        protected WordGrid(int width, int height)
        {
            Size = new Size(width, height);
            data = new char?[width, height];
            customiseCell = null;

            ClearGrid();
        }


        public static WordGrid FromSize(int width, int height)
        {
            if ((width > 0) && (height > 0))
            {
                return new WordGrid(width, height);
            }

            return null;
        }

        //----------------------------------------------------------------------------------
        // IWordGrid Interface
        //----------------------------------------------------------------------------------
        public char? this[int x, int y]
        {
            get { return data[x, y]; }
            set { data[x, y] = value; }
        }

        public char? this[Point p]
        {
            get { return this[p.X, p.Y]; }
            set { this[p.X, p.Y] = value; }
        }

        public void ClearGrid()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    data[x, y] = null;
                }
            }
        }

        public bool InBounds(Point p) { return InBounds(p.X, p.Y); }

        public bool InBounds(int x, int y)
        {
            return ((x >= 0) && (y >= 0) && (x < Width) && (y < Height));
        }

        //----------------------------------------------------------------------------------
        // rendering
        //----------------------------------------------------------------------------------
        public void Render(IRenderer r, Rectangle bounds)
        {
            using (Font guideFont = new Font("Consolas", 8))
            {
                using (Font font = new Font("Consolas", 16))
                {
                    r.FillRectangle(Color.White, bounds);

                    double xStep = (bounds.Width / (double)Width);
                    double yStep = (bounds.Height / (double)Height);

                    int boxWidth = (int)Math.Ceiling(xStep);
                    int boxHeight = (int)Math.Ceiling(yStep);

                    GridRenderSetting[,] gridSettings = new GridRenderSetting[Width, Height];

                    for (int component = 0; component < 3; component++)
                    {
                        //populate the edges
                        for (int x = 0; x < Width; x++)
                        {
                            for (int y = 0; y < Height; y++)
                            {
                                //first time, get setting
                                if (component == 0)
                                {
                                    gridSettings[x, y] = (customiseCell != null) ? customiseCell(x, y, this[x, y]) : GridRenderSetting.Default(); 
                                }

                                renderBox(r, ref bounds, xStep, yStep, x, y, boxWidth, boxHeight, component, font, guideFont, gridSettings[x, y]);
                            }
                        }
                    }
                }
            }
        }

        private void renderBox(IRenderer r, ref Rectangle bounds, 
            double xStep, double yStep, int x, int y,
            int boxWidth, int boxHeight, int component,
            Font font, Font guideFont,
            GridRenderSetting gridSetting)
        {
            //location
            int x1 = (int)Math.Ceiling(bounds.Left + xStep * x);
            int x2 = (int)Math.Ceiling(bounds.Left + xStep * (x + 1));
            int y1 = (int)Math.Ceiling(bounds.Top + yStep * y);
            int y2 = (int)Math.Ceiling(bounds.Top + yStep * (y + 1));

            switch (component)
            {
                case 0: //background
                    r.FillRectangle(gridSetting.fillColor, x1, y1, boxWidth, boxHeight);
                    break;
                
                case 1: //foreground
                    switch (gridSetting.outlineStyle)
	                {
                        case OutlineStyleEnum.NoOutline:
                            break;
                        case OutlineStyleEnum.Square:
                            r.DrawRectangle(gridSetting.outlineColor, 1, x1, y1, boxWidth, boxHeight);
                            break;
                        case OutlineStyleEnum.Beveled:
                            break;
	                }

                    if ((this[x, y] != null) && gridSetting.letterVisible)
                    {
                        using (Font renderFont = new Font(font, gridSetting.FontStyle))
                        {
                            r.DrawString(gridSetting.letterColor, this[x, y].Value.ToString(), renderFont, x1 + 3, y1);
                        }
                    }
                    break;

                case 2: //guide
                    string text = gridSetting.guideText;
                    if(text != null)
                    {
                        r.DrawString(Color.Gray, text, guideFont, x1 + 1, y1);
                    }
                    break;
            }
        }

        /*
        internal void RenderGuideText(IRenderer r, Rectangle bounds, Func<Point, string> getText)
        {
            using (Font font = new Font("Consolas", 8))
            {
                double xStep = (bounds.Width / (double)Width);
                double yStep = (bounds.Height / (double)Height);
                for (int x = 0; x < Width; x++)
                {
                    for (int y = 0; y < Height; y++)
                    {
                        string text = getText(new Point(x, y));
                        if(text != null)
                        {
                            int x1 = (int)Math.Ceiling(bounds.Left + xStep * x);
                            int y1 = (int)Math.Ceiling(bounds.Top + yStep * y);
                            r.DrawString(Color.Gray, text, font, x1 + 1, y1);
                        }
                    }
                }
            }
        }
        */
    }

    
}
