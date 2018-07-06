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
    }

    internal class MRAZoneSkipInfo : IMRAZoneSkipInfo
    {
        public MRAZoneSkipInfo(ITextBoxCore _core, int _id, int _linestart, int _lineend, ITextZone _skipzone)
        {
            this.core = _core;
            this.linestart = _linestart;
            this.lineend = _lineend;
            this.skipzone = _skipzone;
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
        
        #endregion
        
    }
}
