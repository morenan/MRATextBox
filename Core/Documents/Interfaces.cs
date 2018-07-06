using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Morenan.MRATextBox.Core.Documents
{
    internal interface IDocsCore
    {
        string Name { get; }
        string[] Extensions { get; }
        IDictionary<string, IDocsItem> Dict { get; }
        IDictionary<string, IDocsItem> DictSys { get; }
        IDocsItem GetItem(string name);
    }
    
    internal interface IDocsItem
    {
        string Name { get; set; }
        IDocsItem BaseOn { get; set; }
        IDocsItem Parent { get; set; }
    }

    internal interface IDocsCollection : IDocsItem
    {
        IList<IDocsItem> Items { get; set; }
    }

    internal interface IDocsColor : IDocsItem
    {
        Color? Foreground { get; set; }
        Color? Background { get; set; }
        FontWeight? FontWeight { get; set; }
        FontStyle? FontStyle { get; set; }
        FontStretch? FontStretch { get; set; }
        double? FontSize { get; set; }
        FontFamily FontFamily { get; set; }
    }

    internal interface IDocsFill : IDocsItem
    {
        IDocsColor Fill { get; set; }
    }

    internal interface IDocsZone : IDocsFill, IDocsCollection
    {
        IDocsKeyWord Left { get; set; }
        IDocsKeyWord Right { get; set; }
    }

    internal interface IDocsLine : IDocsFill, IDocsCollection
    {
        IDocsKeyWord Left { get; set; }
    }

    internal interface IDocsKeyWord : IDocsFill
    {
        ITextKeyCore Key { get; set; }
    }

    internal interface IDocsKeyWordCollection : IDocsItem
    {
        IList<IDocsKeyWord> KeyWords { get; set; }
    }

    internal interface IDocsWord : IDocsFill
    {
        Regex Regex { get; set; }
    }
    
    internal interface IDocsGroup : IDocsCollection
    {
    }

    internal interface IDocsCycle : IDocsCollection
    {
        int IgnoreStart { get; set; }
        int IgnoreEnd { get; set; }
    }

    internal interface IDocsCompletation : IDocsItem
    {

    }
}
