using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Morenan.MRATextBox.Core;

namespace Morenan.MRATextBox.View.Completation
{
    internal class MRACltItemInfo : INotifyPropertyChanged
    {
        public MRACltItemInfo(ITextBoxCore _core, MRACltBox _parent, MRACltItemTypes _type, int _id, string _text)
        {
            this.core = _core;
            this.parent = _parent;
            this.type = _type;
            this.id = _id;
            this.text = _text;
            this.bolds = new List<MRACltBoldSegment>();
        }

        #region Number

        private ITextBoxCore core;
        public ITextBoxCore Core { get { return this.core; } }

        private MRACltBox parent;
        public MRACltBox Parent { get { return this.parent; } }

        private MRACltItemTypes type;
        public MRACltItemTypes Type { get { return this.type; } }

        private int id;
        public int ID { get { return this.id; } }

        private string text;
        public string Text { get { return this.text; } }

        private List<MRACltBoldSegment> bolds;
        public List<MRACltBoldSegment> Bolds { get { return this.bolds; } }

        private bool isselected;
        public bool IsSelected
        {
            get { return this.isselected; }
            set { this.isselected = value; InvokePropertyChanged("IsSelected"); }
        }

        #endregion

        #region Event Handler

        public event PropertyChangedEventHandler PropertyChanged;

        protected void InvokePropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }

        #endregion
    }

    internal struct MRACltBoldSegment
    {
        public int Start { get; set; }
        public int Count { get; set; }
    }

    internal enum MRACltItemTypes
    {
        Keyword,
        Class,
        Variable,
        Function,
    }
}
