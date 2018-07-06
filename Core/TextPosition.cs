using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Morenan.MRATextBox.Core
{
    internal class TextPosition : ITextPosition
    {
        public TextPosition()
        {

        }

        public override string ToString()
        {
            return String.Format("({0},{1}){2}^{3}", line, column, LeftPart, RightPart);
        }

        #region Number

        private ITextItem item;
        public ITextItem Item { get { return this.item; } set { this.item = value; } }

        private int itemindex;
        public int ItemIndex { get { return this.itemindex; } set { this.itemindex = value; } }

        private int index;
        public int Index { get { return this.index; } set { this.index = value; } }
        
        private int line;
        public int Line { get { return this.line; } set { this.line = value; } }

        private int column;
        public int Column { get { return this.column; } set { this.column = value > 0 ? value : (GetPrevLength() + 1); } }

        public string LeftPart { get { return item.ToString().Substring(0, itemindex); } }

        public string RightPart { get { return item.ToString().Substring(itemindex); } }
        
        #endregion

        #region Method

        #region IComparable

        public int CompareTo(ITextPosition that)
        {
            int cmpline = this.Line.CompareTo(that.Line);
            int cmpcolumn = this.Column.CompareTo(that.Column);
            return cmpline != 0 ? cmpline : cmpcolumn;
        }

        #endregion

        public static void RefNextLineColumn(string text, ref int line, ref int column)
        {
            int entercount = text.Count(c => c == '\n');
            int enterid = text.LastIndexOf('\n');
            if (entercount > 0)
            {
                line += entercount;
                column = text.Length - enterid;
            }
            else
                column += text.Length;
        }

        public static void RefPrevLineColumn(string text, ref int line, ref int column)
        {
            int entercount = text.Count(c => c == '\n');
            //int enterid = text.IndexOf('\n');
            if (entercount > 0)
            {
                line -= entercount;
                column = -1;
            }
            else
                column -= text.Length;
        }

        public static ITextPosition GetNextLine(ITextZone zone, int start, int line)
        {
            for (int i = start; i < zone.Items.Count; i++)
            {
                if (zone.Items[i] is ITextTrim)
                {
                    ITextTrim trim = (ITextTrim)(zone.Items[i]);
                    int enterid = trim.ToString().IndexOf('\n');
                    if (enterid >= 0) return new TextPosition() { Item = trim, ItemIndex = enterid + 1, Line = line, Column = 1 };
                }
                if (zone.Items[i] is ITextZone)
                {
                    ITextZone _zone = (ITextZone)(zone.Items[i]);
                    ITextPosition result = GetNextLine(_zone, 0, line);
                    if (result != null) return result;
                }
            }
            return null;
        }

        public static ITextPosition GetPrevLine(ITextZone zone, int start, int line)
        {
            for (int i = start; i >= 0; i--)
            {
                if (zone.Items[i] is ITextTrim)
                {
                    ITextTrim trim = (ITextTrim)(zone.Items[i]);
                    int enterid = trim.ToString().LastIndexOf('\n');
                    if (enterid >= 0) return new TextPosition() { Item = trim, ItemIndex = enterid, Line = line, Column = -1 };
                }
                if (zone.Items[i] is ITextZone)
                {
                    ITextZone _zone = (ITextZone)(zone.Items[i]);
                    ITextPosition result = GetPrevLine(_zone, _zone.Items.Count - 1, line);
                    if (result != null) return result;
                }
            }
            return null;
        }

        public static bool GetNextLength(ITextZone zone, int start, ref int count)
        {
            for (int i = start; i < zone.Items.Count; i++)
            {
                if (zone.Items[i] is ITextTrim)
                {
                    ITextTrim trim = (ITextTrim)(zone.Items[i]);
                    int enterid = trim.ToString().IndexOf('\n');
                    if (enterid >= 0) { count += enterid; return true; }
                }
                if (zone.Items[i] is ITextZone)
                {
                    ITextZone _zone = (ITextZone)(zone.Items[i]);
                    if (GetNextLength(_zone, 0, ref count)) return true;
                    continue;
                }
                count += zone.Items[i].ToString().Length;
            }
            return false;
        }

        public static bool GetPrevLength(ITextZone zone, int start, ref int count)
        {
            for (int i = start; i >= 0; i--)
            {
                if (zone.Items[i] is ITextTrim)
                {
                    ITextTrim trim = (ITextTrim)(zone.Items[i]);
                    int enterid = trim.ToString().LastIndexOf('\n');
                    if (enterid >= 0) { count += trim.ToString().Length - enterid - 1; return true; }
                }
                if (zone.Items[i] is ITextZone)
                {
                    ITextZone _zone = (ITextZone)(zone.Items[i]);
                    if (GetPrevLength(_zone, _zone.Items.Count - 1, ref count)) return true;
                    continue;
                }
                count += zone.Items[i].ToString().Length;
            }
            return false;
        }

        public static bool GetByteSpaceCount(ITextZone zone, int start, ref int count)
        {
            for (int i = start; i >= 0; i--)
            {
                if (zone.Items[i] is ITextTrim)
                {
                    ITextTrim trim = (ITextTrim)(zone.Items[i]);
                    int enterid = trim.ToString().LastIndexOf('\n');
                    if (enterid >= 0)
                    {
                        string text = trim.ToString().Substring(enterid + 1);
                        foreach (char ch in text)
                            count += TextChar.GetByteSpaceCount(ch);
                        return true;
                    }
                }
                if (zone.Items[i] is ITextZone)
                {
                    ITextZone _zone = (ITextZone)(zone.Items[i]);
                    if (GetByteSpaceCount(_zone, 0, ref count)) return true;
                    continue;
                }
                foreach (char ch in zone.Items[i].ToString())
                    count += TextChar.GetByteSpaceCount(ch);
            }
            return false;
        }

        public static ITextPosition GetNextOffset(ITextZone zone, int start, ref int offset, ref int line, ref int column)
        {
            for (int i = start; i < zone.Items.Count; i++)
            {
                if (zone.Items[i] is ITextZone)
                {
                    ITextZone _zone = (ITextZone)(zone.Items[i]);
                    ITextPosition result = GetNextOffset(_zone, 0, ref offset, ref line, ref column);
                    if (result != null) return result;
                }
                else 
                {
                    string text = zone.Items[i].ToString();
                    if (offset < text.Length) text = text.Substring(0, offset);
                    RefNextLineColumn(text, ref line, ref column);
                    offset -= text.Length;
                    if (offset <= 0) return new TextPosition() { Item = zone.Items[i], ItemIndex = text.Length, Line = line, Column = column };
                }
            }
            return null;
        }

        public static ITextPosition GetPrevOffset(ITextZone zone, int start, ref int offset, ref int line, ref int column)
        {
            for (int i = start; i >= 0; i--)
            {
                if (zone.Items[i] is ITextZone)
                {
                    ITextZone _zone = (ITextZone)(zone.Items[i]);
                    ITextPosition result = GetPrevOffset(_zone, _zone.Items.Count - 1, ref offset, ref line, ref column);
                    if (result != null) return result;
                }
                else
                {
                    string text = zone.Items[i].ToString();
                    if (offset < text.Length) text = text.Substring(text.Length - offset);
                    RefPrevLineColumn(text, ref line, ref column);
                    offset -= text.Length;
                    if (offset <= 0) return new TextPosition() { Item = zone.Items[i], ItemIndex = zone.Items[i].ToString().Length - text.Length, Line = line, Column = column };
                }
            }
            return null;
        }
        
        public ITextPosition Clone()
        {
            return new TextPosition() { Item = this.item, ItemIndex = this.itemindex, Line = this.line, Column = this.column };
        }

        public ITextPosition NextSeek()
        {
            if (itemindex < item.ToString().Length) return Clone();
            ITextItem nextitem = item;
            while (nextitem?.Parent != null && nextitem.ID + 1 >= nextitem.Parent.Items.Count)
                nextitem = nextitem.Parent;
            if (nextitem?.Parent == null) return Clone();
            nextitem = nextitem.Parent.Items[nextitem.ID + 1];
            while (nextitem is ITextZone)
            {
                ITextZone zone = (ITextZone)(nextitem);
                nextitem = zone.Items.FirstOrDefault();
            }
            return new TextPosition() { Item = nextitem, ItemIndex = 0, Line = this.line, Column = this.column };
        }

        public ITextPosition PrevSeek()
        {
            if (itemindex > 0) return Clone();
            ITextItem previtem = item;
            while (previtem?.Parent != null && previtem.ID <= 0)
                previtem = previtem.Parent;
            if (previtem?.Parent == null) return Clone();
            previtem = previtem.Parent.Items[previtem.ID - 1];
            while (previtem is ITextZone)
            {
                ITextZone zone = (ITextZone)(previtem);
                previtem = zone.Items.LastOrDefault();
            }
            return new TextPosition() { Item = previtem, ItemIndex = previtem.ToString().Length, Line = this.line, Column = this.column };
        }

        public ITextPosition Next()
        {
            ITextItem nextitem = item;
            char nextchar = default(char);
            int nextindex = -1;
            int nextline = line;
            int nextcolumn = column + 1;
            if (itemindex >= item.ToString().Length)
            {
                while (nextitem?.Parent != null && nextitem.ID + 1 >= nextitem.Parent.Items.Count)
                    nextitem = nextitem.Parent;
                if (nextitem?.Parent == null) return null;
                nextitem = nextitem.Parent.Items[nextitem.ID + 1];
                while (nextitem is ITextZone)
                {
                    ITextZone zone = (ITextZone)(nextitem);
                    nextitem = zone.Items.FirstOrDefault();
                }
                nextchar = nextitem.ToString()[0];
                nextindex = 1;
            }
            else
            {
                nextchar = item.ToString()[itemindex];
                nextindex = itemindex + 1;
            }
            if (nextchar == '\n')
            {
                nextline = line + 1;
                nextcolumn = 1;
            }
            return new TextPosition() { Item = nextitem, ItemIndex = nextindex, Line = nextline, Column = nextcolumn };
        }

        public ITextPosition Prev()
        {
            ITextItem previtem = item;
            char prevchar = default(char);
            int previndex = itemindex - 1;
            int prevline = line;
            int prevcolumn = column - 1;
            if (itemindex <= 0)
            {
                while (previtem?.Parent != null && previtem.ID <= 0)
                    previtem = previtem.Parent;
                if (previtem?.Parent == null) return null;
                previtem = previtem.Parent.Items[previtem.ID - 1];
                while (previtem is ITextZone)
                {
                    ITextZone zone = (ITextZone)(previtem);
                    previtem = zone.Items.LastOrDefault();
                }
                prevchar = previtem.ToString().LastOrDefault();
                previndex = previtem.ToString().Length - 1;
            }
            else
            {
                prevchar = previtem.ToString()[itemindex - 1];
                previndex = itemindex - 1;
            }
            if (prevchar == '\n')
            {
                prevline = line - 1;
                prevcolumn = -1;
            }
            return new TextPosition() { Item = previtem, ItemIndex = previndex, Line = prevline, Column = prevcolumn };
        }

        public ITextPosition NextItem()
        {
            ITextItem nextitem = item;
            string nexttext = null;
            int nextindex = itemindex;
            int nextline = line;
            int nextcolumn = column + 1;
            if (itemindex >= item.ToString().Length)
            {
                while (nextitem?.Parent != null && nextitem.ID + 1 >= nextitem.Parent.Items.Count)
                    nextitem = nextitem.Parent;
                if (nextitem?.Parent == null) return null;
                nextitem = nextitem.Parent.Items[nextitem.ID + 1];
                while (nextitem is ITextZone)
                {
                    ITextZone zone = (ITextZone)(nextitem);
                    nextitem = zone.Items.FirstOrDefault();
                }
                nexttext = nextitem.ToString();
                nextindex = nexttext.Length;
            }
            else
            {
                nexttext = RightPart;
                nextindex += nexttext.Length;
            }
            RefNextLineColumn(nexttext, ref nextline, ref nextcolumn);
            return new TextPosition() { Item = nextitem, ItemIndex = nextindex, Line = nextline, Column = nextcolumn };
        }

        public ITextPosition PrevItem()
        {
            ITextItem previtem = item;
            string prevtext = null;
            int previndex = itemindex;
            int prevline = line;
            int prevcolumn = column - 1;
            if (itemindex <= 0)
            {
                while (previtem?.Parent != null && previtem.ID <= 0)
                    previtem = previtem.Parent;
                if (previtem?.Parent == null) return null;
                previtem = previtem.Parent.Items[previtem.ID - 1];
                while (previtem is ITextZone)
                {
                    ITextZone zone = (ITextZone)(previtem);
                    previtem = zone.Items.LastOrDefault();
                }
                prevtext = previtem.ToString();
                previndex = 0;
            }
            else
            {
                prevtext = LeftPart;
                previndex = 0;
            }
            RefPrevLineColumn(prevtext, ref prevline, ref prevcolumn);
            return new TextPosition() { Item = previtem, ItemIndex = previndex, Line = prevline, Column = prevcolumn };
        }

        public ITextPosition NextItemNonTrim()
        {
            ITextPosition nextpos = NextItem();
            while (nextpos != null && nextpos.Item is ITextTrim)
                nextpos = nextpos.NextItem();
            return nextpos;
        }

        public ITextPosition PrevItemNonTrim()
        {
            ITextPosition prevpos = PrevItem();
            while (prevpos != null && prevpos.Item is ITextTrim)
                prevpos = prevpos.PrevItem();
            return prevpos;
        }

        public ITextPosition NextLine()
        {
            ITextItem nextitem = item;
            if (nextitem is ITextTrim)
            {
                string rightpart = RightPart;
                int enterid = rightpart.IndexOf('\n');
                if (enterid >= 0) return new TextPosition() { Item = nextitem, ItemIndex = this.itemindex + enterid + 1, Line = this.line + 1, Column = 1 };
            }
            while (nextitem?.Parent != null)
            {
                ITextZone zone = nextitem.Parent;
                ITextPosition result = GetNextLine(zone, nextitem.ID + 1, line + 1);
                if (result != null) return result;
                nextitem = zone;
            }
            return null;
        }

        public ITextPosition PrevLine()
        {
            ITextItem previtem = item;
            if (previtem is ITextTrim)
            {
                string leftpart = LeftPart;
                int enterid = leftpart.LastIndexOf('\n');
                if (enterid >= 0) return new TextPosition() { Item = previtem, ItemIndex = enterid, Line = this.line - 1, Column = -1 };
            }
            while (previtem?.Parent != null)
            {
                ITextZone zone = previtem.Parent;
                ITextPosition result = GetPrevLine(zone, previtem.ID - 1, line - 1);
                if (result != null) return result;
                previtem = zone;
            }
            return null;
        }

        public ITextPosition Up()
        {
            ITextPosition prevline = PrevLine();
            if (prevline == null) return item.Core.GetFirstPosition();
            if (prevline.Column > this.Column)
                prevline = prevline.Move(this.Column - prevline.Column);
            return prevline ?? item.Core.GetFirstPosition();
        }
        
        public ITextPosition Down()
        {
            ITextPosition nextline = NextLine()?.NextLine()?.Prev();
            if (nextline == null) return item.Core.GetLastPosition();
            if (nextline.Column > this.Column)
                nextline = nextline.Move(this.Column - nextline.Column);
            return nextline ?? item.Core.GetLastPosition();
        }
        
        public ITextPosition Move(int offset)
        {
            if (offset == 0) return Clone();
            if (offset > 0)
            {
                ITextItem nextitem = item;
                int nextline = line;
                int nextcolumn = column;
                string rightpart = RightPart;
                if (offset <= rightpart.Length)
                {
                    rightpart = rightpart.Substring(0, offset);
                    RefNextLineColumn(rightpart, ref nextline, ref nextcolumn);
                    return new TextPosition() { Item = nextitem, ItemIndex = this.itemindex + offset, Line = nextline, Column = nextcolumn };
                }
                RefNextLineColumn(rightpart, ref nextline, ref nextcolumn);
                offset -= rightpart.Length;
                while (nextitem?.Parent != null)
                {
                    ITextZone zone = nextitem.Parent;
                    ITextPosition result = GetNextOffset(zone, nextitem.ID + 1, ref offset, ref nextline, ref nextcolumn);
                    if (result != null) return result;
                    nextitem = zone;
                }
            }
            else
            {
                offset = -offset;
                ITextItem previtem = item;
                int prevline = line;
                int prevcolumn = column;
                string leftpart = LeftPart;
                if (offset <= leftpart.Length)
                {
                    leftpart = leftpart.Substring(leftpart.Length - offset);
                    RefPrevLineColumn(leftpart, ref prevline, ref prevcolumn);
                    return new TextPosition() { Item = previtem, ItemIndex = this.itemindex - offset, Line = prevline, Column = prevcolumn };
                }
                RefPrevLineColumn(leftpart, ref prevline, ref prevcolumn);
                offset -= leftpart.Length;
                while (previtem?.Parent != null)
                {
                    ITextZone zone = previtem.Parent;
                    ITextPosition result = GetPrevOffset(zone, previtem.ID - 1, ref offset, ref prevline, ref prevcolumn);
                    if (result != null) return result;
                    previtem = zone;
                }
            }
            return null;
        }
        
        public int GetPrevLength()
        {
            ITextItem previtem = item;
            int count = 0;
            if (previtem is ITextTrim)
            {
                string leftpart = LeftPart;
                int enterid = leftpart.LastIndexOf('\n');
                if (enterid >= 0) return leftpart.Length - enterid - 1;
            }
            count += itemindex;
            while (previtem?.Parent != null)
            {
                ITextZone zone = previtem.Parent;
                if (GetPrevLength(zone, previtem.ID - 1, ref count)) return count;
                previtem = previtem.Parent;
            }
            return count;
        }

        public int GetNextLength()
        {
            ITextItem nextitem = item;
            int count = 0;
            if (nextitem is ITextTrim)
            {
                string rightpart = RightPart;
                int enterid = rightpart.IndexOf('\n');
                if (enterid >= 0) return enterid;
            }
            count += item.ToString().Length - itemindex;
            while (nextitem?.Parent != null)
            {
                ITextZone zone = nextitem.Parent;
                if (GetNextLength(zone, nextitem.ID + 1, ref count)) return count;
                nextitem = nextitem.Parent;
            }
            return count;
        }

        public int GetByteSpaceCount()
        {
            ITextItem previtem = item;
            int count = 0;
            if (previtem is ITextTrim)
            {
                string leftpart = LeftPart;
                int enterid = leftpart.LastIndexOf('\n');
                if (enterid >= 0)
                {
                    for (int i = enterid + 1; i < leftpart.Length; i++)
                        count += TextChar.GetByteSpaceCount(leftpart[i]);
                    return count;
                }
            }
            foreach (char ch in LeftPart)
                count += TextChar.GetByteSpaceCount(ch);
            while (previtem?.Parent != null)
            {
                ITextZone zone = previtem.Parent;
                if (GetByteSpaceCount(zone, previtem.ID - 1, ref count)) return count;
                previtem = previtem.Parent;
            }
            return count;
        }
        
        #endregion

    }
}
