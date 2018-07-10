using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Morenan.MRATextBox.Core
{
    internal class TextLine : TextZone, ITextLine
    {
        public TextLine(TextBoxCore _core, ITextKey _left, ITextTrim _enter) : base(_core, _left, null)
        {
            if (_enter != null) Add(_enter);
            Doc = _left?.Doc?.Parent;
        }

        public bool HasEnterEnd()
        {
            for (int i = Items.Count - 1; i >= 0; i--)
            {
                ITextItem item = Items[i];
                if (item is ITextTrim)
                {
                    ITextTrim trim = (ITextTrim)item;
                    return trim.GetEnterCount() > 0;
                }
                if (item is ITextLine)
                {
                    ITextLine line = (ITextLine)item;
                    return line.HasEnterEnd();
                }
                return false;
            }
            return false;
        }

        public bool HasEnterContinue()
        {
            for (int i = Items.Count - 1; i >= 0; i--)
            {
                ITextItem item = Items[i];
                if (item is ITextTrim)
                {
                    ITextTrim trim = (ITextTrim)item;
                    if (trim.GetEnterCount() > 0) return false;
                    continue;
                }
                if (item is ITextLine)
                {
                    ITextLine line = (ITextLine)item;
                    return line.HasEnterContinue();
                }
                return false;
            }
            return false;
        }
        
    }
}
