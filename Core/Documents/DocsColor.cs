using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Morenan.MRATextBox.Core.Documents
{
    internal class DocsColor : DocsItem, IDocsColor
    {
        public DocsColor(string _name, IDocsItem _baseon, IDocsItem _parent) : base(_name, _baseon, _parent)
        {
            this.foreground = null;
            this.background = null;
            this.fontweight = null;
        }

        #region Number

        private Color? foreground;
        public Color? Foreground
        {
            get { return this.foreground ?? (BaseOn is IDocsColor ? ((IDocsColor)(BaseOn)).Foreground : null); }
            set { this.foreground = value; }
        }

        private Color? background;
        public Color? Background
        {
            get { return this.background ?? (BaseOn is IDocsColor ? ((IDocsColor)(BaseOn)).Background : null); }
            set { this.background = value; }
        }

        private FontWeight? fontweight;
        public FontWeight? FontWeight
        {
            get { return this.fontweight ?? (BaseOn is IDocsColor ? ((IDocsColor)(BaseOn)).FontWeight : null); }
            set { this.fontweight = value; }
        }

        private FontFamily fontfamily;
        public FontFamily FontFamily
        {
            get { return this.fontfamily ?? (BaseOn is IDocsColor ? ((IDocsColor)(BaseOn)).FontFamily : null); }
            set { this.fontfamily = value; }
        }

        private FontStyle? fontstyle;
        public FontStyle? FontStyle
        {
            get { return this.fontstyle ?? (BaseOn is IDocsColor ? ((IDocsColor)(BaseOn)).FontStyle : null); }
            set { this.fontstyle = value; }
        }

        private FontStretch? fontstretch;
        public FontStretch? FontStretch
        {
            get { return this.fontstretch ?? (BaseOn is IDocsColor ? ((IDocsColor)(BaseOn)).FontStretch : null); }
            set { this.fontstretch = value; }

        }

        private double? fontsize;
        public double? FontSize
        {
            get { return this.fontsize ?? (BaseOn is IDocsColor ? ((IDocsColor)(BaseOn)).FontSize : null); }
            set { this.fontsize = value; }

        }

        #endregion
    }
}
