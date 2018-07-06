using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Morenan.MRATextBox.Core;

namespace Morenan.MRATextBox.View
{
    internal class MRAStructBarDrawingVisual : MRATextItemDrawingVisual
    {
        #region Resource

        private const double RectSize = 10.0;
        private const double InsideMargin = 2.0;
        private const double DefaultWidth = 16.0;

        #endregion

        public MRAStructBarDrawingVisual(MRATextItemView _parent) : base(_parent)
        {

        }

        #region Method

        protected override void Render(DrawingContext ctx)
        {
            if (Core == null || Core.IsDisposed) return;
            base.Render(ctx);
            Brush fbsNormal = null;
            Brush fbsInside = null;
            Brush bbsInside = null;
            Brush bbsInsideMouseOver = null;
            Brush fbsInsideMouseOver = null;
            Brush fbsMouseOver = null;
            Brush bbsMouseOver = null;
            Pen penNormal = new Pen(ViewParent.StructBarMouseOver ? fbsMouseOver : fbsNormal, 1.0);
            Pen penInside = new Pen(ViewParent.StructBarMouseOver ? fbsMouseOver : fbsInside, 1.0);
            ITextBoxCore textcore = ViewParent.TextCore;
            Rect rect = new Rect(textcore.View.MarginStructBar, 0, textcore.View.MarginLeft - textcore.View.MarginStructBar, ViewParent.ActualHeight);
            textcore.DictBrush.TryGetValue("foreground_structbar_normal", out fbsNormal);
            textcore.DictBrush.TryGetValue("foreground_structbar_inside", out fbsInside);
            textcore.DictBrush.TryGetValue("background_structbar_inside_mouseover", out bbsInsideMouseOver);
            textcore.DictBrush.TryGetValue("foreground_structbar_inside_mouseover", out fbsInsideMouseOver);
            textcore.DictBrush.TryGetValue("background_structbar_inside", out bbsInside);
            textcore.DictBrush.TryGetValue("foreground_structbar_normal_mouseover", out fbsMouseOver);
            textcore.DictBrush.TryGetValue("background_structbar_normal_mouseover", out bbsMouseOver);
            
            if (ViewParent.StructBarMouseOver)
            {
                fbsInside = fbsInsideMouseOver;
                bbsInside = bbsInsideMouseOver;
            }
            if (ViewParent.StructBarMouseOver && bbsMouseOver != null)
            {
                ctx.DrawRectangle(bbsMouseOver, null, rect);
            }
            if (ViewParent.IntoZone != null)
            {
                ctx.DrawLine(penNormal,
                    new Point(rect.X + rect.Width / 2, rect.Top),
                    new Point(rect.X + rect.Width / 2, rect.Bottom));
            }
            if (ViewParent.CloseZone != null)
            {
                if (ViewParent.IntoZone == null)
                    ctx.DrawLine(penNormal,
                        new Point(rect.X + rect.Width / 2, rect.Top),
                        new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2));
                ctx.DrawLine(penNormal,
                    new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2),
                    new Point(rect.Right, rect.Y + rect.Height / 2));
            }
            if (ViewParent.SkipZone != null || ViewParent.OpenZone != null)
            {
                Rect rectInside = new Rect((rect.Width - RectSize) / 2, (rect.Height - RectSize) / 2, RectSize, RectSize);
                ctx.DrawRectangle(bbsInside, penNormal, rectInside);
                if (ViewParent.SkipZone != null)
                {
                    ctx.DrawLine(penInside,
                        new Point(rectInside.Left + InsideMargin, rectInside.Y + rectInside.Height / 2),
                        new Point(rectInside.Right - InsideMargin, rectInside.Y + rectInside.Height / 2));
                    ctx.DrawLine(penInside,
                        new Point(rectInside.X + rectInside.Width / 2, rectInside.Top + InsideMargin),
                        new Point(rectInside.X + rectInside.Width / 2, rectInside.Bottom - InsideMargin));
                }
                if (ViewParent.OpenZone != null)
                {
                    ctx.DrawLine(penInside,
                        new Point(rectInside.Left + InsideMargin, rectInside.Y + rectInside.Height / 2),
                        new Point(rectInside.Right - InsideMargin, rectInside.Y + rectInside.Height / 2));
                }
            }
        }

        #endregion

    }
}
