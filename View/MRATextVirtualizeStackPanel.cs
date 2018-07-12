using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Morenan.MRATextBox.View
{
    internal class MRATextVirtualizingStackPanel : Panel
    {
        public MRATextVirtualizingStackPanel()
        {
            this.sb_v = new ScrollBar() { Orientation = Orientation.Vertical, Cursor = Cursors.Arrow };
            this.sb_h = new ScrollBar() { Orientation = Orientation.Horizontal, Cursor = Cursors.Arrow };
            this.lits = new List<IMRATextItemInfo>();
            this.vits = new List<MRATextItemView>();
            this.vstart = 0;
            this.vcount = 0;
            this.vheight = 0.0;
            this.vwrect = new Rect();
            this.exrect = new Rect();
            sb_v.ValueChanged += OnVerticalScrollChanged;
            sb_h.ValueChanged += OnHorizontalScrollChanged;
            Panel.SetZIndex(sb_v, 1000);
            Panel.SetZIndex(sb_h, 1000);
            Children.Add(sb_v);
            Children.Add(sb_h);
            //AddInternalChild(sb_v);
            //AddInternalChild(sb_h);
            VerticalAlignment = VerticalAlignment.Stretch;
            HorizontalAlignment = HorizontalAlignment.Stretch;
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }
        
        #region Number

        private MRATextControl parent;
        public MRATextControl ViewParent { get { return this.parent; } }

        private ScrollBar sb_v;
        private ScrollBar sb_h;
        private bool sb_v_changing = false;
        private bool sb_h_changing = false;
        private List<IMRATextItemInfo> lits;
        private List<MRATextItemView> vits;
        private int vstart;
        private int vcount;
        private double vheight;
        private Rect vwrect;
        private Rect exrect;
        
        public bool CanHorizontalScroll { get { return exrect.Width > ActualWidth; } }
        public bool CanVerticalScroll { get { return vstart > 0 || vstart + vcount < lits.Count(); } }
        public double ViewportPixelWidth { get { return ActualWidth - sb_v.ActualWidth; } }
        public double ViewportPixelHeight { get { return ActualHeight - sb_h.ActualHeight; } }

        public double HorizontalOffset { get { return vwrect.X; } }
        public double VerticalOffset { get { return vwrect.Y; } }
        public double ViewportWidth { get { return vwrect.Width; } }
        public double ViewportHeight { get { return vwrect.Height; } }
        public double ExtentWidth { get { return exrect.Width; } }
        public double ExtentHeight { get { return exrect.Height; } }

        #endregion

        #region Layout Override

        protected override Size MeasureOverride(Size availableSize)
        {
            sb_v.Measure(availableSize);
            sb_h.Measure(availableSize);
            for (int i = 0; i < vcount; i++)
                vits[i].Measure(availableSize);
            return availableSize;
        }
        
        protected override Size ArrangeOverride(Size finalSize)
        {
            sb_v.Arrange(new Rect(finalSize.Width - sb_v.DesiredSize.Width, 0, sb_v.DesiredSize.Width, finalSize.Height - sb_h.DesiredSize.Width));
            sb_h.Arrange(new Rect(0, finalSize.Height - sb_h.DesiredSize.Height, finalSize.Width - sb_v.DesiredSize.Width, sb_h.DesiredSize.Height));
            double height = 0.0;
            for (int i = 0; i < vcount; i++)
            {
                vits[i].Arrange(new Rect(-vwrect.X, height, vits[i].Width, vits[i].Height));
                height += vits[i].Height;
            }
            for (int i = vcount; i < vits.Count(); i++)
                vits[i].Arrange(new Rect(0, 0, 0, 0));
            return finalSize;
        }

        #endregion
        
        #region Scroll

        #region ViewStart

        protected static readonly DependencyProperty ViewStartProperty = DependencyProperty.Register(
            "ViewStart", typeof(int), typeof(MRATextVirtualizingStackPanel), new PropertyMetadata(0, OnPropertyChanged_ViewStart));
        
        public int ViewStart
        {
            get { return (int)GetValue(ViewStartProperty); }
            set { SetValue(ViewStartProperty, value); }
        }

        protected enum ViewStartChangedReasons { Up, Down, User, Items };
        
        private static void OnPropertyChanged_ViewStart(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }

        #endregion

        #region ViewCount

        protected static readonly DependencyProperty ViewCountProperty = DependencyProperty.Register(
            "ViewCount", typeof(int), typeof(MRATextVirtualizingStackPanel), new PropertyMetadata(0, OnPropertyChanged_ViewCount));

        public int ViewCount
        {
            get { return (int)GetValue(ViewCountProperty); }
            protected set { SetValue(ViewCountProperty, value); }
        }

        private static void OnPropertyChanged_ViewCount(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            
        }

        #endregion

        #region Change

        public void SetHorizontalOffset(double _offset)
        {
            sb_h.Value = _offset / exrect.Width;
        }

        public void SetVerticalOffset(double _offset)
        {
            int _vstart = (int)_offset;
            if (_vstart > vstart)
            {
                ShowBack(_vstart - vstart);
                ShowUser();
            }
            else if (_vstart < vstart)
            {
                ShowFount(vstart - _vstart);
                ShowUser();
            }
        }

        public void LineUp()
        {
            ShowFount(1);
            ShowUser();
        }

        public void LineDown()
        {
            ShowBack(1);
            ShowUser();
        }

        public void LineLeft()
        {
            double _value = sb_h.Value;
            _value -= 0.05;
            _value = Math.Max(_value, 0.0);
            sb_h.Value = _value;
        }

        public void LineRight()
        {
            double _value = sb_h.Value;
            _value += 0.05;
            _value = Math.Min(_value, 1.0);
            sb_h.Value = _value;
        }

        public void PageUp()
        {
            ShowFount(vcount);
            ShowUser();
        }

        public void PageDown()
        {
            ShowBack(vcount);
            ShowUser();
        }

        public void PageHome()
        {
            vstart = 0;
            ShowClear();
            ShowBack();
            ShowUser();
        }

        public void PageEnd()
        {
            vstart = lits.Count() - 1;
            ShowClear();
            ShowFount();
            ShowUser();
        }

        #endregion

        #endregion

        #region Items

        protected void ShowClear()
        {
            foreach (MRATextItemView view in vits)
                view.DataContext = null;
            vcount = 0;
            vheight = 0.0;
        }

        private bool showuser_executing = false;
        protected void ShowUser()
        {
            if (showuser_executing) return;
            showuser_executing = true;
            //vwrect.X = sb_v.Value * exrect.Width;
            vwrect.Y = vstart;
            vwrect.Width = ViewportPixelWidth;
            vwrect.Height = vcount;
            exrect.X = 0.0;
            exrect.Y = 0.0;
            //exrect.Width = 0.0;
            exrect.Height = lits.Count();   
            for (int i = 0; i < vcount; i++)
                if (vits[i].Width > exrect.Width)
                    exrect.Width = vits[i].Width;
            if (!sb_h_changing)
            {
                sb_h.Value = (exrect.Width - vwrect.Width > 0) ? vwrect.X / (exrect.Width - vwrect.Width) : 0.0;
                sb_h.ViewportSize = (exrect.Width - vwrect.Width > 0) ? vwrect.Width / (exrect.Width - vwrect.Width) : 0.0;
            }
            if (!sb_v_changing)
            {
                sb_v.Value = (exrect.Height - vwrect.Height > 0) ? vwrect.Y / (exrect.Height - vwrect.Height) : 0.0;
                sb_v.ViewportSize = (exrect.Height - vwrect.Height > 0) ? vwrect.Height / (exrect.Height - vwrect.Height) : 0.0;
            }
            ViewStart = vstart;
            ViewCount = vcount;
            InvalidateMeasure();
            InvalidateArrange();
            showuser_executing = false;
        }

        protected void ShowFount(int _start, int _count)
        {
            List<MRATextItemView> _vits = new List<MRATextItemView>();
            for (int i = 0; i < _count; i++)
            {
                if (vcount <= 0) break;
                MRATextItemView item = null;
                if (vcount < vits.Count())
                {
                    item = vits.LastOrDefault();
                    vits.RemoveAt(vits.Count() - 1);
                }
                else
                {
                    item = new MRATextItemView();
                    Children.Add(item);
                }
                item.DataContext = lits[_start + i];
                vheight += item.Height;
                _vits.Add(item);
                while (vcount > 0 && vheight - vits[vcount - 1].Height >= ViewportPixelHeight)
                {
                    vheight -= vits[vcount - 1].Height;
                    vits[vcount-- - 1].DataContext = null;
                }
            }
            vstart = _start;
            vcount += _vits.Count();
            vits.InsertRange(0, _vits);
        }

        protected void ShowBack(int _start, int _count)
        {
            int oldvcount = vcount;
            int rcount = 0;
            int reused = 0;
            for (int i = _count - 1; i >= 0; i--)
            {
                if (rcount >= vcount) break;
                MRATextItemView item = null;
                if (vcount >= vits.Count())
                {
                    if (reused < rcount)
                        item = vits[reused++];
                    else
                    {
                        item = new MRATextItemView();
                        Children.Add(item);
                    }
                    vits.Add(item);
                }
                else
                    item = vits[vcount];
                item.DataContext = lits[_start + i];
                vheight += item.Height;
                vcount++;
                while (rcount < vcount && vheight - vits[rcount].Height >= ViewportPixelHeight)
                {
                    vheight -= vits[rcount].Height;
                    vits[rcount++].DataContext = null;
                }
            }
            for (int i = reused; i < rcount; i++)
                vits.Add(vits[i]);
            vits.Reverse(oldvcount, vcount - oldvcount);
            vits.RemoveRange(0, rcount);
            vcount -= rcount;
            vstart = _start + _count - vcount;
        }

        protected void ShowFount(int _count)
        {
            int _start = vstart - _count;
            _start = Math.Max(_start, 0);
            _count = vstart - _start;
            ShowFount(_start, _count);
        }

        protected void ShowBack(int _count)
        {
            int _start = vstart + vcount;
            _count = Math.Min(_count, lits.Count() - _start);
            ShowBack(_start, _count);
        }
        /*
        protected void HideFount(int _count)
        {
            List<MRATextItemView> _vits = vits.GetRange(0, _count);
            _vits.RemoveRange(0, _count);
            vstart += _count;
            vcount -= _count;
            vheight -= _vits.Sum(v => v.Height);
            foreach (MRATextItemView item in _vits)
                item.DataContext = null;
            vits.AddRange(_vits);
            ShowBack();
        }

        protected void HideBack(int _count)
        {
            for (int i = vcount - _count; i < vcount; i++)
            {
                vheight -= vits[i].Height;
                vits[i].DataContext = null;
            }
            vcount -= _count;
            ShowFount();
        }
        */
        protected void ShowFount()
        {
            List<MRATextItemView> _vits = new List<MRATextItemView>();
            while (vstart > 0 && vheight < ViewportPixelHeight)
            {
                MRATextItemView item = null;
                if (vits.Count() > vcount)
                {
                    item = vits.LastOrDefault();
                    vits.RemoveAt(vits.Count() - 1);
                }
                else
                {
                    item = new MRATextItemView();
                    Children.Add(item);
                }
                item.DataContext = lits[--vstart];
                vcount++;
                vheight += item.Height;
                _vits.Add(item);
            }
            vits.InsertRange(0, _vits);
        }

        protected void ShowBack()
        {
            while (vstart + vcount < lits.Count() && vheight < ViewportPixelHeight)
            {
                while (vits.Count() <= vcount)
                {
                    MRATextItemView item = new MRATextItemView();
                    Children.Add(item);
                    vits.Add(item);
                }
                {
                    MRATextItemView item = vits[vcount];
                    item.DataContext = lits[vstart + vcount++];
                    vheight += item.Height;
                }
            }
        }
        /*
        protected void HideFount()
        {
            int rcount = 0;
            while (rcount < vcount && vheight > ViewportPixelHeight)
            {
                MRATextItemView item = vits[rcount];
                if (vheight - item.Height < ActualHeight) break;
                vheight -= item.Height;
                item.DataContext = null;
                rcount++;
            }
            if (rcount > 0)
            {
                vstart += rcount;
                vcount -= rcount;
                List<MRATextItemView> _vits = vits.GetRange(0, rcount);
                vits.RemoveRange(0, rcount);
                vits.AddRange(_vits);
            }
        }

        protected void HideBack()
        {
            while (vcount > 0 && vheight > ViewportPixelHeight)
            {
                MRATextItemView item = vits[vcount - 1];
                if (vheight - item.Height < ActualHeight) break;
                vheight -= item.Height;
                vcount--;
                item.DataContext = null;
            }
        }
        */
        public void InvalidateLogicItems(List<IMRATextItemInfo> _lits)
        {
            this.lits = _lits;
            InvalidateVisualItems();
        }

        protected void InvalidateVisualItems()
        {
            vstart = Math.Max(vstart, 0);
            vstart = Math.Min(vstart, lits.Count() - 1);
            ShowClear();
            ShowBack();
            ShowFount();
            ShowUser();
        }

        #endregion

        #region Event Handler
        
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement)
            {
                FrameworkElement fele = (FrameworkElement)sender;
                while (fele != null)
                {
                    if (fele is MRATextControl)
                    {
                        this.parent = (MRATextControl)(fele);
                        parent.UI_Stack = this;
                        InvalidateLogicItems(parent.Items.Cast<IMRATextItemInfo>().ToList());
                    }
                    if (fele.Parent is FrameworkElement)
                        fele = (FrameworkElement)(fele.Parent);
                    else if (fele.TemplatedParent is FrameworkElement)
                        fele = (FrameworkElement)(fele.TemplatedParent);
                    else
                        fele = null;
                }
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            //parent.UI_Stack = null;
            //this.parent = null;
        }
        
        private void OnVerticalScrollChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            sb_v_changing = true;
            int _vstart = (int)(e.NewValue * (exrect.Height - vwrect.Height));
            _vstart = Math.Max(_vstart, 0);
            _vstart = Math.Min(_vstart, lits.Count() - 1);
            if (_vstart < vstart)
            {
                ShowFount(vstart - _vstart);
                ShowUser();
            }
            else if (_vstart > vstart)
            {
                ShowBack(_vstart - vstart);
                ShowUser();
            }
            sb_v_changing = false;
        }

        private void OnHorizontalScrollChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            sb_h_changing = true;
            vwrect.X = e.NewValue * (exrect.Width - vwrect.Width);
            InvalidateMeasure();
            InvalidateArrange();
            sb_h_changing = false;
        }

        #endregion
    }
}
