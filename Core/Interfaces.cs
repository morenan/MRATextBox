using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Morenan.MRATextBox.Core.Documents;

namespace Morenan.MRATextBox.Core
{
    internal interface ITextBoxCore : ITextFontCore, IDisposable
    {
        IList<ITextItem> Items { get; }
        IDocsCore DocsCore { get; }
        IDictionary<string, ITextKeyCore> DictKey { get; }
        IDictionary<char, ITextKeyCore> DictOneKey { get; }
        IDictionary<string, Brush> DictBrush { get; }
        IDictionary<string, double> DictValue { get; }
        int LineCount { get; }
        ITextPosition SelectedStart { get; set; }
        ITextPosition SelectedEnd { get; set; }
        ITextPosition GetFirstPosition();
        ITextPosition GetLastPosition();
        ITextPosition GetLinePosition(int line);
        ITextPosition GetPosition(int line, int column);
        ITextPosition GetFirstPosition(ITextItem item);
        ITextPosition GetLastPosition(ITextItem item);
        int GetFirstLine(ITextItem item);
        int GetLastLine(ITextItem item);
        string GetSelectedText();
        string GetText(ITextPosition start, ITextPosition end);
        MRATextBox View { get; }
        void Cut();
        void Copy();
        void Paste();
        void Delete();
        void SelectAll();
        bool CanUndo();
        bool CanRedo();
        void Undo();
        void Redo();
    }

    internal interface ITextFontCore
    {
        FontStyle FontStyle { get; }
        FontStretch FontStretch { get; }
        FontFamily FontFamily { get; }
        FontWeight FontWeight { get; }
        double FontSize { get; }
        Brush Foreground { get; }
        Brush Background { get; }
        ITextFontCore Clone();
    }

    internal interface ITextFontIndex
    {
        ITextFontCore Core { get; }
        int Index { get; }
    }

    internal interface ITextKeyCore
    {
        TextKeyFeatures Feature { get; set; }
        TextKeyRelations Relation { get; set; }
        ITextKeyCore That { get; set; }
        ITextKeyCore Into { get; set; }
        ITextKeyInfo Info { get; set; }
        ITextKeyCollection Collection { get; set; }
        IDocsItem Doc { get; set; }
        string Keyword { get; set; }
    }

    internal interface ITextKeyCollection
    {
        ITextKeyCore Prototype { get; }
        IList<ITextKeyCore> Items { get; }
    }

    internal interface ITextKeyInfo
    {

    }

    internal interface ITextPosition : IComparable<ITextPosition>
    {
        ITextItem Item { get; set; }
        int ItemIndex { get; set; }
        int Index { get; set; }
        int Line { get; set; }
        int Column { get; set; }
        string LeftPart { get; }
        string RightPart { get; }

        ITextPosition Clone();
        ITextPosition NextSeek();
        ITextPosition PrevSeek();
        ITextPosition Next();
        ITextPosition Prev();
        ITextPosition NextLine();
        ITextPosition PrevLine();
        ITextPosition NextItem();
        ITextPosition PrevItem();
        ITextPosition NextItemNonTrim();
        ITextPosition PrevItemNonTrim();
        ITextPosition Move(int offset);
        ITextPosition MoveLine(int lineoffset);
        ITextPosition Up();
        ITextPosition Down();
        int GetPrevLength();
        int GetNextLength();
        int GetByteSpaceCount();
    }

    internal interface ITextItem : IDisposable
    {
        ITextBoxCore Core { get; }
        IDocsItem Doc { get; set; }
        ITextZone Parent { get; set; }
        int ID { get; set; }
        int Level { get; set; }
        ITextItem Insert(int id, string text);
        ITextItem Remove(int id, int count);
        bool IsAncestorOf(ITextItem that);
        bool IsChildOf(ITextItem that);
    }
    
    internal interface ITextZone : ITextItem
    {
        IList<ITextItem> Items { get; }
        int LineCount { get; set; }
        int SkipCount { get; set; }
        bool IsSkip { get; set; }
        void Add(ITextItem item);
        void Remove(ITextItem item);
        void Replace(int start, int count, IEnumerable<ITextItem> _additems);
        IEnumerable<ITextItem> GetRange(int start, int count);
    }

    internal interface ITextLine : ITextZone
    {
        bool HasEnterEnd();
        bool HasEnterContinue();
    }
    
    internal interface ITextWord : ITextItem
    {

    }

    internal interface ITextChar : ITextWord
    {

    }
    
    internal interface ITextTrim : ITextWord
    {
        int GetEnterCount();
    }

    internal interface ITextKey : ITextWord
    {
        ITextKeyCore KeyCore { get; }
    }
}
