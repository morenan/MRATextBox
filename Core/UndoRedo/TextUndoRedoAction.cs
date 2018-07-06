using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Morenan.MRATextBox.Core.UndoRedo
{
    internal class TextUndoRedoAction : ITextUndoRedoAction
    {
        #region Resource

        private static readonly Regex Regex_NamedOnly = new Regex(@"^[a-z_][a-z0-9_]*$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex Regex_DigitalOnly = new Regex(@"^[0-9]+$", RegexOptions.Compiled);
        private static readonly Regex Regex_NumeralOnly = new Regex(@"^[0-9]+\.[0-9]+$", RegexOptions.Compiled);
        private static readonly Regex Regex_TrimOnly = new Regex(@"^\s+$", RegexOptions.Compiled);

        protected static bool IsPureText(string text)
        {
            Match match = Regex_NamedOnly.Match(text);
            if (match.Success) return true;
            match = Regex_DigitalOnly.Match(text);
            if (match.Success) return true;
            match = Regex_NumeralOnly.Match(text);
            if (match.Success) return true;
            match = Regex_TrimOnly.Match(text);
            if (match.Success) return true;
            return false;
        }
        
        #endregion

        public TextUndoRedoAction()
        {
            this.removedtext = String.Empty;
            this.replacedtext = String.Empty;
            this.backspace = false;
            this.backspacetext = String.Empty;
        }

        #region Number

        private int oldlinestart;
        public int OldLineStart { get { return this.oldlinestart; } set { this.oldlinestart = value; } }

        private int oldcolumnstart;
        public int OldColumnStart { get { return this.oldcolumnstart; } set { this.oldcolumnstart = value; } }

        private int oldlineend;
        public int OldLineEnd { get { return this.oldlineend; } set { this.oldlineend = value; } }
        
        private int oldcolumnend;
        public int OldColumnEnd { get { return this.oldcolumnend; } set { this.oldcolumnend = value; } }

        private int newlinestart;
        public int NewLineStart { get { return this.newlinestart; } set { this.newlinestart = value; } }

        private int newcolumnstart;
        public int NewColumnStart { get { return this.newcolumnstart; } set { this.newcolumnstart = value; } }

        private int newlineend;
        public int NewLineEnd { get { return this.newlineend; } set { this.newlineend = value; } }

        private int newcolumnend;
        public int NewColumnEnd { get { return this.newcolumnend; } set { this.newcolumnend = value; } }

        private string removedtext;
        public string RemovedText { get { return this.removedtext ?? String.Empty; } set { this.removedtext = value; } }

        private string replacedtext;
        public string ReplacedText { get { return this.replacedtext ?? String.Empty; } set { this.replacedtext = value; } }

        private bool backspace;
        public bool Backspace { get { return this.backspace; } set { this.backspace = value; } }

        private string backspacetext;
        public string BackspaceText { get { return this.backspacetext ?? String.Empty; } set { this.backspacetext = value; } }

        #endregion

        #region Method

        public ITextUndoRedoAction Concat(ITextUndoRedoAction that)
        {
            if (this.NewLineStart != that.OldLineStart) return null;
            if (this.NewLineEnd != that.OldLineEnd) return null;
            if (this.NewColumnStart != that.OldColumnStart) return null;
            if (this.NewColumnEnd != that.OldColumnEnd) return null;
            if (this.backspace)
            {
                if (!that.Backspace) return null;
                string newtext = that.BackspaceText + this.BackspaceText;
                if (IsPureText(newtext)) return new TextUndoRedoAction() {
                    OldLineStart = this.OldLineStart, OldLineEnd = this.OldLineEnd, OldColumnStart = this.OldColumnStart, OldColumnEnd = this.OldColumnEnd,
                    NewLineStart = that.NewLineStart, NewLineEnd = that.NewLineEnd, NewColumnStart = that.NewColumnStart, NewColumnEnd = that.NewColumnEnd,
                    Backspace = true, BackspaceText = newtext};
                return null;
            }
            if (this.RemovedText.Length > 0) return null;
            if (that.RemovedText.Length > 0) return null;
            if (this.ReplacedText.Length > 0 && that.ReplacedText.Length > 0)
            {
                string newtext = this.BackspaceText + that.BackspaceText;
                if (IsPureText(newtext)) return new TextUndoRedoAction() {
                    OldLineStart = this.OldLineStart, OldLineEnd = this.OldLineEnd, OldColumnStart = this.OldColumnStart, OldColumnEnd = this.OldColumnEnd,
                    NewLineStart = that.NewLineStart, NewLineEnd = that.NewLineEnd, NewColumnStart = that.NewColumnStart, NewColumnEnd = that.NewColumnEnd,
                    ReplacedText = newtext };
                return null;
            }
            return null;
        }

        #endregion
    }
}
