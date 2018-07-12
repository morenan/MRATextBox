using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using System.Windows.Threading;
using Morenan.MRATextBox.Core;

namespace Morenan.MRATextBox.View
{
    using WinTextLine = System.Windows.Media.TextFormatting.TextLine;

    internal class MRASelectionDrawingVisual : MRATextItemDrawingVisual
    {
        #region Resource

        private static readonly Pen Pen_Default_Caret = new Pen(Brushes.Black, 1);
        private static readonly Brush Brush_Default_Range = Brushes.Violet;

        #endregion

        public MRASelectionDrawingVisual(MRATextItemView _parent) : base(_parent)
        {
            this.timerblink = new DispatcherTimer(new TimeSpan(0, 0, 0, 0, 500), DispatcherPriority.Normal, _HandleBlink, Dispatcher);
            this.status = SelectionStatus.None;
            this.blinkshow = true;
            this.forceshow = false;
        }

        #region Number

        private enum SelectionStatus { None = 0, Caret, Range }

        private SelectionStatus status;
        private DispatcherTimer timerblink;
        private bool blinkshow;
        private bool forceshow;
        public bool ForceShow
        {
            get { return this.forceshow; }
            set { this.forceshow = value; }
        }

        #endregion

        #region Method

        private void _HandleBlink(object sender, EventArgs e)
        {
            if (status == SelectionStatus.Caret)
            {
                forceshow = false;
                blinkshow = !blinkshow;
                if (status == SelectionStatus.Caret) InvalidateVisual();
            }
        }

        protected override void Render(DrawingContext ctx)
        {
            if (Core == null || Core.IsDisposed) return;
            base.Render(ctx);
            IMRATextItemInfo item = ViewParent?.Core;
            ITextBoxCore core = ViewParent?.TextCore;
            WinTextLine text = ViewParent?.TextLine;
            if (core == null) return;
            ITextPosition start = core.SelectedStart;
            ITextPosition end = core.SelectedEnd;
            if (start.CompareTo(end) == 0)
            {
                ITextPosition pos = start.NextSeek();
                RenderBracket(ctx, pos);
                RenderDefaultMatch(ctx, pos);
                pos = pos.PrevSeek();
                RenderBracket(ctx, pos);
                RenderDefaultMatch(ctx, pos);
            }
            if (start.Line == end.Line && start.Column == end.Column)
                status = start.Line == item.Line ? SelectionStatus.Caret : SelectionStatus.None;
            else if (item.Line >= start.Line && item.Line <= end.Line)
                status = SelectionStatus.Range;
            else
                status = SelectionStatus.None;
            switch (status)
            {
                case SelectionStatus.None:
                    break;
                case SelectionStatus.Caret:
                    {
                        if (!blinkshow && !forceshow) break;
                        int index = start.Column - 1;
                        TextBounds bound = text.GetTextBounds(index, 1).FirstOrDefault();
                        Brush brush = null;
                        double thickness = 1.0;
                        ViewParent.TextCore.DictBrush.TryGetValue("foreground_rawtext_caret", out brush);
                        ViewParent.TextCore.DictValue.TryGetValue("rawtext_caret_thickness", out thickness);
                        Pen pen = new Pen(brush, thickness);
                        Point p1 = bound.Rectangle.TopLeft;
                        Point p2 = bound.Rectangle.BottomLeft;
                        p1.Y -= 2;
                        p1.X += ViewParent.TextCore.View.MarginLeft;
                        p2.X += ViewParent.TextCore.View.MarginLeft;
                        ctx.DrawLine(pen, p1, p2);
                    }
                    break;
                case SelectionStatus.Range:
                    {
                        //int left = start.Column - 1;
                        //int right = end.Column - 1;
                        if (start.Line < item.Line) start = Core.Start;
                        if (end.Line > item.Line) end = Core.End;
                        //if (right - left <= 0) break;
                        //TextBounds bound = text.GetTextBounds(left, right - left).FirstOrDefault();
                        Brush brush = null;
                        //Rect rect = bound.Rectangle;
                        ViewParent.TextCore.DictBrush.TryGetValue("background_rawtext_selected", out brush);
                        //rect.X += ViewParent.TextCore.View.MarginLeft;
                        //ctx.DrawRectangle(brush, null, rect);
                        DrawTextRectangle(ctx, brush, null, start, end);
                    }
                    break;
            }
        }

        protected void RenderBracket(DrawingContext ctx, ITextPosition pos)
        {
            ITextItem item = pos.Item;
            ITextZone zone = item.Parent;
            if (!(item is ITextKey)) return;
            ITextKey key = (ITextKey)item;
            if ((key.KeyCore.Feature & (TextKeyFeatures.ZoneLeft | TextKeyFeatures.ZoneRight)) == TextKeyFeatures.None) return;
            Brush brush = null;
            ViewParent.TextCore.DictBrush.TryGetValue("background_rawtext_bracket", out brush);
            if (Core.ContainLine(pos.Line))
            {
                ITextPosition start = pos.LeftPart.Length > 0 ? pos.PrevItem() : pos;
                ITextPosition end = pos.RightPart.Length > 0 ? pos.NextItem() : pos;
                DrawTextRectangle(ctx, brush, null, start.Column - 1, end.Column - start.Column);
            }
            if (item.ID == 0)
            {
                ITextItem rightkey = zone.Items.LastOrDefault();
                if (Core.ContainLine(pos.Line + zone.LineCount - 1))
                {
                    ITextPosition rstart = new TextPosition() { Item = rightkey, ItemIndex = 0, Line = pos.Line + zone.LineCount - 1, Column = -1 };
                    ITextPosition rend = rstart?.NextItem();
                    if (rstart != null && rend != null)
                        DrawTextRectangle(ctx, brush, null, rstart, rend);
                }
            }
            else
            {
                ITextItem leftkey = zone.Items.FirstOrDefault();
                if (Core.ContainLine(pos.Line - zone.LineCount + 1))
                {
                    ITextPosition lstart = new TextPosition() { Item = leftkey, ItemIndex = 0, Line = pos.Line - zone.LineCount + 1, Column = -1 };
                    ITextPosition lend = lstart?.NextItem();
                    if (lstart != null && lend != null)
                        DrawTextRectangle(ctx, brush, null, lstart, lend);
                }
            }
        }

        protected void RenderDefaultMatch(DrawingContext ctx, ITextPosition pos)
        {
            Brush brush = null;
            if (!ViewParent.TextCore.DictBrush.TryGetValue("background_rawtext_defaultmatch", out brush)) return;
            ITextItem item = pos.Item;
            if (item is ITextKey) return;
            string word = item.ToString();
            ITextPosition start = Core.Start;
            ITextPosition end = Core.End;
            if (Core is IMRAZoneSkipInfo)
            {
                IMRAZoneSkipInfo zkcore = (IMRAZoneSkipInfo)Core;
                while (start != null && !zkcore.SkipZone.IsAncestorOf(start.Item))
                {
                    if (!(start.Item is ITextTrim) && !(start.Item is ITextKey) 
                     && start.Item.ToString().Equals(word))
                    {
                        ITextPosition wstart = start.LeftPart.Length > 0 ? start.PrevItem() : start;
                        ITextPosition wend = start.RightPart.Length > 0 ? start.NextItem() : start;
                        DrawTextRectangle(ctx, brush, null, wstart, wend);
                    }
                    start = start?.NextItem();
                }
                while (end != null && !zkcore.SkipZone.IsAncestorOf(end.Item))
                {
                    if (!(end.Item is ITextTrim) && !(end.Item is ITextKey)
                     && end.Item.ToString().Equals(word))
                    {
                        ITextPosition wstart = end.LeftPart.Length > 0 ? end.PrevItem() : end;
                        ITextPosition wend = end.RightPart.Length > 0 ? end.NextItem() : end;
                        DrawTextRectangle(ctx, brush, null, wstart, wend);
                    }
                    end = end?.PrevItem();
                }
            }
            else
            {
                while (start.Item != end.Item)
                {
                    if (!(start.Item is ITextTrim) && !(start.Item is ITextKey)
                     && start.Item.ToString().Equals(word))
                    {
                        ITextPosition wstart = start.LeftPart.Length > 0 ? start.PrevItem() : start;
                        ITextPosition wend = start.RightPart.Length > 0 ? start.NextItem() : start;
                        DrawTextRectangle(ctx, brush, null, wstart, wend);
                    }
                    start = start?.NextItem();
                }
            }
        }

        protected void DrawTextRectangle(DrawingContext ctx, Brush brush, Pen pen, ITextPosition start, ITextPosition end)
        {
            if (Core is IMRAZoneSkipInfo)
            {
                IMRAZoneSkipInfo zscore = (IMRAZoneSkipInfo)Core;
                if (start.Line == zscore.LineStart && end.Line == zscore.LineStart)
                {
                    DrawTextRectangle(ctx, brush, pen, start.Column - 1, end.Column - start.Column);
                }
                else if (start.Line == zscore.LineEnd && end.Line == zscore.LineEnd)
                {
                    WinTextLine text = ViewParent?.TextLine;
                    int startid = text.Length - Core.End.Column + start.Column;
                    int endid = text.Length - Core.End.Column + end.Column;
                    DrawTextRectangle(ctx, brush, pen, startid, endid - startid);
                }
                else if (start.Line == zscore.LineStart && end.Line == zscore.LineEnd)
                {
                    WinTextLine text = ViewParent?.TextLine;
                    int endid = text.Length - Core.End.Column + end.Column;
                    DrawTextRectangle(ctx, brush, pen, start.Column - 1, endid - start.Column + 1);
                }
            }
            else
            {
                DrawTextRectangle(ctx, brush, pen, start.Column - 1, end.Column - start.Column);
            }
        }

        protected void DrawTextRectangle(DrawingContext ctx, Brush brush, Pen pen, int start, int count)
        {
            if (count <= 0) return;
            WinTextLine text = ViewParent?.TextLine;
            TextBounds bound = text.GetTextBounds(start, count).FirstOrDefault();
            Rect rect = bound.Rectangle;
            rect.X += ViewParent.TextCore.View.MarginLeft;
            ctx.DrawRectangle(brush, null, rect);
        }

        #endregion
    }
}
