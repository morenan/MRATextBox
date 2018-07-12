using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Morenan.MRATextBox.Core;

namespace Morenan.MRATextBox.View
{
    internal interface IMRATextItemInfo : IDisposable
    {
        bool IsDisposed { get; }
        ITextBoxCore Core { get; }
        int ID { get; set; }
        int Line { get; set; }
        ITextPosition Start { get; set; }
        ITextPosition End { get; set; }
        MRATextItemView View { get; set; }
        bool OpenZoneFocus { get; set; }
        bool CloseZoneFocus { get; set; }
        bool IntoZoneFocus { get; set; }
        bool ContainLine(int _line);
    }
    
    internal class MRATextItemInfo : IMRATextItemInfo
    {
        public MRATextItemInfo(ITextBoxCore _core, int _id, int _line)
        {
            this.core = _core;
            this.id = _id;
            this.line = _line;
            this.view = null;
            this.openzonefocus = false;
            this.closezonefocus = false;
            this.intozonefocus = false;
        }

        private bool isdisposed = false;
        public bool IsDisposed { get { return this.isdisposed; } }

        public virtual void Dispose()
        {
            if (isdisposed) return;
            this.isdisposed = true;
            this.core = null;
            this.id = -1;
            this.line = -1;
            this.start = null;
            this.end = null;
        }

        #region Number

        private ITextBoxCore core;
        public ITextBoxCore Core { get { return this.core; } }

        private int line;
        public int Line { get { return this.line; } set { this.line = value; } }

        private int id;
        public int ID { get { return this.id; } set { this.id = value; } }

        private ITextPosition start;
        public ITextPosition Start { get { return this.start; } set { this.start = value; } }

        private ITextPosition end;
        public ITextPosition End { get { return this.end; } set { this.end = value; } }

        private MRATextItemView view;
        public MRATextItemView View { get { return this.view; } set { this.view = value; } }
        
        private bool openzonefocus;
        public bool OpenZoneFocus { get { return this.openzonefocus; } set { this.openzonefocus = value; } }

        private bool closezonefocus;
        public bool CloseZoneFocus { get { return this.closezonefocus; } set { this.closezonefocus = value; } }

        private bool intozonefocus;
        public bool IntoZoneFocus { get { return this.intozonefocus; } set { this.intozonefocus = value; } }

        #endregion

        #region Method

        public bool ContainLine(int _line) { return this.line == _line; }

        #endregion

    }
}
