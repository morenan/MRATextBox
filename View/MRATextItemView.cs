using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

using Morenan.MRATextBox.Core;
using Morenan.MRATextBox.Core.Documents;
using Morenan.MRATextBox.Counselor;

namespace Morenan.MRATextBox.View
{
    using WinTextLine = System.Windows.Media.TextFormatting.TextLine;

    internal class MRATextItemView : Panel
    {
        public MRATextItemView()
        {
            this.textfmtr = TextFormatter.Create(TextFormattingMode.Ideal);
            this.textbuilder = new StringBuilder();
            this.fontindics = new List<ITextFontIndex>();
            this.structbarmouseover = false;
            this.rawtextmouseover = false;

            this.dvNumberBar = new MRANumberBarDrawingVisual(this);
            this.dvStructBar = new MRAStructBarDrawingVisual(this);
            this.dvRawText = new MRARawTextDrawingVisual(this);
            this.dvSelection = new MRASelectionDrawingVisual(this);
            this.dvs = new List<MRATextItemDrawingVisual>() { dvNumberBar, dvStructBar, dvSelection, dvRawText };

            AddLogicalChild(dvNumberBar);
            AddLogicalChild(dvStructBar);
            AddLogicalChild(dvSelection);
            AddLogicalChild(dvRawText);
            AddVisualChild(dvNumberBar);
            AddVisualChild(dvStructBar);
            AddVisualChild(dvSelection);
            AddVisualChild(dvRawText);

            HorizontalAlignment = HorizontalAlignment.Left;
            VerticalAlignment = VerticalAlignment.Top;
        }

        #region Override Visual

        protected override Visual GetVisualChild(int index)
        {
            return index >= 0 && index < dvs.Count() ? dvs[index] : null;
        }

        protected override int VisualChildrenCount
        {
            get { return dvs.Count(); }
        }
        
        #endregion

        #region Number

        public IMRATextItemInfo Core { get { return (DataContext is IMRATextItemInfo) ? (IMRATextItemInfo)(DataContext) : null; } }
        public ITextBoxCore TextCore { get { return Core?.Core; } }

        #region Text System

        private TextFormatter textfmtr;
        private MRATextSource textsource;
        private WinTextLine textline;
        public WinTextLine TextLine { get { return this.textline; } }
        private StringBuilder textbuilder;
        private List<ITextFontIndex> fontindics;
        //private ITextPosition poscache;
        private int skipzonestart;
        public int SkipZoneStart { get { return this.skipzonestart; } }
        private int skipzonecount;
        public int SkipZoneCount { get { return this.skipzonecount; } }
        private ITextZone skipzone;
        public ITextZone SkipZone { get { return this.skipzone; } }
        private ITextZone openzone;
        public ITextZone OpenZone { get { return this.openzone; } }
        private ITextZone closezone;
        public ITextZone CloseZone { get { return this.closezone; } }
        private ITextZone intozone;
        public ITextZone IntoZone { get { return this.intozone; } }

        #endregion

        #region MouseOver

        private bool numberbarmouseover;
        public bool NumberBarMouseOver
        {
            get { return this.numberbarmouseover; }
            set { this.numberbarmouseover = value; }
        }
        private bool structbarmouseover;
        public bool StructBarMouseOver
        {
            get { return this.structbarmouseover; }
            set { if (structbarmouseover == value) return; this.structbarmouseover = value; dvStructBar.InvalidateVisual(); }
        }
        private bool rawtextmouseover;
        public bool RawTextMouseOver
        {
            get { return this.rawtextmouseover; }
            set { this.rawtextmouseover = value; }
        }
        
        #endregion

        #region DrawingVisual

        private MRARawTextDrawingVisual dvRawText;
        private MRANumberBarDrawingVisual dvNumberBar;
        private MRAStructBarDrawingVisual dvStructBar;
        private MRASelectionDrawingVisual dvSelection;
        private List<MRATextItemDrawingVisual> dvs;
        
        #endregion
        
        #endregion

        #region Method

        protected void InvalidateText()
        {
            if (Core == null || Core.IsDisposed) return;
            ITextZone zonebackground = null;
            ITextZone zoneforeground = null;
            ITextZone zonefontweight = null;
            ITextZone zonefontfamily = null;
            ITextZone zonefontstyle = null;
            ITextZone zonefontstretch = null;
            ITextZone zonefontsize = null;
            ITextItem item = Core.Start.Item;
            textbuilder.Clear();
            fontindics.Clear();
            this.skipzone = null;
            this.skipzonestart = -1;
            this.openzone = null;
            this.closezone = null;
            this.intozone = null;
            void _FindAllZone(ITextItem startitem)
            {
                for (ITextZone zone = startitem.Parent; zone != null; zone = zone.Parent)
                {
                    if (!(zone.Doc is IDocsFill)) continue;
                    IDocsFill doc = (IDocsFill)(zone.Doc);
                    if (doc.Fill == null) continue;
                    if (zonebackground == null && doc.Fill.Background != null) zonebackground = zone;
                    if (zoneforeground == null && doc.Fill.Foreground != null) zoneforeground = zone;
                    if (zonefontweight == null && doc.Fill.FontWeight != null) zonefontweight = zone;
                    if (zonefontfamily == null && doc.Fill.FontFamily != null) zonefontfamily = zone;
                    if (zonefontstyle == null && doc.Fill.FontStyle != null) zonefontstyle = zone;
                    if (zonefontstretch == null && doc.Fill.FontStretch != null) zonefontstretch = zone;
                    if (zonefontsize == null && doc.Fill.FontSize != null) zonefontsize = zone;
                    if (zonebackground == null) continue;
                    if (zoneforeground == null) continue;
                    if (zonefontweight == null) continue;
                    if (zonefontfamily == null) continue;
                    if (zonefontstyle == null) continue;
                    if (zonefontsize == null) continue;
                    if (zonefontstretch == null) continue;
                    break;
                }
            }
            void _RefindAllZone(ITextItem startitem)
            {
                zonebackground = null;
                zoneforeground = null;
                zonefontweight = null;
                zonefontfamily = null;
                zonefontstyle = null;
                zonefontsize = null;
                zonefontstretch = null;
                _FindAllZone(startitem);
            }
            void _AppendText(ITextItem additem, string addtext)
            {
                FontFamily fontfamily = TextCore.FontFamily;
                FontStyle fontstyle = TextCore.FontStyle;
                FontStretch fontstretch = TextCore.FontStretch;
                FontWeight fontweight = TextCore.FontWeight;
                double fontsize = TextCore.FontSize;
                Brush background = null;
                Brush foreground = TextCore.Foreground;
                if (additem.Doc is IDocsFill)
                {
                    IDocsFill doc = (IDocsFill)(additem.Doc);
                    if (doc.Fill != null)
                    {
                        if (doc.Fill.FontFamily != null) fontfamily = doc.Fill.FontFamily;
                        if (doc.Fill.FontStyle != null) fontstyle = doc.Fill.FontStyle.Value;
                        if (doc.Fill.FontWeight != null) fontweight = doc.Fill.FontWeight.Value;
                        if (doc.Fill.FontStretch != null) fontstretch = doc.Fill.FontStretch.Value;
                        if (doc.Fill.FontSize != null) fontsize = doc.Fill.FontSize.Value;
                        if (doc.Fill.Background != null) background = new SolidColorBrush(doc.Fill.Background.Value);
                        if (doc.Fill.Foreground != null) foreground = new SolidColorBrush(doc.Fill.Foreground.Value);
                    }
                }
                if (zonefontfamily?.Doc is IDocsFill) fontfamily = ((IDocsFill)(zonefontfamily.Doc)).Fill.FontFamily;
                if (zonefontstyle?.Doc is IDocsFill) fontstyle = ((IDocsFill)(zonefontstyle.Doc)).Fill.FontStyle.Value;
                if (zonefontweight?.Doc is IDocsFill) fontweight = ((IDocsFill)(zonefontweight.Doc)).Fill.FontWeight.Value;
                if (zonefontstretch?.Doc is IDocsFill) fontstretch = ((IDocsFill)(zonefontstretch.Doc)).Fill.FontStretch.Value;
                if (zonefontsize?.Doc is IDocsFill) fontsize = ((IDocsFill)(zonefontsize.Doc)).Fill.FontSize.Value;
                if (zonebackground?.Doc is IDocsFill) background = new SolidColorBrush(((IDocsFill)(zonebackground.Doc)).Fill.Background.Value);
                if (zoneforeground?.Doc is IDocsFill) foreground = new SolidColorBrush(((IDocsFill)(zoneforeground.Doc)).Fill.Foreground.Value);
                fontindics.Add(new TextFontIndex(new TextFontCore(fontstyle, fontstretch, fontfamily, fontweight, fontsize, background, foreground), textbuilder.Length));
                textbuilder.Append(addtext);
            }
            void _AppendSkipZone(ITextZone skipzone)
            {
                FontFamily fontfamily = TextCore.FontFamily;
                FontStyle fontstyle = TextCore.FontStyle;
                FontStretch fontstretch = TextCore.FontStretch;
                FontWeight fontweight = TextCore.FontWeight;
                double fontsize = TextCore.FontSize;
                Brush background = null;
                Brush foreground = Brushes.Gray;
                TextCore.DictBrush.TryGetValue("foreground_skipzone", out foreground);
                skipzonestart = textbuilder.Length;
                skipzonecount = 3;
                fontindics.Add(new TextFontIndex(new TextFontCore(fontstyle, fontstretch, fontfamily, fontweight, fontsize, background, foreground), textbuilder.Length));
                int showstart = 0;
                int showend = 0;
                if (TextCore.View.Counselor != null)
                {
                    string zonename = skipzone?.Doc?.Name ?? "{zone_deault}";
                    MRATextInputContext ictx = new MRATextInputContext(skipzone, 0, 0);
                    MRAZoneContext zctx = new MRAZoneContext(zonename, ictx, MRAZoneAction.Skip);
                    IMRAZoneResult zret = TextCore.View.Counselor.ZoneAction(zctx);
                    if (zret is IMRAZoneSkipResult)
                    {
                        IMRAZoneSkipResult zsret = (IMRAZoneSkipResult)zret;
                        showstart = zsret.ShowStart;
                        showend = zsret.ShowEnd;
                    }
                }
                for (int i = 0; i < Math.Min(showstart, skipzone.Items.Count); i++)
                    textbuilder.Append(skipzone.Items[i].ToString());
                textbuilder.Append("...");
                for (int i = Math.Max(0, skipzone.Items.Count - showend); i < skipzone.Items.Count; i++)
                    textbuilder.Append(skipzone.Items[i].ToString());
            }
            void _RemoveZone(ITextItem zone)
            {
                if (item == zonebackground) zonebackground = null;
                if (item == zoneforeground) zoneforeground = null;
                if (item == zonefontweight) zonefontweight = null;
                if (item == zonefontfamily) zonefontfamily = null;
                if (item == zonefontstyle) zonefontstyle = null;
                if (item == zonefontstretch) zonefontstretch = null;
                if (item == zonefontsize) zonefontstretch = null;
            }
            void _AddZone(ITextZone zone)
            {
                if (zone.Doc is IDocsFill)
                {
                    IDocsFill doc = (IDocsFill)(zone.Doc);
                    if (doc.Fill != null)
                    {
                        if (doc.Fill.Background != null) zonebackground = zone;
                        if (doc.Fill.Foreground != null) zoneforeground = zone;
                        if (doc.Fill.FontWeight != null) zonefontweight = zone;
                        if (doc.Fill.FontFamily != null) zonefontfamily = zone;
                        if (doc.Fill.FontStyle != null) zonefontstyle = zone;
                        if (doc.Fill.FontStretch != null) zonefontstretch = zone;
                        if (doc.Fill.FontSize != null) zonefontsize = zone;
                    }
                }
            }
            _FindAllZone(item);
            if (Core.Start != null && Core.End != null)
            {
                ITextItem startitem = Core.Start.Item;
                ITextItem enditem = Core.End.Item;
                startitem = startitem.Parent;
                enditem = enditem.Parent;
                while (startitem.Level > enditem.Level) startitem = startitem.Parent;
                while (startitem.Level < enditem.Level) enditem = enditem.Parent;
                while (startitem != enditem) { startitem = startitem.Parent; enditem = enditem.Parent; }
                while (startitem?.Parent != null)
                {
                    if (!(startitem is ITextLine) && ((ITextZone)startitem).LineCount > 1)
                    {
                        this.intozone = (ITextZone)startitem;
                        break;
                    }
                    startitem = startitem.Parent;
                }
            }
            {
                ITextItem startitem = Core.Start.Item;
                ITextItem enditem = Core.End.Item;
                if (startitem.ID == 0 
                 && !(startitem.Parent is ITextLine) && !(startitem.Parent is ITextBoxCore) && startitem.Parent.LineCount > 1)
                    this.openzone = startitem.Parent;
                if (enditem.ID == enditem.Parent.Items.Count - 1
                 && !(enditem.Parent is ITextLine) && !(enditem.Parent is ITextBoxCore) && enditem.Parent.LineCount > 1)
                    this.closezone = enditem.Parent;
            }
            if (Core.Start.Item == Core.End.Item)
            {
                string text = Core.Start.RightPart.Substring(0, Core.End.ItemIndex - Core.Start.ItemIndex);
                _AppendText(Core.Start.Item, text);
            }
            else if (Core is IMRAZoneSkipInfo)
            {
                IMRAZoneSkipInfo zscore = (IMRAZoneSkipInfo)Core;
                this.skipzone = zscore.SkipZone;
                if (Core.Start.Item.Parent != zscore.SkipZone)
                {
                    _AppendText(Core.Start.Item, Core.Start.RightPart);
                    while (item != null && item != Core.End.Item)
                    {
                        while (item?.Parent != null && item.ID + 1 >= item.Parent.Items.Count)
                        {
                            item = item.Parent;
                            ITextZone zone = (ITextZone)item;
                            if (!(zone is ITextLine) && zone.LineCount > 1) this.closezone = zone;
                            _RemoveZone(item);
                        }
                        if (item?.Parent == null) break;
                        item = item.Parent.Items[item.ID + 1];
                        _FindAllZone(item);
                        while (item is ITextZone)
                        {
                            ITextZone zone = (ITextZone)item;
                            if (zone == zscore.SkipZone) break;
                            _AddZone(zone);
                            item = zone.Items.FirstOrDefault();
                        }
                        if (item == zscore.SkipZone) break;
                        _AppendText(item, item.ToString());
                    }
                }
                _AppendSkipZone(zscore.SkipZone);
                if (Core.End.Item.Parent != zscore.SkipZone)
                {
                    item = zscore.SkipZone;
                    _RefindAllZone(item);
                    while (item != null && item != Core.End.Item)
                    {
                        while (item?.Parent != null && item.ID + 1 >= item.Parent.Items.Count)
                        {
                            item = item.Parent;
                            _RemoveZone(item);
                        }
                        if (item?.Parent == null) break;
                        item = item.Parent.Items[item.ID + 1];
                        _FindAllZone(item);
                        while (item is ITextZone)
                        {
                            ITextZone zone = (ITextZone)item;
                            if (!(zone is ITextLine) && zone.LineCount > 1) this.openzone = zone;
                            _AddZone(zone);
                            item = zone.Items.FirstOrDefault();
                        }
                        _AppendText(item, item.ToString());
                    }
                    _AppendText(Core.End.Item, Core.End.LeftPart);
                }
            }
            else
            {
                _AppendText(Core.Start.Item, Core.Start.RightPart);
                while (item != null && item != Core.End.Item)
                {
                    while (item?.Parent != null && item.ID + 1 >= item.Parent.Items.Count)
                    {
                        item = item.Parent;
                        ITextZone zone = (ITextZone)item;
                        if (!(zone is ITextLine) && zone.LineCount > 1)
                        {
                            this.closezone = zone;
                            if (intozone == null || zone.Level < intozone.Level)
                                this.intozone = zone;
                        }
                        _RemoveZone(item);
                    }
                    if (item?.Parent == null) break;
                    item = item.Parent.Items[item.ID + 1];
                    _FindAllZone(item);
                    while (item is ITextZone)
                    {
                        ITextZone zone = (ITextZone)item;
                        if (!(zone is ITextLine) && zone.LineCount > 1)
                        {
                            this.openzone = zone;
                            if (intozone == null || zone.Level < intozone.Level)
                                this.intozone = zone;
                        }
                        _AddZone(zone);
                        item = zone.Items.FirstOrDefault();
                    }
                    if (item == Core.End.Item) break;
                    _AppendText(item, item.ToString());
                }
                _AppendText(Core.End.Item, Core.End.LeftPart);
            }
            
            this.textsource = new MRATextSource(textbuilder.ToString(), fontindics);
            this.textline = textfmtr.FormatLine(textsource, 0, 10000.0, new MRATextParagraphProperties(TextCore, textsource), null);
        }

        public new void InvalidateVisual()
        {
            InvalidateText();
            dvNumberBar.InvalidateVisual();
            dvStructBar.InvalidateVisual();
            dvRawText.InvalidateVisual();
            dvSelection.InvalidateVisual();
            base.InvalidateVisual();
        }

        public void InvalidateSelection()
        {
            dvSelection.ForceShow = true;
            dvSelection.InvalidateVisual();
        }

        public void InvalidateStructBar()
        {
            dvStructBar.InvalidateVisual();
        }

        public void UpdateMouseOver(Point p)
        {
            NumberBarMouseOver = p.X >= TextCore.View.MarginNumberBar && p.X < TextCore.View.MarginStructBar;
            StructBarMouseOver = p.X >= TextCore.View.MarginStructBar && p.X < TextCore.View.MarginLeft;
            RawTextMouseOver = p.X >= TextCore.View.MarginLeft;
        }

        public void ReleaseMouseOver()
        {
            NumberBarMouseOver = false;
            StructBarMouseOver = false;
            RawTextMouseOver = false;
        }

        public ITextPosition GetTextPosition(Point p)
        {
            int l = 0, r = textline.Length;
            TextBounds bound = null;
            double ml = TextCore.View.MarginLeft;
            if (p.X < ml) return null;
            while (r - l > 1)
            {
                int mid = (l + r) >> 1;
                bound = textline.GetTextBounds(l, mid - l).FirstOrDefault();
                if (p.X - ml >= bound.Rectangle.Right) l = mid; else r = mid;
            }
            if (l == r) return GetTextPosition(l + 1);
            l = Math.Min(l, r);
            bound = textline.GetTextBounds(l, 1).FirstOrDefault();
            return (p.X - ml >= bound.Rectangle.Right) ? GetTextPosition(l + 2) : GetTextPosition(l + 1);
        }

        public ITextPosition GetTextPosition(int column)
        {
            if (column > textline.Length) column = textline.Length;
            if (Core is IMRAZoneSkipInfo)
            {
                IMRAZoneSkipInfo zscore = (IMRAZoneSkipInfo)Core;
                if (column < skipzonestart)
                    return Core.Start.Move(column - Core.Start.Column);
                else if (column > skipzonestart + skipzonecount)
                    return Core.End.Move(column - textline.Length);
                else
                    return null;
            }
            else
            {
                if (column - Core.Start.Column < Core.End.Column - column)
                    return Core.Start.Move(column - Core.Start.Column);
                else
                    return Core.End.Move(column - Core.End.Column);
            }
        }

        public Rect GetColumnActualRect(int column)
        {
            TextBounds bound = textline.GetTextBounds(column - 1, 1).FirstOrDefault();
            Rect rect = bound.Rectangle;
            rect.X += TextCore.View.MarginLeft;
            return rect;
        }


        #endregion

        #region Event Handler

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == DataContextProperty)
            {
                if (e.OldValue is IMRATextItemInfo)
                {
                    IMRATextItemInfo oldvalue = (IMRATextItemInfo)(e.OldValue);
                    oldvalue.View = null;
                }
                if (e.NewValue is IMRATextItemInfo)
                {
                    IMRATextItemInfo newvalue = (IMRATextItemInfo)(e.NewValue);
                    newvalue.View = this;
                }
                if (Core == null)
                {
                    ReleaseMouseOver();
                }
                else
                {
                    InvalidateVisual();
                    Width = Math.Max(TextCore.View.ActualWidth, textline.Width + TextCore.View.MarginLeft);
                    Height = textline.TextHeight;
                    Point p = Mouse.PrimaryDevice.GetPosition(this);
                    UpdateMouseOver(p);
                }
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            dvNumberBar.InvalidateVisual();
            dvStructBar.InvalidateVisual();
            dvRawText.InvalidateVisual();
            dvSelection.InvalidateVisual();
        }

        #endregion
    }
}
