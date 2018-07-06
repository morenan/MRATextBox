using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Morenan.MRATextBox.Core
{
    internal class TextFontCore : ITextFontCore
    {
        public TextFontCore(FontStyle _fontstyle, FontStretch _fontstretch, FontFamily _fontfamily, FontWeight _fontweight, double _fontsize, Brush _background, Brush _foreground)
        {
            this.fontstyle = _fontstyle;
            this.fontstretch = _fontstretch;
            this.fontfamily = _fontfamily;
            this.fontweight = _fontweight;
            this.fontsize = _fontsize;
            this.background = _background;
            this.foreground = _foreground;
        }

        public TextFontCore(ITextFontCore that)
        {
            this.fontsize = that.FontSize;
            this.fontstretch = that.FontStretch;
            this.fontfamily = that.FontFamily;
            this.fontweight = that.FontWeight;
            this.fontsize = that.FontSize;
            this.background = that.Background;
            this.foreground = that.Foreground;
        }

        private FontStyle fontstyle;
        public FontStyle FontStyle { get { return this.fontstyle; } }

        private FontStretch fontstretch;
        public FontStretch FontStretch { get { return this.fontstretch; } }

        private FontFamily fontfamily;
        public FontFamily FontFamily { get { return this.fontfamily; } }

        private FontWeight fontweight;
        public FontWeight FontWeight { get { return this.fontweight; } }

        private double fontsize;
        public double FontSize { get { return this.fontsize; } }

        private Brush foreground;
        public Brush Foreground { get { return this.foreground; } }

        private Brush background;
        public Brush Background { get { return this.background; } }

        public ITextFontCore Clone()
        {
            return new TextFontCore(this);
        }
    }
}
