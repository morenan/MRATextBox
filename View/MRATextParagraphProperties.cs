using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.TextFormatting;
using Morenan.MRATextBox.Core;

namespace Morenan.MRATextBox.View
{
    internal class MRATextParagraphProperties : TextParagraphProperties
    {
        private ITextBoxCore core;
        private MRATextSource textsource;

        public MRATextParagraphProperties(ITextBoxCore _core, MRATextSource _textsource)
        {
            this.core = _core;
            this.textsource = _textsource;
        }

        public override FlowDirection FlowDirection { get { return FlowDirection.LeftToRight; } }

        public override TextAlignment TextAlignment { get { return TextAlignment.Left; } }

        public override double LineHeight { get { return core.FontSize + 2.0; } }

        public override bool FirstLineInParagraph { get { return false; } }

        public override TextRunProperties DefaultTextRunProperties { get { return new MRATextProperties(new TextFontCore(core)); } }

        public override TextWrapping TextWrapping { get { return TextWrapping.NoWrap; } }

        public override TextMarkerProperties TextMarkerProperties { get { return new MRATextMarkerProperties(textsource); } }

        public override double Indent { get { return 0.0; } }
        

    }

    internal class MRATextMarkerProperties : TextMarkerProperties
    {
        private MRATextSource textsource;

        public MRATextMarkerProperties(MRATextSource _textsource)
        {
            this.textsource = _textsource;
        }

        public override double Offset { get { return 0.0; } }

        public override TextSource TextSource { get { return this.textsource; } }
    }
}
