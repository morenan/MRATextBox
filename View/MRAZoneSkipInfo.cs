using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Morenan.MRATextBox.Core;

namespace Morenan.MRATextBox.View
{
    internal interface IMRAZoneSkipInfo : IMRATextItemInfo
    {
        int LineStart { get; set; }
        int LineEnd { get; set; }
        ITextZone SkipZone { get; set; }
        bool SkipZoneFocus { get; set; }
    }

    internal class MRAZoneSkipInfo : IMRAZoneSkipInfo
    {
        public MRAZoneSkipInfo(ITextBoxCore _core, int _id, int _linestart, int _lineend, ITextZone _skipzone)
        {
            this.core = _core;
            this.id = _id;
            this.linestart = _linestart;
            this.lineend = _lineend;
            this.skipzone = _skipzone;
            this.openzonefocus = false;
            this.closezonefocus = false;
            this.intozonefocus = false;
        }

        private bool isdisposed = false;
        public bool IsDisposed { get { return this.isdisposed; } }

        public void Dispose()
        {
            if (isdisposed) return;
            this.isdisposed = true;
            this.core = null;
            this.skipzone = null;
        }
        
        #region Number

        private ITextBoxCore core;
        public ITextBoxCore Core { get { return this.core; } }

        private int id;
        public int ID { get { return this.id; } set { this.id = value; } }

        private int linestart;
        public int LineStart { get { return this.linestart; } set { this.linestart = value; } }

        private int lineend;
        public int LineEnd { get { return this.lineend; } set { this.lineend = value; } }

        int IMRATextItemInfo.Line
        {
            get { return this.linestart; }
            set { this.lineend += value - linestart; this.linestart = value; }
        }

        private ITextPosition start;
        public ITextPosition Start { get { return this.start; } set { this.start = value; } }

        private ITextPosition end;
        public ITextPosition End { get { return this.end; } set { this.end = value; } }

        private ITextZone skipzone;
        public ITextZone SkipZone { get { return this.skipzone; } set { this.skipzone = value; } }

        private MRATextItemView view;
        public MRATextItemView View { get { return this.view; } set { this.view = value; } }

        private bool openzonefocus;
        public bool OpenZoneFocus { get { return this.openzonefocus; } set { this.openzonefocus = value; } }

        private bool closezonefocus;
        public bool CloseZoneFocus { get { return this.closezonefocus; } set { this.closezonefocus = value; } }

        private bool intozonefocus;
        public bool IntoZoneFocus { get { return this.intozonefocus; } set { this.intozonefocus = value; } }

        private bool skipzonefocus;
        public bool SkipZoneFocus { get { return this.skipzonefocus; } set { this.skipzonefocus = value; } }

        #endregion

        #region Method

        public bool ContainLine(int _line) { return _line >= this.linestart && _line <= this.lineend; }

        #endregion

    }
}
