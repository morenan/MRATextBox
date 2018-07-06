using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Morenan.MRATextBox.Counselor
{
    internal class MRADocMatchResult : IMRADocMatchResult
    {
        public MRADocMatchResult(IMRATextItem _result, int _start, int _count)
        {
            this.result = _result;
            this.start = _start;
            this.count = _count;
        }

        private IMRATextItem result;
        public IMRATextItem Result { get { return this.result; } }

        private int start;
        public int Start { get { return this.start; } }

        private int count;
        public int Count { get { return this.count; } }
    }
}
