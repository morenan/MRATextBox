using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Morenan.MRATextBox.Core.Documents;

namespace Morenan.MRATextBox.Core
{
    internal class TextItem : ITextItem
    {
        public TextItem(TextBoxCore _core)
        {
            this.core = _core;
        }

        private bool isdisposed = false;
        public bool IsDisposed { get { return this.isdisposed; } }

        public void Dispose()
        {
            if (isdisposed) return;
            this.isdisposed = true;
            _Dispose();
        }

        protected virtual void _Dispose()
        {
            this.core = null;
            this.parent = null;
        }

        #region Number

        private TextBoxCore core;
        public ITextBoxCore Core { get { return this.core; } }

        private IDocsItem doc;
        public IDocsItem Doc { get { return this.doc; } set { this.doc = value; } }
        
        private ITextZone parent;
        public ITextZone Parent { get { return this.parent; } set { this.parent = value; } }

        private int id;
        public int ID { get { return this.id; } set { this.id = value; } }

        private int level;
        public virtual int Level { get { return this.level; } set { this.level = value; } }

        #endregion

        #region Method

        public virtual ITextItem Insert(int id, string text)
        {
            ITextItem result = new TextWord(core, ToString().Insert(id, text))
                { Parent = this.parent, ID = this.id, Level = this.level };
            if (core.WaitCaret == this) core.WaitCaret = result;
            return result;
        }

        public virtual ITextItem Remove(int id, int count)
        {
            return new TextWord(core, ToString().Remove(id, count))
                { Parent = this.parent, ID = this.id, Level = this.level };
        }
        
        public bool IsAncestorOf(ITextItem that)
        {
            if (that == null) return false;
            if (that.Level < this.level) return false;
            while (that.Level > this.level) that = that.Parent;
            return this == that;
        }

        public bool IsChildOf(ITextItem that)
        {
            if (that == null) return false;
            return that.IsAncestorOf(this);
             
        }

        #endregion
    }
}
