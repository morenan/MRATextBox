using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Morenan.MRATextBox.Core
{
    internal class TextWord : TextItem, ITextWord
    {
        public TextWord(TextBoxCore _core, string _word) : base(_core)
        {
            this.word = _word;
        }

        protected override void _Dispose()
        {
            this.word = null;
            base._Dispose();
        }

        public override string ToString()
        {
            return word;
        }

        #region Number

        private string word;
        public string Word { get { return this.word; } set { this.word = value; } }
    
        #endregion
        
    }

    internal class TextTrim : TextWord, ITextTrim
    {
        public TextTrim(TextBoxCore _core, string _word) : base(_core, _word)
        {
        }

        public int GetEnterCount()
        {
            return Word.Count(c => c == '\n');
        }

        public override ITextItem Insert(int id, string text)
        {
            ITextItem result = new TextTrim((TextBoxCore)Core, ToString().Insert(id, text))
            { Parent = this.Parent, ID = this.ID, Level = this.Level };
            if (((TextBoxCore)Core).WaitCaret == this)
                ((TextBoxCore)Core).WaitCaret = result;
            return result;
        }

        public override ITextItem Remove(int id, int count)
        {
            return new TextTrim((TextBoxCore)Core, ToString().Remove(id, count))
            { Parent = this.Parent, ID = this.ID, Level = this.Level };
        }

    }
}
