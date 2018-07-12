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
            ITextBoxCore textcore = ViewParent.TextCore;
            Rect rect = new Rect(textcore.View.MarginStructBar, 0, textcore.View.MarginLeft - textcore.View.MarginStructBar, ViewParent.ActualHeight);
            textcore.DictBrush.TryGetValue("foreground_structbar_normal", out fbsNormal);
            textcore.DictBrush.TryGetValue("foreground_structbar_inside", out fbsInside);
            textcore.DictBrush.TryGetValue("background_structbar_inside_mouseover", out bbsInsideMouseOver);
            textcore.DictBrush.TryGetValue("foreground_structbar_inside_mouseover", out fbsInsideMouseOver);
            textcore.DictBrush.TryGetValue("background_structbar_inside", out bbsInside);
            textcore.DictBrush.TryGetValue("foreground_structbar_normal_mouseover", out fbsMouseOver);
            textcore.DictBrush.TryGetValue("background_structbar_normal_mouseover", out bbsMouseOver);
            Pen penNormal = new Pen(fbsNormal, 1.0);
            Pen penNormalFocus = new Pen(fbsMouseOver, 1.2);
            Pen penInside = new Pen(fbsInside, 1.0);
            Pen penInsideFocus = new Pen(fbsInsideMouseOver, 1.0);

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
                double top = rect.Top;
                double bottom = rect.Bottom;
                if (ViewParent.IntoZone == ViewParent.OpenZone)
                    top += (rect.Height - RectSize) / 2;
                if (ViewParent.IntoZone == ViewParent.CloseZone)
                    bottom -= rect.Height / 2;
                ctx.DrawLine(Core.IntoZoneFocus ? penNormalFocus : penNormal,
                    new Point(rect.X + rect.Width / 2, top),
                    new Point(rect.X + rect.Width / 2, bottom));
                if (Core.OpenZoneFocus)
                    ctx.DrawLine(penNormalFocus,
                        new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2),
                        new Point(rect.X + rect.Width / 2, bottom));
                if (Core.CloseZoneFocus)
                    ctx.DrawLine(penNormalFocus,
                        new Point(rect.X + rect.Width / 2, top),
                        new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2));
            }
            if (ViewParent.CloseZone != null)
            {
                Pen pen = Core.CloseZoneFocus ? penNormalFocus : penNormal;
                if (ViewParent.IntoZone == null)
                    ctx.DrawLine(pen,
                        new Point(rect.X + rect.Width / 2, rect.Top),
                        new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2));
                ctx.DrawLine(pen,
                    new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2),
                    new Point(rect.X + rect.Width / 2 + RectSize / 2, rect.Y + rect.Height / 2));
            }
            if (ViewParent.SkipZone != null || ViewParent.OpenZone != null)
            {
                Pen pen = penNormal;
                Brush fill = bbsInside;
                Rect rectInside = new Rect((rect.Width - RectSize) / 2 + textcore.View.MarginStructBar, (rect.Height - RectSize) / 2, RectSize, RectSize);
                if (ViewParent.OpenZone != null && Core.OpenZoneFocus) { pen = penNormalFocus; fill = bbsInsideMouseOver; }
                if (ViewParent.SkipZone != null && Core is IMRAZoneSkipInfo && ((IMRAZoneSkipInfo)Core).SkipZoneFocus) { pen = penNormalFocus; fill = bbsInsideMouseOver; }
                ctx.DrawRectangle(fill, pen, rectInside);
                pen = penInside;
                if (ViewParent.OpenZone != null && Core.OpenZoneFocus) pen = penInsideFocus;
                if (ViewParent.SkipZone != null)
                {
                    ctx.DrawLine(pen,
                        new Point(rectInside.Left + InsideMargin, rectInside.Y + rectInside.Height / 2),
                        new Point(rectInside.Right - InsideMargin, rectInside.Y + rectInside.Height / 2));
                    ctx.DrawLine(pen,
                        new Point(rectInside.X + rectInside.Width / 2, rectInside.Top + InsideMargin),
                        new Point(rectInside.X + rectInside.Width / 2, rectInside.Bottom - InsideMargin));
                }
                if (ViewParent.OpenZone != null)
                {
                    ctx.DrawLine(pen,
                        new Point(rectInside.Left + InsideMargin, rectInside.Y + rectInside.Height / 2),
                        new Point(rectInside.Right - InsideMargin, rectInside.Y + rectInside.Height / 2));
                }
            }
        }

        #endregion

    }
}
