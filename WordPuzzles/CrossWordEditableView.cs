/*  ---------------------------------------------------------------------------------------------------------------------------------------
 *  (C) 2019, Dr Warren Creemers.
 *  This file is subject to the terms and conditions defined in the included file 'LICENSE.txt'
 *  ---------------------------------------------------------------------------------------------------------------------------------------
 */
using WD_toolbox.Maths.Geometry;
using WD_toolbox.Maths.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WD_toolbox.Maths.Geometry;
using WD_toolbox;
using WDLibApplicationFramework.ViewAndEdit2D;
using WD_toolbox.Rendering;

namespace WordPuzzles.WordPuzzles
{
    public class CrossWordEditableView : EditableView2DBase<CrossWordPuzzle>
    {
        private CrossWordPuzzle puzzle;

        public override CrossWordPuzzle What { get { return puzzle; }}
        public int blockSizeInPixels { get; set; }

        int oldX = -1, oldY = -1;

        public CrossWordEditableView(CrossWordPuzzle puzzle)
        {
            this.puzzle = puzzle;
            blockSizeInPixels = 32;
        }

        public CrossWordEditableView(CrossWordPuzzle puzzle, int blockRenderSize) : this(puzzle)
        {
            blockSizeInPixels = blockRenderSize;
        }


        public override Rectangle2D WorldSpaceBounds
        {
            get
            {
                return new Rectangle2D(0, 0, puzzle.Width * blockSizeInPixels, puzzle.Height * blockSizeInPixels);
            }
            set
            {
                
            }
        }

        public override void Render(WD_toolbox.Rendering.IRenderer r)
        {
            if (renderMatrixProxy == null)
            {
                renderMatrixProxy = this.CalculateWorldToScreenTransformMatrix();
            }
            r.SetTransform(renderMatrixProxy);
      
            
            puzzle.Render(r, (Rectangle)WorldSpaceBounds);
            if ((oldX >= 0) && (oldY >= 0))
            {
                //r.DrawRectangle(Color.Blue, 1, oldX * blockSizeInPixels, oldY * blockSizeInPixels, blockSizeInPixels, blockSizeInPixels);
            }

            r.ResetTransform();
        }

        public override void RenderGizmoLayer(IRenderer r)
        {
            if ((oldX >= 0) && (oldY >= 0))
            {
                //r.DrawRectangle(Color.Green, 1, oldX * blockSizeInPixels, oldY * blockSizeInPixels, blockSizeInPixels, blockSizeInPixels);
                Rectangle2D rec = new WD_toolbox.Maths.Geometry.Rectangle2D(oldX * blockSizeInPixels, oldY * blockSizeInPixels, blockSizeInPixels, blockSizeInPixels);
                List<Point2D> points = new List<Point2D>() { rec.TopLeft, rec.TopRight, rec.LowerLeft, rec.LowerRight };
                points = renderMatrixProxy.Transform(points);

                r.DrawLine(Color.Red, 1, points[0], points[1]);
                r.DrawLine(Color.Red, 1, points[0], points[2]);
                r.DrawLine(Color.Red, 1, points[2], points[3]);
                r.DrawLine(Color.Red, 1, points[1], points[3]);
            }
        }

        public override void DoMouseMove(object sender, MouseButtons button, Point2D worldScpacePos)
        {
            base.DoMouseMove(sender, button, worldScpacePos);

            int x = (int) (worldScpacePos.X / blockSizeInPixels);
            int y = (int) (worldScpacePos.Y / blockSizeInPixels);

            if ((x != oldX) || (y != oldY))
            {
                oldX = x;
                oldY = y;
                OnRefreshNeeded();
            }
        }
    }
}
