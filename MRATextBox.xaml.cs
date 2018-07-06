using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Morenan.MRATextBox.Core;
using Morenan.MRATextBox.View;
using Morenan.MRATextBox.Counselor;

namespace Morenan.MRATextBox
{
    /// <summary>
    /// MRATextBox.xaml 的交互逻辑
    /// </summary>
    public partial class MRATextBox : UserControl
    {
        public MRATextBox()
        {
            InitializeComponent();
            this.marginnumberbar = 0.0;
            this.marginstructbar = 60.0;
            this.marginleft = 80.0;
            Core = new TextBoxCore();
        }

        #region Property

        #region CodeDocumentSource

        protected static readonly DependencyProperty CodeDocumentSourceProperty = DependencyProperty.Register(
            "CodeDocumentSource", typeof(string), typeof(MRATextBox), new PropertyMetadata(null, OnPropertyChanged_CodeDocumentSource));

        /// <summary> 代码分析文档的uri路径（xchd文件）  </summary>
        public string CodeDocumentSource
        {
            get { return (string)GetValue(CodeDocumentSourceProperty); }
            set { SetValue(CodeDocumentSourceProperty, value); }
        }

        private static void OnPropertyChanged_CodeDocumentSource(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MRATextBox) ((MRATextBox)d).OnCodeDocumentSourceChanged(e);
        }

        protected virtual void OnCodeDocumentSourceChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is string) Core.LoadXmlCodeDocument(e.NewValue.ToString());
        }

        #endregion

        #region ConfigSource

        protected static readonly DependencyProperty ConfigSourceProperty = DependencyProperty.Register(
            "ConfigSource", typeof(string), typeof(MRATextBox), new PropertyMetadata(null, OnPropertyChanged_ConfigSource));

        /// <summary> 显示设置文件的uri路径（xchd文件） </summary>
        public string ConfigSource
        {
            get { return (string)GetValue(ConfigSourceProperty); }
            set { SetValue(ConfigSourceProperty, value); }
        }

        private static void OnPropertyChanged_ConfigSource(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MRATextBox) ((MRATextBox)d).OnConfigSourceChanged(e);
        }

        protected virtual void OnConfigSourceChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is string) Core.LoadXmlConfig(e.NewValue.ToString());
        }

        #endregion

        #region IsReadOnly

        protected static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(
            "IsReadOnly", typeof(bool), typeof(MRATextBox), new PropertyMetadata(false, OnPropertyChanged_IsReadOnly));

        /// <summary> 是否只读 </summary>
        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        private static void OnPropertyChanged_IsReadOnly(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MRATextBox) ((MRATextBox)d).OnIsReadOnlyChanged(e);
        }

        protected virtual void OnIsReadOnlyChanged(DependencyPropertyChangedEventArgs e)
        {

        }

        #endregion

        #region Counselor
        
        protected static readonly DependencyProperty CounselorProperty = DependencyProperty.Register(
            "Counselor", typeof(IMRATextBoxCounselor), typeof(MRATextBox), new PropertyMetadata(null, OnPropertyChanged_Counselor));

        /// <summary> 外部顾问 </summary>
        public IMRATextBoxCounselor Counselor
        {
            get { return (IMRATextBoxCounselor)GetValue(CounselorProperty); }
            set { SetValue(CounselorProperty, value); }
        }

        private static void OnPropertyChanged_Counselor(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MRATextBox) ((MRATextBox)d).OnCounselorChanged(e);
        }

        protected virtual void OnCounselorChanged(DependencyPropertyChangedEventArgs e)
        {

        }

        #endregion

        #region IsCltOpening
        
        protected static readonly DependencyProperty IsCltOpeningProperty = DependencyProperty.Register(
            "IsCltOpening", typeof(bool), typeof(MRATextBox), new PropertyMetadata(false, OnPropertyChanged_IsCltOpening));

        /// <summary> 正在打开并使用代码补全窗口 </summary>
        public bool IsCltOpening
        {
            get { return (bool)GetValue(IsCltOpeningProperty); }
            internal set { SetValue(IsCltOpeningProperty, value); }
        }

        private static void OnPropertyChanged_IsCltOpening(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MRATextBox) ((MRATextBox)d).OnIsCltOpeningChanged(e);
        }

        protected virtual void OnIsCltOpeningChanged(DependencyPropertyChangedEventArgs e)
        {
            if (IsCltOpening)
            {
                int line = Core.SelectedStart.Line;
                int column = Core.SelectedStart.Column;
                IMRATextItemInfo lineitem = UI_Main.GetItem(line);
                MRATextItemView lineview = lineitem?.View;
                if (lineview == null) return;
                #region Canvas Top
                {
                    Point p = lineview.TranslatePoint(new Point(0, 0), CV_Cover);
                    if (p.Y + lineview.ActualHeight + UI_CltBox.ActualHeight < ActualHeight)
                        Canvas.SetTop(UI_CltBox, p.Y + lineview.ActualHeight);
                    else
                        Canvas.SetTop(UI_CltBox, p.Y - UI_CltBox.ActualHeight);
                }
                #endregion
                #region Canvas Left
                {
                    Rect rect = lineview.GetColumnActualRect(column);
                    double x = rect.X;
                    if (UI_Main.UI_Stack != null) x -= UI_Main.UI_Stack.HorizontalOffset;
                    x = Math.Max(x, 0.0);
                    x = Math.Min(x, ActualWidth - UI_CltBox.ActualWidth);
                    Canvas.SetLeft(UI_CltBox, x);
                }
                #endregion
                IEnumerable<IMRACltItem> cltsrcs = Counselor.GetCltItems(Core.GetInputContext());
                UI_CltBox.SetCltSources(cltsrcs);
                ITextPosition pos = Core.SelectedStart.NextSeek();
                UI_CltBox.SetInputText(String.Empty);
                UI_CltBox.SetInputText(pos.Item.ToString());
                UI_CltBox.Visibility = Visibility.Visible;
            }
            else
            {
                UI_CltBox.Visibility = Visibility.Hidden;
            }
        }

        #endregion
        
        #endregion

        #region Number

        private TextBoxCore core;
        internal TextBoxCore Core
        {
            get
            {
                return this.core;
            }
            set
            {
                if (core == value) return;
                TextBoxCore _core = core;
                this.core = null;
                if (_core != null && _core.View != null) _core.View = null;
                this.core = value;
                if (core != null && core.View != this) core.View = this;
                UI_Main.Core = core;
                UI_CltBox.Core = core;
            }
        }
        
        private double marginnumberbar;
        internal double MarginNumberBar { get { core?.DictValue?.TryGetValue("margin_numberbar", out marginnumberbar); return marginnumberbar; } }

        private double marginstructbar;
        internal double MarginStructBar { get { core?.DictValue?.TryGetValue("margin_structbar", out marginstructbar); return marginstructbar; } }

        private double marginleft;
        internal double MarginLeft { get { core?.DictValue?.TryGetValue("margin_left", out marginleft); return marginleft; } }
        
        #endregion

        #region Method

        /// <summary> 获取整个文本 </summary>
        /// <returns></returns>
        public string GetText()
        {
            return Core.GetAllText();
        }
        
        /// <summary> 设置文本 </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public void SetText(string text)
        {
            Core.Replace(Core.GetFirstPosition(), Core.GetLastPosition(), text);
            UI_Main.InvalidateItems();
        }

        /// <summary> 读取文件 </summary>
        /// <param name="filepath">文件路径</param>
        public void Load(string filepath)
        {
            FileStream fs = File.OpenRead(filepath);
            StreamReader sr = new StreamReader(fs);
            string text = sr.ReadToEnd();
            sr.Close();
            SetText(text);
        }

        /// <summary> 保存文件 </summary>
        /// <param name="filepath">文件路径</param>
        public void Save(string filepath)
        {
            FileStream fs = File.OpenWrite(filepath);
            StreamWriter sw = new StreamWriter(fs);
            string text = GetText();
            sw.Write(text);
            sw.Close();
        }

        #region Clt (Completation)
        
        internal void CltOpen(string inputtext)
        {
            if (IsCltOpening) return;
            if (Counselor == null) return;
            if (!Counselor.CanCltOpen(Core.GetInputContext(), inputtext)) return;
            IsCltOpening = true;
            
        }

        internal void CltClose(string inputtext)
        {
            if (!IsCltOpening) return;
            if (Counselor == null) return;
            if (!Counselor.CanCltClose(Core.GetInputContext(), inputtext)) return;
            IsCltOpening = false;
        }

        internal void CltCloseByBackspace(string backspacetext)
        {
            if (!IsCltOpening) return;
            if (Counselor == null) return;
            if (!Counselor.CanCltCloseByBackspace(Core.GetInputContext(), backspacetext)) return;
            IsCltOpening = false;
        }

        internal void CltUpdate()
        {
            if (!IsCltOpening) return;
            ITextPosition pos = Core.SelectedStart.NextSeek();
            UI_CltBox.SetInputText(pos.Item.ToString());
        }

        #endregion

        #endregion

        #region Event Handler

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {

        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        #endregion
    }
}
