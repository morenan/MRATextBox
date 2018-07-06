using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Morenan.MRATextBox.Core;
using Morenan.MRATextBox.Core.Documents;

namespace Morenan.MRATextBox.Counselor
{
    internal abstract class MRADocMatchItem : IMRATextItem
    {
        protected MRADocMatchItem(string _name, ITextItem _item)
        {
            this.name = _name;
            this.item = _item;
        }

        private ITextItem item;
        public ITextItem Item { get { return this.item; } }
        public override string ToString() { return item?.ToString() ?? String.Empty; }

        private string name;
        public string Name { get { return name; } }

        private IMRATextItem parent;
        public IMRATextItem Parent { get { return this.parent; } set { this.parent = value; } }

        public MRATextItemTypes ItemType { get { return item is ITextZone ? MRATextItemTypes.MatchResultZone : MRATextItemTypes.MatchResultSingle; } }

        public MRATextItemFeatures Feature { get { return MRATextItemFeatures.None; } }
    }

    internal class MRADocMatchWord : MRADocMatchItem, IMRATextWord
    {
        public MRADocMatchWord(string _name, ITextItem _item) : base(_name, _item) { }
    }

    internal class MRADocMatchCollection : MRADocMatchItem, IMRATextCollection
    {
        public MRADocMatchCollection(string _name, ITextItem _item) : base(_name, _item)
        {
            this.items = new List<MRADocMatchItem>();
        }

        private List<MRADocMatchItem> items;
        public List<MRADocMatchItem> Items { get { return this.items; } }
        public IMRATextItem this[int index] { get { return items[index]; } }
        public int Count { get { return items.Count; } }

        public IEnumerator<IMRATextItem> GetEnumerator()
        {
            foreach (MRADocMatchItem item in items)
                yield return item;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
