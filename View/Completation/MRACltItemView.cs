using System;
using System.Threading;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using Morenan.MRATextBox.Core;

namespace Morenan.MRATextBox.View.Completation
{
    internal class MRACltItemView : UserControl
    {
        #region Resource

        private static readonly Rect Rect_Icon = new Rect(2, 2, 16, 16);
        private static readonly Point Point_Text = new Point(20, 2);
        private static readonly Point Point_BgStart = new Point(20, 0);
        private static readonly ImageSource Icon_Keyword = new BitmapImage(
            new Uri("pack://application:,,,/Morenan.MRATextBox;component/Resources/Icon/White_Keyword.png"));
        private static readonly ImageSource Icon_Class = new BitmapImage(
            new Uri("pack://application:,,,/Morenan.MRATextBox;component/Resources/Icon/White_Class.png"));
        private static readonly ImageSource Icon_Variable = new BitmapImage(
            new Uri("pack://application:,,,/Morenan.MRATextBox;component/Resources/Icon/White_Variable.png"));
        private static readonly ImageSource Icon_Function = new BitmapImage(
            new Uri("pack://application:,,,/Morenan.MRATextBox;component/Resources/Icon/White_Function.png"));

        #endregion

        public MRACltItemView()
        {

        }

        #region Number

        public MRACltItemInfo Info { get { return DataContext is MRACltItemInfo ? (MRACltItemInfo)DataContext : null; } }
        
        private Brush background_normal;
        private Brush background_selected;
        private Brush foreground_normal;
        private Brush foreground_selected;

        #endregion

        #region Method

        protected void Initialize()
        {
            if (Info?.Core != null)
            {
                ITextBoxCore core = Info.Core;
                core.DictBrush.TryGetValue("background_cltbox", out background_normal);
                core.DictBrush.TryGetValue("background_cltbox_selected", out background_selected);
                core.DictBrush.TryGetValue("foreground_cltbox", out foreground_normal);
                core.DictBrush.TryGetValue("foreground_cltbox_selected", out foreground_selected);
                Background = background_normal;
                Foreground = Info.IsSelected ? foreground_selected : foreground_normal;
            }
        }

        #endregion

        #region Event Handler
        
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == DataContextProperty)
            {
                if (e.OldValue is MRACltItemInfo)
                {
                    MRACltItemInfo oldinfo = (MRACltItemInfo)(e.OldValue);
                    oldinfo.PropertyChanged -= OnInfoPropertyChanged;
                }
                if (e.NewValue is MRACltItemInfo)
                {
                    MRACltItemInfo newinfo = (MRACltItemInfo)(e.NewValue);
                    newinfo.PropertyChanged += OnInfoPropertyChanged;
                }
                Initialize();
                InvalidateVisual();
            }
        }

        private void OnInfoPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "IsSelected":
                    //Background = Info.IsSelected ? background_selected : background_normal;
                    Foreground = Info.IsSelected ? foreground_selected : foreground_normal;
                    InvalidateVisual();
                    break;
            }
        }

        protected override void OnRender(DrawingContext ctx)
        {
            base.OnRender(ctx);
            if (Info == null) return;
            #region Background
            if (Info.IsSelected)
            {
                MRACltBox cltbox = Info.Core.View.UI_CltBox;
                Rect bgrect = new Rect(Point_BgStart, new Point(Math.Max(cltbox.ActualWidth, ActualWidth), ActualHeight));
                ctx.DrawRectangle(background_selected, null, bgrect);
            }
            #endregion
            #region Icon
            {
                ImageSource icon = null;
                switch (Info.Type)
                {
                    case MRACltItemTypes.Keyword: icon = Icon_Keyword; break;
                    case MRACltItemTypes.Class: icon = Icon_Class; break;
                    case MRACltItemTypes.Variable: icon = Icon_Variable; break;
                    case MRACltItemTypes.Function: icon = Icon_Function; break;
                }
                ctx.DrawImage(icon, Rect_Icon);
            }
            #endregion
            #region Text
            {
                ITextBoxCore core = Info.Core;
                Typeface typeface = new Typeface(core.FontFamily, core.FontStyle, core.FontWeight, core.FontStretch);
                FormattedText fmttext = new FormattedText(Info.Text, Thread.CurrentThread.CurrentUICulture, FlowDirection.LeftToRight, typeface, 12, Foreground);
                foreach (MRACltBoldSegment bold in Info.Bolds)
                    fmttext.SetFontWeight(FontWeights.Bold, bold.Start, bold.Count);
                ctx.DrawText(fmttext, Point_Text);
            }
            #endregion
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            Info?.Parent?.SelectSet(Info?.ID ?? -1);
        }

        #endregion
    }
}
