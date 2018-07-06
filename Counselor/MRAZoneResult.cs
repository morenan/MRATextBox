using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Morenan.MRATextBox.Counselor
{
    public class MRAZoneResult : IMRAZoneResult
    {
        public MRAZoneResult(bool _cancel)
        {
            this.cancel = _cancel;
        }

        private bool cancel;
        public bool Cancel { get { return this.cancel; } }
    }

    public class MRAZoneSkipResult : MRAZoneResult, IMRAZoneSkipResult
    {
        public MRAZoneSkipResult(int _showstart = 0, int _showend = 0) : base(false)
        {
            this.showstart = _showstart;
            this.showend = _showend;
        }

        private int showstart;
        public int ShowStart { get { return this.showstart; } }

        private int showend;
        public int ShowEnd { get { return this.showend; } }
    }
}
