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
        IList<ITextUndoRedoAction> Undos { get; }
        IList<ITextUndoRedoAction> Redos { get; }
        void Clear();
        void Add(ITextUndoRedoAction action);
        ITextUndoRedoAction Undo();
        ITextUndoRedoAction Redo();
    }

    internal interface ITextUndoRedoAction
    {
        int OldLineStart { get; set; }
        int OldLineEnd { get; set; }
        int OldColumnStart { get; set; }
        int OldColumnEnd { get; set; }
        int NewLineStart { get; set; }
        int NewLineEnd { get; set; }
        int NewColumnStart { get; set; }
        int NewColumnEnd { get; set; }
        bool Backspace { get; set; }
        string RemovedText { get; set; }
        string ReplacedText { get; set; }
        string BackspaceText { get; set; }
        ITextUndoRedoAction Concat(ITextUndoRedoAction action);
    }
}
