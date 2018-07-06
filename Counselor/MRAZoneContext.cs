using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Morenan.MRATextBox.Counselor
{
    internal class MRAZoneContext : IMRAZoneContext
    {
        public MRAZoneContext(string _name, IMRATextInputContext _inputcontext)
        {
            this.name = _name;
            this.inputcontext = _inputcontext;
        }

        private string name;
        public string Name { get { return this.name; } }

        private MRAZoneAction action;
        public MRAZoneAction Action { get { return this.action; } }

        private IMRATextInputContext inputcontext;
        public IMRATextInputContext InputContext { get { return this.inputcontext; } }
    }
    
    /// <summary> 对这个代码区域的操作 </summary>
    public enum MRAZoneAction
    {
        /// <summary> 在界面上收缩这个区域 </summary>
        Skip,
        /// <summary> 在界面上展开这个区域 </summary>
        Show
    }
}
