using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Morenan.MRATextBox.Core.UndoRedo
{
    internal interface ITextUndoRedoCore
    {
        ITextBoxCore Parent { get; }
        IList<ITextAction> Undos { get; }
        IList<ITextAction> Redos { get; }
        void Clear();
        void Add(ITextAction action);
        ITextAction Undo();
        ITextAction Redo();
    }

    internal interface ITextAction
    {
        int OldLineStart { get; set; }
        int OldLineEnd { get; set; }
        int OldColumnStart { get; set; }
        int OldColumnEnd { get; set; }
        int NewLineStart { get; set; }
        int NewLineEnd { get; set; }
        int NewColumnStart { get; set; }
        int NewColumnEnd { get; set; }
    }

    internal interface ITextUndoRedoAction : ITextAction
    {
        bool Backspace { get; set; }
        string RemovedText { get; set; }
        string ReplacedText { get; set; }
        string BackspaceText { get; set; }
        ITextUndoRedoAction Concat(ITextUndoRedoAction action);
    }
    
    internal interface ITextIndentAction : ITextAction
    {
        int IndentStartLine { get; set; }
        int[] IndentSpaces { get; set; }
        int[] IndentIndics { get; set; }
    }

    internal interface IText1to3LineAction : ITextAction
    {
        int SourceLine { get; set; }
        int ToLineNumber { get; set; }
        ITextZone Zone { get; set; }
    }
}
