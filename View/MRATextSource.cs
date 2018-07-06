using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.TextFormatting;
using Morenan.MRATextBox.Core;

namespace Morenan.MRATextBox.View
{
    internal class MRATextSource : TextSource
    {
        public MRATextSource(string _text, IList<ITextFontIndex> _cids)
        {
            this.text = _text;
            this.cids = _cids;
            this.cid = 0;
        }

        #region Number

        private string text;
        public string Text { get { return this.text; } }

        private IList<ITextFontIndex> cids;
        public IList<ITextFontIndex> CIDs { get { return this.cids; } }

        private int cid;

        #endregion

        #region Method

        public override TextSpan<CultureSpecificCharacterBufferRange> GetPrecedingText(int textSourceCharacterIndexLimit)
        {
            CharacterBufferRange range1 = new CharacterBufferRange(text, 0, Math.Min(text.Length, textSourceCharacterIndexLimit));
            CultureSpecificCharacterBufferRange range2 = new CultureSpecificCharacterBufferRange(Thread.CurrentThread.CurrentUICulture, range1);
            return new TextSpan<CultureSpecificCharacterBufferRange>(range1.Length, range2);
        }

        public override int GetTextEffectCharacterIndexFromTextSourceCharacterIndex(int id)
        {
            return id;
        }

        public override TextRun GetTextRun(int tid)
        {
            if (tid >= text.Length) return new TextEndOfLine(1);
            while (cid >= 0 && cids[cid].Index > tid) cid--;
            while (cid + 1 < cids.Count && cids[cid + 1].Index <= tid) cid++;
            int length = (cid + 1 < cids.Count ? cids[cid + 1].Index : text.Length) - tid;
            return new TextCharacters(text, tid, length, new MRATextProperties(cids[cid].Core));
        }

        #endregion
    }

    
}
