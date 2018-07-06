using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Morenan.MRATextBox.Core;
using Morenan.MRATextBox.Counselor;

namespace Morenan.MRATextBox.View.Completation
{
    internal class MRACltBox : ItemsControl
    {
        public MRACltBox()
        {
            this.core = null;
            this.items = new List<MRACltItemInfo>();
            this.cltsrcs = new List<IMRACltItem>();
            this.keywords = new List<ITextKeyCore>();
            this.selectedindex = -1;
            ItemsSource = items;
        }

        #region Number

        private ITextBoxCore core;
        public ITextBoxCore Core
        {
            get { return this.core; }
            set { this.core = value; Initialize(); }
        }

        private MRACompletationAccuracy accuracy;
        public MRACompletationAccuracy Accuracy
        {
            get { return this.accuracy; }
            set { this.accuracy = value; }
        }

        private MRACltVirtualizeStackPanel itempanel;
        public MRACltVirtualizeStackPanel ItemPanel
        {
            get { return this.itempanel; }
            set { this.itempanel = value; }
        }

        private int selectedindex;
        public int SelectedIndex { get { return this.selectedindex; } }

        private List<MRACltItemInfo> items;
        private List<IMRACltItem> cltsrcs;
        private List<ITextKeyCore> keywords;
        
        #endregion

        #region Method
        
        protected void Initialize()
        {
            if (core != null)
            {
                keywords.Clear();
                keywords.AddRange(core.DictKey.Values);
            }
        }

        protected bool Match(string inputtext, string itemtext, IList<MRACltBoldSegment> bolds, ref double similiar)
        {
            if (inputtext.Length == 0)
            {
                similiar = 0.0;
                return true;
            }
            if ((accuracy & MRACompletationAccuracy.IgnoreCase) != MRACompletationAccuracy.None)
            {
                itemtext = itemtext.ToUpper();
                inputtext = inputtext.ToUpper();
            }
            if ((accuracy & MRACompletationAccuracy.StartWith) != MRACompletationAccuracy.None)
            {
                if (itemtext.StartsWith(inputtext))
                {
                    bolds.Add(new MRACltBoldSegment() { Start = 0, Count = inputtext.Length });
                    return true;
                }
                similiar = 10.0;
            }
            if ((accuracy & MRACompletationAccuracy.EndWith) != MRACompletationAccuracy.None)
            {
                if (itemtext.EndsWith(inputtext))
                {
                    bolds.Add(new MRACltBoldSegment() { Start = itemtext.Length - inputtext.Length, Count = inputtext.Length });
                    return true;
                }
                similiar = 6.0;
            }
            if ((accuracy & MRACompletationAccuracy.SubString) != MRACompletationAccuracy.None)
            {
                int index = itemtext.IndexOf(inputtext);
                if (index >= 0)
                {
                    bolds.Add(new MRACltBoldSegment() { Start = index, Count = inputtext.Length });
                    return true;
                }
                similiar = 4.0;
            }
            if ((accuracy & MRACompletationAccuracy.Discrete) != MRACompletationAccuracy.None)
            {
                int start = 0, i = 0, j = 0;
                bool last = false;
                for (; j < inputtext.Length && i < itemtext.Length; i++)
                {
                    if (itemtext[i] == inputtext[j])
                    {
                        if (!last) start = i;
                        last = true;
                    }
                    else
                    {
                        if (last) bolds.Add(new MRACltBoldSegment() { Start = start, Count = i - start });
                        last = false;
                    }
                }
                if (j >= inputtext.Length)
                {
                    if (last && i > start) bolds.Add(new MRACltBoldSegment() { Start = start, Count = i - start });
                    similiar = 5.0 - bolds.Count();
                    return true;
                }
            }
            return false;
        }

        public void SetCltSources(IEnumerable<IMRACltItem> _cltsrcs)
        {
            cltsrcs.Clear();
            cltsrcs.AddRange(_cltsrcs);
        }

        public void SetInputText(string inputtext)
        {
            List<MRACltItemInfo> _items = new List<MRACltItemInfo>();
            List<MRACltBoldSegment> bolds = new List<MRACltBoldSegment>();
            double maxsimi = 0.0;
            int maxsimiid = -1;
            foreach (ITextKeyCore keyword in keywords)
            {
                bolds.Clear();
                double _simi = 0.0;
                if (!Match(inputtext, keyword.Keyword, bolds, ref _simi)) continue;
                if (maxsimiid < 0 || _simi > maxsimi) { maxsimi = _simi; maxsimiid = items.Count(); }
                MRACltItemInfo item = new MRACltItemInfo(core, this, MRACltItemTypes.Keyword, items.Count(), keyword.Keyword);
                item.Bolds.AddRange(bolds);
                _items.Add(item);
            }
            foreach (IMRACltItem cltitem in cltsrcs)
            {
                bolds.Clear();
                double _simi = 0.0;
                if (!Match(inputtext, cltitem.Text, bolds, ref _simi)) continue;
                if (maxsimiid < 0 || _simi > maxsimi) { maxsimi = _simi; maxsimiid = items.Count(); }
                MRACltItemTypes itemtype = MRACltItemTypes.Variable;
                if (cltitem is IMRACltType) itemtype = MRACltItemTypes.Class;
                if (cltitem is IMRACltVar) itemtype = MRACltItemTypes.Variable;
                if (cltitem is IMRACltFunc) itemtype = MRACltItemTypes.Function;
                MRACltItemInfo item = new MRACltItemInfo(core, this, itemtype, items.Count(), cltitem.Text);
                item.Bolds.AddRange(bolds);
                _items.Add(item);
            }
            if (_items.Count() > 0)
            {
                _items.Sort((i1, i2) => i1.Text.CompareTo(i2.Text));
                items = _items;
                ItemsSource = items;
                this.selectedindex = maxsimiid;
                if (selectedindex >= 0 && selectedindex < items.Count())
                {
                    items[selectedindex].IsSelected = true;
                    ScrollIntoMiddle(selectedindex);
                }
            }
        }

        public void ScrollIntoView(int index)
        {
            if (itempanel != null)
            {
                if ((int)itempanel.VerticalOffset > index)
                    itempanel.SetVerticalOffset(index);
                else if ((int)(itempanel.VerticalOffset + itempanel.ViewportHeight) <= index)
                    itempanel.SetVerticalOffset(index - itempanel.ViewportHeight + 1);
            }
        }

        public void ScrollIntoMiddle(int index)
        {
            if (itempanel != null)
            {
                double vo = index - itempanel.VerticalOffset / 2;
                vo = Math.Max(vo, 0);
                itempanel.SetVerticalOffset(vo);
            }
        }

        public void SelectUp()
        {
            SelectSet(selectedindex <= 0 ? items.Count() - 1 : selectedindex - 1);
            ScrollIntoView(selectedindex);
        }

        public void SelectDown()
        {
            SelectSet(selectedindex + 1 >= items.Count() ? 0 : selectedindex + 1);
            ScrollIntoView(selectedindex);
        }

        public void SelectSet(int index)
        {
            if (selectedindex >= 0 && selectedindex < items.Count())
                items[selectedindex].IsSelected = false;
            selectedindex = index;
            if (selectedindex >= 0 && selectedindex < items.Count())
                items[selectedindex].IsSelected = true;
        }

        #endregion
        
    }

    public enum MRACompletationAccuracy
    {
        None = 0x00000000,
        StartWith = 0x00000001,
        EndWith = 0x00000002,
        SubString = 0x00000004,
        Discrete = 0x00000008,
        IgnoreCase = 0x00000010
    }
}
