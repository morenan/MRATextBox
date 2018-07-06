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
        
    }
}
