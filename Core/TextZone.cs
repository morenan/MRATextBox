using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Morenan.MRATextBox.Core
{
    internal class TextZone : TextItem, ITextZone
    {
        public TextZone(TextBoxCore _core, ITextKey _left, ITextKey _right) : base(_core)
        {
            this.items = new List<ITextItem>();
            this.linecount = 1;
            this.skipcount = 0;
            this.isskip = false;
            if (_left != null) Add(_left);
            if (_right != null) Add(_right);
            Doc = _left?.Doc?.Parent ?? _right?.Doc?.Parent;
        }
        
        protected override void _Dispose()
        {
            this.items = null;
            base._Dispose();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (ITextItem item in items)
                sb.Append(item.ToString());
            return sb.ToString();
        }

        #region Number

        private List<ITextItem> items;
        public IList<ITextItem> Items { get { return this.items; } }

        private int linecount;
        public int LineCount { get { return this.linecount; } set { this.linecount = value; } }

        private int skipcount;
        public int SkipCount { get { return this.skipcount; } set { this.skipcount = value; } }

        private bool isskip;
        public bool IsSkip { get { return this.isskip; } set { skipcount -= isskip ? 1 : 0; this.isskip = value; skipcount += isskip ? 1 : 0; } }

        public override int Level
        {
            get { return base.Level; }
            set { base.Level = value; foreach (ITextItem item in items) item.Level = Level + 1; }
        }

        #endregion

        #region Method

        protected void AncestorRelease()
        {
            ITextZone _parent = Parent;
            while (_parent != null)
            {
                _parent.LineCount -= linecount - 1;
                _parent.SkipCount -= skipcount;
                _parent = _parent.Parent;
            }
        }

        protected void AncestorCapture()
        {
            ITextZone _parent = Parent;
            while (_parent != null)
            {
                _parent.LineCount += linecount - 1;
                _parent.SkipCount += skipcount;
                _parent = _parent.Parent;
            }
        }

        public void Add(ITextItem item)
        {
            AncestorRelease();
            item.Parent = this;
            item.ID = items.Count();
            item.Level = Level + 1;
            items.Add(item);
            if (item is ITextTrim)
            {
                ITextTrim trim = (ITextTrim)item;
                linecount += trim.GetEnterCount();
            }
            if (item is ITextZone)
            {
                ITextZone zone = (ITextZone)item;
                linecount += zone.LineCount - 1;
                skipcount += zone.SkipCount;
            }
            AncestorCapture();
        }

        public void Insert(int id, ITextItem item)
        {
            AncestorRelease();
            item.Parent = this;
            item.ID = id;
            item.Level = Level + 1;
            for (int i = id; i < items.Count(); i++)
                items[i].ID++;
            items.Insert(id, item);
            if (item is ITextTrim)
            {
                ITextTrim trim = (ITextTrim)item;
                linecount += trim.GetEnterCount();
            }
            if (item is ITextZone)
            {
                ITextZone zone = (ITextZone)item;
                linecount += zone.LineCount - 1;
                skipcount += zone.SkipCount;
            }
            AncestorCapture();
        }

        public void Remove(ITextItem item)
        {
            AncestorRelease();
            for (int i = item.ID + 1; i < items.Count(); i++)
                items[i].ID--;
            items.Remove(item);
            item.Parent = null;
            item.ID = -1;
            item.Level = -1;
            if (item is ITextTrim)
            {
                ITextTrim trim = (ITextTrim)item;
                linecount -= trim.GetEnterCount();
            }
            if (item is ITextZone)
            {
                ITextZone zone = (ITextZone)item;
                linecount -= zone.LineCount - 1;
                skipcount -= zone.SkipCount;
            }
            AncestorCapture();
        }
        
        public void Replace(int start, int count, IEnumerable<ITextItem> _additems)
        {
            AncestorRelease();
            items.RemoveRange(start, count);
            items.InsertRange(start, _additems);
            this.linecount = 1;
            this.skipcount = isskip ? 1 : 0;
            for (int i = 0; i < items.Count(); i++)
            {
                items[i].Parent = this;
                items[i].ID = i;
                items[i].Level = Level + 1;
                if (items[i] is ITextTrim)
                {
                    ITextTrim trim = (ITextTrim)items[i];
                    linecount += trim.GetEnterCount();
                }
                if (items[i] is ITextZone)
                {
                    ITextZone zone = (ITextZone)items[i];
                    linecount += zone.LineCount - 1;
                    skipcount += zone.SkipCount;
                }
            }
            AncestorCapture();
        }

        public IEnumerable<ITextItem> GetRange(int start, int count)
        {
            for (int i = start; i < start + count; i++)
                yield return items[i];
        }
        
        #endregion
    }
}
