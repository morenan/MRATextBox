using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Morenan.MRATextBox.Core
{
    internal class TextChar : TextItem, ITextChar
    {
        #region Resource

        public static bool IsTrim(char ch)
        {
            return ch == ' ' || ch == '\t' || ch == '\r' || ch == '\n';
        }

        public static bool IsWord(char ch)
        {
            return (ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z') || (ch >= '0' && ch <= '9') || ch == '_';
        }

        public static bool IsKey(char ch)
        {
            return !IsWord(ch) && !IsTrim(ch);
        }

        public static int GetByteSpaceCount(char ch)
        {
            return ch < 256 ? 1 : 2;
        }

        #endregion

        public TextChar(TextBoxCore _core, char _ch) : base(_core)
        {
            this.ch = _ch;
        }

        protected override void _Dispose()
        {
            base._Dispose();
        }

        public override string ToString()
        {
            return ch.ToString();
        }

        #region Number

        private char ch;
        public char Char { get { return this.ch; } }

        #endregion

    }
}
