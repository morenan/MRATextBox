using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Morenan.MRATextBox.Core.UndoRedo
{
    internal class TextUndoRedoAction : TextAction, ITextUndoRedoAction
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
