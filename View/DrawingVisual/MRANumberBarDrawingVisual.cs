using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Morenan.MRATextBox.Core;

namespace Morenan.MRATextBox.View
{
    internal class MRANumberBarDrawingVisual : MRATextItemDrawingVisual
    {
        public MRANumberBarDrawingVisual(MRATextItemView _parent) : base(_parent)
        {

        }

        #region Method

        protected override void Render(DrawingContext ctx)
        {
            if (Core == null || Core.IsDisposed) return;
            base.Render(ctx);
            ITextBoxCore textcore = ViewParent.TextCore;
            double fontsize = textcore.FontSize;
            Brush foreground = null;
            textcore.DictBrush.TryGetValue("foreground_linenumber", out foreground);

            Typeface typeface = new Typeface(textcore.FontFamily, textcore.FontStyle, textcore.FontWeight, textcore.FontStretch);
            FormattedText fmttext = new FormattedText(Core.Line.ToString(), Thread.CurrentThread.CurrentUICulture, FlowDirection.LeftToRight, typeface, fontsize, foreground);
            ctx.DrawText(fmttext, new Point(textcore.View.MarginNumberBar, 0));
        }

        #endregion
    }
}
