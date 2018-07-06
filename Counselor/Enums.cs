using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Morenan.MRATextBox.Counselor
{
    public enum MRATextItemTypes
    {
        None = 0x00,
        InputContext = 0x10,
        InputContextSingle = 0x11,
        InputContextZone = 0x12,
        MatchResult = 0x20,
        MatchResultSingle = 0x21,
        MatchResultZone = 0x22
    }

    public enum MRATextItemFeatures
    {
        None = 0x00000000,
    }

    public enum MRACltItemTypes
    {
        None = 0x00,
        Type = 0x01,
        Var = 0x02,
        Func = 0x03,
        Params = 0x04
    }

    public enum MRACltItemFeatures
    {
        None = 0x00000000,
    }
}
