using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using Morenan.MRATextBox.Core;

namespace Morenan.MRATextBox.View
{
    internal class MRATextProperties : TextRunProperties
    {
        private ITextFontCore core;

        public MRATextProperties(ITextFontCore _core)
        {
            this.core = _core;
        }
         
        public override Typeface Typeface { get { return new Typeface(core.FontFamily, core.FontStyle, core.FontWeight, core.FontStretch); } }

        public override double FontRenderingEmSize { get { return core.FontSize; } }

        public override double FontHintingEmSize { get { return core.FontSize; } }

        public override Brush BackgroundBrush { get { return core.Background; } }

        public override Brush ForegroundBrush { get { return core.Foreground; } }

        public override BaselineAlignment BaselineAlignment { get { return BaselineAlignment.TextBottom; } }

        public override CultureInfo CultureInfo { get { return Thread.CurrentThread.CurrentUICulture; } }

        public override TextDecorationCollection TextDecorations { get { return new TextDecorationCollection(); } }

        public override TextEffectCollection TextEffects { get { return new TextEffectCollection(); } }
    }
}
