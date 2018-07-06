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
                        int left = start.Column - 1;
                        int right = end.Column - 1;
                        if (start.Line < item.Line) left = 0;
                        if (end.Line > item.Line) right = text.Length;
                        if (right - left <= 0) break;
                        TextBounds bound = text.GetTextBounds(left, right - left).FirstOrDefault();
                        Brush brush = null;
                        Rect rect = bound.Rectangle;
                        ViewParent.TextCore.DictBrush.TryGetValue("background_rawtext_selected", out brush);
                        rect.X += ViewParent.TextCore.View.MarginLeft;
                        ctx.DrawRectangle(brush, null, rect);
                    }
                    break;
            }
        }

        #endregion
    }
}
