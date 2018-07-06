using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using System.Windows.Controls;
using System.Windows;
using Morenan.MRATextBox.Core;
using Morenan.MRATextBox.Core.Documents;

namespace Morenan.MRATextBox.View
{
    internal class MRARawTextDrawingVisual : MRATextItemDrawingVisual
    {
        public MRARawTextDrawingVisual(MRATextItemView _parent) : base(_parent)
        {
        }

        #region Number
        
        #endregion

        #region Method
        
        protected override void Render(DrawingContext ctx)
        {
            if (Core == null || Core.IsDisposed) return;
            base.Render(ctx);
            ViewParent?.TextLine?.Draw(ctx, new Point(ViewParent.TextCore.View.MarginLeft, 0), InvertAxes.None);
            if (ViewParent.SkipZone != null && ViewParent.SkipZoneID >= 0)
            {
                TextBounds bound = ViewParent.TextLine.GetTextBounds(ViewParent.SkipZoneID, 3).FirstOrDefault();
                Brush foreground = Brushes.Gray;
                ViewParent.TextCore.DictBrush.TryGetValue("foreground_skipzone", out foreground);
                Pen pen = new Pen(foreground, 1.0);
                Rect rect = bound.Rectangle;
                rect.X += ViewParent.TextCore.View.MarginLeft;
                ctx.DrawRectangle(null, pen, rect);
            }
        }

        #endregion
    }
    
}
