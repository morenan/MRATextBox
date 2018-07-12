using System;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Runtime.InteropServices;
using System.Reflection;
using System.IO;
using Morenan.MRATextBox.Core.Documents;
using Morenan.MRATextBox.Core.UndoRedo;
using Morenan.MRATextBox.Counselor;

namespace Morenan.MRATextBox.Core
{
    using Class_DocsCore = Morenan.MRATextBox.Core.Documents.DocsCore;

    /// <summary> MRATextBox的内核实现 </summary>
    internal class TextBoxCore : ITextBoxCore, ITextZone
    {
        #region Resource

        private static readonly Regex Regex_NonTrim = new Regex(@"[^\s]\s+$", RegexOptions.Compiled);

        #endregion

        /// <summary> 构造函数 </summary>
        public TextBoxCore()
        {
            this.items = new List<ITextItem>() { new TextTrim(this, String.Empty) { Parent = this, Level = 0 } };
            this.dictkey = new Dictionary<string, ITextKeyCore>();
            this.dictonekey = new Dictionary<char, ITextKeyCore>();
            this.dictbrush = new Dictionary<string, Brush>();
            this.dictvalue = new Dictionary<string, double>();
            this.docscore = null;
            this.undocore = new TextUndoRedoCore(this);
            this.linecount = 1;
            this.selectedstart = new TextPosition() { Item = items[0], ItemIndex = 0, Line = 1, Column = 1 };
            this.selectedend = new TextPosition() { Item = items[0], ItemIndex = 0, Line = 1, Column = 1 };
            this.defaultfontfamily = new FontFamily("Consolas");
            this.defaultfontsize = 12;
            this.defaultbackground = Brushes.White;
            this.defaultforeground = Brushes.Black;
            this.defaultfontstretch = FontStretches.Normal;
            this.defaultfontstyle = FontStyles.Normal;
            this.defaultfontweight = FontWeights.Normal;
            this.tabbyteperiod = 4;
            // 加载默认文档
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream stream = assembly.GetManifestResourceStream("Morenan.MRATextBox.Resources.MRA_CPP.xchd");
            LoadXmlCodeDocument(stream);
            stream.Close();
            //LoadXmlCodeDocument("pack://application:,,,/Morenan.MRATextBox;component/Resources/MRA_CPP.xaml");
            stream = assembly.GetManifestResourceStream("Morenan.MRATextBox.Resources.MRA_Config.xchd");
            LoadXmlConfig(stream);
            stream.Close();
            //LoadXmlConfig("pack://application:,,,/Morenan.MRATextBox;component/Resources/MRA_Config.xaml");
        }

        /// <summary> 是否已被释放 </summary>
        private bool isdisposed = false;
        /// <summary> 是否已被释放 </summary>
        public bool IsDisposed { get { return this.isdisposed; } }

        /// <summary> 释放函数 </summary>
        public void Dispose()
        {
            if (isdisposed) return;
            this.isdisposed = true;
            this.items = null;
            this.dictkey = null;
            this.dictonekey = null;
            this.docscore = null;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (ITextItem item in items)
                sb.Append(item.ToString());
            return sb.ToString();
        }

        #region Number

        /// <summary> 对ITextZone的接口，不使用 </summary>
        public ITextBoxCore Core { get { return this; } }
        /// <summary> 对ITextZone的接口，不使用 </summary>
        public IDocsItem Doc { get { return null; } set { } }
        /// <summary> 对ITextZone的接口，不使用 </summary>
        public ITextZone Parent { get { return null; } set { } }
        /// <summary> 对ITextZone的接口，不使用 </summary>
        public int ID { get { return -1; } set { } }
        /// <summary> 对ITextZone的接口，不使用 </summary>
        public int Level { get { return -1; } set { } }

        /// <summary> 作为容器包含的子项，同时也是对ITextZone的接口 </summary>
        private List<ITextItem> items;
        /// <summary> 作为容器包含的子项，同时也是对ITextZone的接口 </summary>
        public IList<ITextItem> Items { get { return this.items; } }

        /// <summary> 文档核，包含所编写代码相关的信息 </summary>
        private IDocsCore docscore;
        /// <summary> 文档核，包含所编写代码相关的信息 </summary>
        public IDocsCore DocsCore { get { return this.docscore; } }

        /// <summary> 撤销栈的核，用于管理撤销和恢复 </summary>
        private ITextUndoRedoCore undocore;
        /// <summary> 撤销栈的核，用于管理撤销和恢复 </summary>
        public ITextUndoRedoCore UndoCore { get { return this.undocore; } }

        /// <summary> 作为字典查找关键词 </summary>
        private Dictionary<string, ITextKeyCore> dictkey;
        /// <summary> 作为字典查找关键词 </summary>
        public IDictionary<string, ITextKeyCore> DictKey { get { return this.dictkey; } }

        /// <summary> 作为字典查找关键字符 </summary>
        private Dictionary<char, ITextKeyCore> dictonekey;
        /// <summary> 作为字典查找关键字符 </summary>
        public IDictionary<char, ITextKeyCore> DictOneKey { get { return this.dictonekey; } }

        /// <summary> 作为字典查找颜色刷 </summary>
        private Dictionary<string, Brush> dictbrush;
        /// <summary> 作为字典查找颜色刷 </summary>
        public IDictionary<string, Brush> DictBrush { get { return this.dictbrush; } }

        /// <summary> 作为字典查找实数值 </summary>
        private Dictionary<string, double> dictvalue;
        /// <summary> 作为字典查找实数值 </summary>
        public IDictionary<string, double> DictValue { get { return this.dictvalue; } }
        
        /// <summary> 行的总数 </summary>
        private int linecount;
        /// <summary> 行的总数 </summary>
        public int LineCount { get { return this.linecount; } set { this.linecount = value; } }

        /// <summary> 省略区域的总数 </summary>
        private int skipcount;
        /// <summary> 省略区域的总数 </summary>
        public int SkipCount { get { return this.skipcount; } set { this.skipcount = value; } }
        
        /// <summary> 是否省略，本是ITextZone的特性，这里不能省略 </summary>
        public bool IsSkip { get { return false; } set { } }

        /// <summary> 在此内核上加载的显示控件 </summary>
        private MRATextBox view;
        /// <summary> 在此内核上加载的显示控件 </summary>
        public MRATextBox View
        {
            get
            {
                return this.view;
            }
            set
            {
                if (view == value) return;
                MRATextBox _view = view;
                this.view = null;
                if (_view != null && _view.Core != null) _view.Core = null;
                this.view = value;
                if (view != null && view.Core != this) view.Core = this;
            }
        }

        /// <summary> 未加载控件之前，默认使用的FontStyle属性 </summary>
        private FontStyle defaultfontstyle;
        /// <summary> 字体风格（斜体/下划线) </summary>
        public FontStyle FontStyle { get { return view?.FontStyle ?? defaultfontstyle; } }

        /// <summary> 未加载控件之前，默认使用的FontStretch属性 </summary>
        private FontStretch defaultfontstretch;
        /// <summary> 字体的横纵比 </summary>
        public FontStretch FontStretch { get { return view?.FontStretch ?? defaultfontstretch; } }

        /// <summary> 未加载控件之前，默认使用的FontFamily属性 </summary>
        private FontFamily defaultfontfamily;
        /// <summary> 字体所属的字体集 </summary>
        public FontFamily FontFamily { get { return view?.FontFamily ?? defaultfontfamily; } }

        /// <summary> 未加载控件之前，默认使用的FontWeight属性 </summary>
        private FontWeight defaultfontweight;
        /// <summary> 字体线条的粗细 </summary>
        public FontWeight FontWeight { get { return view?.FontWeight ?? defaultfontweight; } }

        /// <summary> 未加载控件之前，默认使用的FontSize属性 </summary>
        private double defaultfontsize;
        /// <summary> 字体的大小 </summary>
        public double FontSize { get { return view?.FontSize ?? defaultfontsize; } }

        /// <summary> 未加载控件之前，默认使用的Foreground属性 </summary>
        private Brush defaultforeground;
        /// <summary> 字体的颜色 </summary>
        public Brush Foreground { get { return view?.Foreground ?? defaultforeground; } }

        /// <summary> 未加载控件之前，默认使用的Background属性 </summary>
        private Brush defaultbackground;
        /// <summary> 背景的颜色 </summary>
        public Brush Background { get { return view?.Background ?? defaultbackground; } }
        
        /// <summary> 用户选择内容的开始 </summary>
        private ITextPosition selectedstart;
        /// <summary> 用户选择内容的开始 </summary>
        public ITextPosition SelectedStart { get { return this.selectedstart; } set { this.selectedstart = value; } }

        /// <summary> 用户选择内容的结束 </summary>
        private ITextPosition selectedend;
        /// <summary> 用户选择内容的结束 </summary>
        public ITextPosition SelectedEnd { get { return this.selectedend; } set { this.selectedend = value; } }
        
        #endregion

        #region Method

        /// <summary> 作为ITextFontCore的接口，克隆一个副本 </summary>
        /// <returns>新的副本</returns>
        ITextFontCore ITextFontCore.Clone()
        {
            return new TextFontCore(this);
        }

        /// <summary> 是否是它的上级？ </summary>
        /// <param name="that">它</param>
        /// <returns>是</returns>
        public bool IsAncestorOf(ITextItem that)
        {
            return that?.Core == this;
        }

        /// <summary> 是否是它的下级？ </summary>
        /// <param name="that">它</param>
        /// <returns>是</returns>
        public bool IsChildOf(ITextItem that)
        {
            return false;
        }

        /// <summary> ITextItem的接口，未使用 </summary>
        /// <param name="id"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public ITextItem Insert(int id, string text)
        {
            return null;
        }

        /// <summary> ITextItem的接口，未使用 </summary>
        /// <param name="id"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public ITextItem Remove(int id, int count)
        {
            return null;
        }

        /// <summary> 往后加入一个新项，同时也作为ITextZone的接口 </summary>
        /// <param name="item">新项</param>
        public void Add(ITextItem item)
        {
            item.Parent = this;
            item.ID = items.Count();
            item.Level = Level + 1;
            items.Add(item);
            if (item is ITextTrim) linecount += ((ITextTrim)item).GetEnterCount();
            if (item is ITextZone) linecount += ((ITextZone)item).LineCount - 1;
        }

        /// <summary> 往指定索引插入一个新项，同时也作为ITextZone的接口 </summary>
        /// <param name="id">索引</param>
        /// <param name="item">新项</param>
        public void Insert(int id, ITextItem item)
        {
            item.Parent = this;
            item.ID = id;
            item.Level = Level + 1;
            for (int i = id; i < items.Count(); i++)
                items[i].ID++;
            items.Insert(id, item);
            if (item is ITextTrim) linecount += ((ITextTrim)item).GetEnterCount();
            if (item is ITextZone) linecount += ((ITextZone)item).LineCount - 1;
        }

        /// <summary> 移除一个项，同时也作为ITextZone的接口 </summary>
        /// <param name="item"></param>
        public void Remove(ITextItem item)
        {
            for (int i = item.ID + 1; i < items.Count(); i++)
                items[i].ID--;
            items.Remove(item);
            item.Parent = null;
            item.ID = -1;
            item.Level = -1;
            if (item is ITextTrim) linecount -= ((ITextTrim)item).GetEnterCount();
            if (item is ITextZone) linecount -= ((ITextZone)item).LineCount - 1;
        }

        /// <summary> 替换一段为新的项集，同时也作为ITextZone的接口 </summary>
        /// <param name="start">要替换的起始位置</param>
        /// <param name="count">要替换的长度</param>
        /// <param name="_additems">要替换的新的项集</param>
        public void Replace(int start, int count, IEnumerable<ITextItem> _additems)
        {
            items.RemoveRange(start, count);
            items.InsertRange(start, _additems);
            this.linecount = 1;
            for (int i = 0; i < items.Count(); i++)
            {
                items[i].Parent = this;
                items[i].ID = i;
                items[i].Level = Level + 1;
                if (items[i] is ITextTrim) linecount += ((ITextTrim)items[i]).GetEnterCount();
                if (items[i] is ITextZone) linecount += ((ITextZone)items[i]).LineCount - 1;
            }
        }

        /// <summary> 获取一段的项集，同时也作为ITextZone的接口 </summary>
        /// <param name="start">要获取的起始位置</param>
        /// <param name="count">要获取的长度</param>
        /// <returns>获取到的项集</returns>
        public IEnumerable<ITextItem> GetRange(int start, int count)
        {
            for (int i = start; i < start + count; i++)
                yield return items[i];
        }

        /// <summary> 将两个文本坐标位置包含的部分，替换为新的文本 </summary>
        /// <param name="start">开始的坐标</param>
        /// <param name="end">结束的坐标</param>
        /// <param name="text">要替换的新的文本</param>
        public void Replace(ITextPosition start, ITextPosition end, string text)
        {
            start = start.PrevSeek();
            end = end.NextSeek();
            if (start.CompareTo(end) == 0 && text.Equals("\n")
             && !(end.Item.Parent is ITextLine) && !(end.Item.Parent is ITextBoxCore) && end.Item.Parent.LineCount == 1)
            {
                int startline = start.Line;
                IText1to3LineAction action1 = To3Line(end.Item.Parent, end);
                undocore.Add(action1);
                ITextIndentAction action2 = AutoIntent(action1.Zone, 0, action1.Zone.Items.Count);
                undocore.Add(action2);
                return;
            }
            {
                // 旧的选择范围，删除和替换的文本
                int oldlinestart = start.Line;
                int oldcolumnstart = start.Column;
                int oldlineend = end.Line;
                int oldcolumnend = end.Column;
                string removedtext = GetText(start, end);
                string replacedtext = text;
                // 进行处理
                bool _oldintentenable = intentenable;
                intentenable = !text.Equals(" ");
                replacedtext = _Replace(start, end, text);
                intentenable = _oldintentenable;
                // 处理过后新的选择范围
                int newlinestart = selectedstart.Line;
                int newcolumnstart = selectedstart.Column;
                int newlineend = selectedend.Line;
                int newcolumnend = selectedend.Column;
                // 建立动作
                ITextUndoRedoAction action = new TextUndoRedoAction()
                {
                    OldLineStart = oldlinestart,
                    OldLineEnd = oldlineend,
                    OldColumnStart = oldcolumnstart,
                    OldColumnEnd = oldcolumnend,
                    NewLineStart = newlinestart,
                    NewLineEnd = newlineend,
                    NewColumnStart = newcolumnstart,
                    NewColumnEnd = newcolumnend,
                    RemovedText = removedtext,
                    ReplacedText = replacedtext
                };
                undocore.Add(action);
            }
            
        }

        /// <summary> 执行count次退格 </summary>
        /// <param name="count">退格次数</param>
        /// <returns>退格掉的文本</returns>
        public string Backspace(int count = 1)
        {
            ITextPosition start = selectedstart;
            ITextPosition end = selectedend;
            int poscmp = start.CompareTo(end);
            if (poscmp > 0) return String.Empty;
            if (poscmp < 0) { string deletetext = GetText(start, end); Delete(); return deletetext; }
            for (int i = 0; i < count; i++) start = start?.Prev();
            if (start == null) return String.Empty;

            // 旧的选择范围
            int oldlinestart = start.Line;
            int oldcolumnstart = start.Column;
            int oldlineend = end.Line;
            int oldcolumnend = end.Column;
            string backspacetext = GetText(start, end);
            // 进行处理
            bool _oldintentenable = intentenable;
            intentenable = false;
            _Replace(start, end, String.Empty);
            intentenable = _oldintentenable;
            // 处理过后新的选择范围
            int newlinestart = selectedstart.Line;
            int newcolumnstart = selectedstart.Column;
            int newlineend = selectedend.Line;
            int newcolumnend = selectedend.Column;
            // 建立动作
            ITextUndoRedoAction action = new TextUndoRedoAction()
            {
                OldLineStart = oldlinestart, OldLineEnd = oldlineend, OldColumnStart = oldcolumnstart, OldColumnEnd = oldcolumnend,
                NewLineStart = newlinestart, NewLineEnd = newlineend, NewColumnStart = newcolumnstart, NewColumnEnd = newcolumnend,
                Backspace = true, BackspaceText = backspacetext
            };
            undocore.Add(action);
            return backspacetext;
        }

        /// <summary> 智能退格 </summary>
        /// <returns>退格掉的文本</returns>
        public string SmartBackspace()
        {
            ITextPosition start = selectedstart;
            ITextPosition end = selectedend;
            int poscmp = start.CompareTo(end);
            if (poscmp != 0) return Backspace();
            int count = 0;
            while (start.Item is ITextTrim)
            {
                if (start.Line == 0 && start.Column == 0)
                    return String.Empty;
                if (start.LeftPart.Length == 0)
                {
                    start = start.PrevSeek();
                    continue;
                }
                int enterid = start.LeftPart.LastIndexOf('\n');
                if (enterid >= 0)
                {
                    count += start.LeftPart.Length - enterid - 1;
                    break;
                }
                else
                {
                    count += start.LeftPart.Length;
                    start = start.PrevItem()?.PrevSeek();
                }
            }
            count = Math.Max(count, 1);
            return Backspace(count);
        }
        
        /// <summary> 获取顾问系统的输入上下文 </summary>
        /// <returns></returns>
        public IMRATextInputContext GetInputContext()
        {
            return new MRATextInputContext(selectedstart.Item.Parent, selectedstart.Item.ID, selectedstart.ItemIndex);
        }

        #region Replace

        /// <summary> 将范围内的文本部分替换，边界由两个坐标start和end决定 </summary>
        /// <param name="start">边界起始</param>
        /// <param name="end">边界终止</param>
        /// <param name="text">替换文本</param>
        /// <returns>实际替换成的文本</returns>
        protected string _Replace(ITextPosition start, ITextPosition end, string text)
        {
            start = start.PrevSeek();
            end = end.NextSeek();
            // 查找公共父节点
            ITextItem starti = start.Item;
            ITextItem endi = end.Item;
            while (starti.Level > endi.Level)
                starti = starti.Parent;
            while (starti.Level < endi.Level)
                endi = endi.Parent;
            while (starti != null && endi != null && starti != endi)
            {
                starti = starti.Parent;
                endi = endi.Parent;
            }
            while (starti != null && endi != null && starti == endi && !(starti is ITextZone))
            {
                starti = starti.Parent;
                endi = endi.Parent;
            }
            // 进行处理
            if (starti != null && endi != null && starti == endi && starti is ITextZone)
                return _Replace((ITextZone)(starti), start, end, text);
            else
                return _Replace(this, start, end, text);
        }

        /// <summary> 将指定域项(ITextZone)中的部分替换，由两个边界坐标来决定要替换的部分 </summary>
        /// <param name="zone">指定的域项</param>
        /// <param name="start">起始坐标</param>
        /// <param name="end">终点坐标</param>
        /// <param name="text">要替换的新的文本</param>
        /// <returns>实际替换成的文本</returns>
        protected string _Replace(ITextZone zone, ITextPosition start, ITextPosition end, string text)
        {
            start = start.PrevSeek();
            end = end.NextSeek();
            
            ITextItem starti = start.Item;
            ITextItem endi = end.Item;
            List<ITextItem> leftitems = new List<ITextItem>();
            List<ITextItem> rightitems = new List<ITextItem>();
            // 计算插入光标的行数
            int caretline = start.Line;
            caretline += text.Count(c => c == '\n');
            // 中间的未结构化部分，之前两个坐标拆散的部分合并在一起以纯文本表示。
            string startleftpart = start.LeftPart;
            string endrightpart = end.RightPart;
            int textendline = caretline + endrightpart.Count(c => c == '\n');
            ITextPosition leftpos = startleftpart.Length > 0 ? start.PrevItem() : start.Clone();
            text = String.Format("{0}{1}{2}", startleftpart, text, endrightpart);
            // 左边的结构化部分，扫描得到
            _Replace_GetLeft(zone, start.Item.Parent, start.Item, leftitems);
            // 右边的结构化部分，扫描得到
            _Replace_GetRight(zone, end.Item.Parent, end.Item, rightitems);

            // 用Build系统重构
            BuildStart(zone.Parent, zone.ID);
            BuildAppend(leftitems);
            BuildTabStart(leftpos.GetByteSpaceCount(), 4);
            string replacedtext = BuildAppend(text, startleftpart.Length, text.Length - startleftpart.Length - endrightpart.Length);
            BuildWaitCaret(caretline, endrightpart.Length);
            BuildTabEnd();
            BuildAppend(rightitems);
            BuildEnd();

            return replacedtext;
        }

        /// <summary> 扫描左侧部分，搜集到容器中 </summary>
        /// <param name="zone">搜集范围限制的最上级根</param>
        /// <param name="current">当前域</param>
        /// <param name="target">目标，左侧部分要被搜集</param>
        /// <param name="leftitems">容器</param>
        protected void _Replace_GetLeft(ITextZone zone, ITextZone current, ITextItem target, List<ITextItem> leftitems)
        {
            // 上面的左侧在内部的左侧的左侧，优先处理
            if (current != zone) _Replace_GetLeft(zone, current.Parent, current, leftitems);
            // 遍历内部左侧得到
            for (int i = 0; i < target.ID; i++) leftitems.Add(current.Items[i]);
        }

        /// <summary> 扫描右侧部分，搜集到容器中  </summary>
        /// <param name="zone">搜集范围限制的最上级根</param>
        /// <param name="current">当前域</param>
        /// <param name="target">目标，右侧部分要被搜集</param>
        /// <param name="rightitems">容器</param>
        protected void _Replace_GetRight(ITextZone zone, ITextZone current, ITextItem target, List<ITextItem> rightitems)
        {
            // 遍历内部右侧得到
            for (int i = target.ID + 1; i < current.Items.Count; i++) rightitems.Add(current.Items[i]);
            // 上面的右侧在内部的右侧的右侧，滞后处理
            if (current != zone) _Replace_GetRight(zone, current.Parent, current, rightitems);
        }

        #endregion

        #region Build

        /// <summary> Build系统处理的域 </summary>
        private ITextZone buildzone;
        /// <summary> Build系统处理的域的位置 </summary>
        private int buildid;
        /// <summary> Build系统影响到的第一行 </summary>
        private int buildlinestart;
        /// <summary> Build系统影响到的第一行 </summary>
        public int BuildLineStart { get { return this.buildlinestart; } }
        /// <summary> Build系统影响到的最后一行 </summary>
        private int buildlineend;
        /// <summary> Build系统影响到的最后一行 </summary>
        public int BuildLineEnd { get { return this.buildlineend; } }
        /// <summary> 影响第一行对应的原第一行 </summary>
        private int originlinestart;
        /// <summary> 影响第一行对应的原第一行 </summary>
        public int OriginLineStart { get { return this.originlinestart; } }
        /// <summary> 影响最后一行对应的原最后一行 </summary>
        private int originlineend;
        /// <summary> 影响最后一行对应的原最后一行 </summary>
        public int OriginLineEnd { get { return this.originlineend; } }
        /// <summary> Build系统LIFO生成 </summary>
        private List<ITextItem> builditems = new List<ITextItem>();
        /// <summary> Build系统文本解析后的结构项 </summary>
        private List<ITextItem> waititems = new List<ITextItem>();
        /// <summary> Build系统左括号栈 </summary>
        private List<ITextKey> leftkeys = new List<ITextKey>();
        /// <summary> Build系统行起始符栈 </summary>
        private List<ITextKey> linekeys = new List<ITextKey>();
        /// <summary> Build系统中当前处于注释行内 </summary>
        private ITextKey cmtline;
        /// <summary> Build系统中当前处于注释域内 </summary>
        private ITextKey cmtzone;
        /// <summary> Build系统中最近添加的续行符 </summary>
        private ITextKey ctnline;

        /// <summary> 等待Build系统完毕后插入光标的项 </summary>
        private ITextItem waitcaret;
        /// <summary> 等待Build系统完毕后插入光标的项 </summary>
        public ITextItem WaitCaret { get { return this.waitcaret; } set { this.waitcaret = value; } }
        /// <summary> 插入光标所在项内的位置 </summary>
        private int waitcaretitemindex;
        /// <summary> 插入光标所在项内的位置 </summary>
        //public int WaitCaretItemIndex { get { return this.waitcaretitemindex; } set { this.waitcaretitemindex = value; } }
        /// <summary> 插入光标所在行 </summary>
        private int waitcaretline;
        /// <summary> 插入光标所在项的退格距离 </summary>
        private int waitcaretbackindex;
        /// <summary> 插入光标所在项的倒退行数 </summary>
        private int waitcaretbackline;
        /// <summary> 转换tab符时需要统计的字节数量 </summary>
        private int tabbytecount;
        /// <summary> 制表符转换为空格符的空格周期 </summary>
        private int tabbyteperiod;
        
        /// <summary> 开始Build系统 </summary>
        /// <param name="zone">Build系统处理的域</param>
        /// <param name="id">Build系统处理的域的位置</param>
        protected void BuildStart(ITextZone zone, int id)
        {
            this.buildzone = zone;
            this.buildid = id;
            this.cmtline = null;
            this.cmtzone = null;
            this.ctnline = null;
            builditems.Clear();
            waititems.Clear();
            leftkeys.Clear();
            linekeys.Clear();
        }

        /// <summary> 结束Build系统，将成果作用于给定的域中 </summary>
        protected void BuildEnd()
        {
            // 未完成的注释行换行符允许为空
            if (cmtline != null)
            {
                int start = cmtline.ID;
                ITextLine line = new TextLine(this, cmtline, null);
                line.Replace(1, 0, builditems.GetRange(start + 1, builditems.Count() - start - 1));
                builditems.RemoveRange(start, builditems.Count() - start);
                builditems.Add(line);
                cmtline = null;
            }
            // 未完成的普通行换行符允许为空
            foreach (ITextKey linekey in linekeys)
            {
                int start = linekey.ID;
                ITextLine line = new TextLine(this, linekey, null);
                line.Replace(1, 0, builditems.GetRange(start + 1, builditems.Count() - start - 1));
                builditems.RemoveRange(start, builditems.Count() - start);
                builditems.Add(line);
            }
            linekeys.Clear();
            // 对结构重构可能性的判断
            bool structchanged = false;
            bool _IsZoneBorder(ITextItem item)
            {
                if (!(item is ITextKey)) return false;
                ITextKey key = (ITextKey)item;
                return (key.KeyCore.Feature & (TextKeyFeatures.ZoneLeft | TextKeyFeatures.ZoneRight)) != TextKeyFeatures.None;
            }
            bool _IsContinueCommentLine(ITextItem item)
            {
                if (!(item is ITextLine)) return false;
                ITextLine line = (ITextLine)item;
                ITextItem left = line.Items.FirstOrDefault();
                if (!(left is ITextKey)) return false;
                ITextKey lkey = (ITextKey)left;
                if ((lkey.KeyCore.Feature & TextKeyFeatures.LineComment) == TextKeyFeatures.None) return false;
                if (line.HasEnterEnd()) return false;
                return true;
            }
            if (!structchanged) structchanged = builditems.FirstOrDefault(_IsZoneBorder) != null;
            if (!structchanged) structchanged = _IsContinueCommentLine(builditems.LastOrDefault());
            // 缩进相关信息
            ITextZone intentzone = null;
            int intentstart = -1;
            int intentcount = 0;
            // 如果改变域为空，说明重构的是整个根
            if (buildzone == null)
            {
                this.originlinestart = 1;
                this.originlineend = linecount;
                Replace(0, items.Count(), builditems);
                if (items.Count() == 0) items.Add(new TextTrim(this, String.Empty) { Parent = this, Level = 0 });
                this.buildlinestart = 1;
                this.buildlineend = linecount;
                intentzone = this;
                intentstart = 0;
                intentcount = items.Count();
                //AutoIntent(this, 0, items.Count());
            }
            // 有可能改变域的结构，需要将这个域拆开，迭代给上一层并重开Build系统来处理
            else if (structchanged)
            {
                ITextItem[] _builditems = builditems.ToArray();
                ITextZone _buildzone = buildzone;
                int _buildid = buildid;
                BuildStart(buildzone.Parent, buildzone.ID);
                BuildAppend(_buildzone.GetRange(0, _buildid));
                BuildAppend(_builditems);
                BuildAppend(_buildzone.GetRange(_buildid + 1, _buildzone.Items.Count() - (_buildid + 1)));
                BuildEnd();
                return;
            }
            // 没有就可以直接替换了
            else
            {
                ITextItem originitem = buildzone.Items[buildid];
                this.originlinestart = GetFirstLine(originitem);
                this.originlineend = GetLastLine(originitem);
                buildzone.Replace(buildid, 1, builditems);
                if (buildzone.Items.Count() == 0) buildzone.Add(new TextTrim(this, String.Empty));
                this.buildlinestart = GetFirstLine(buildzone.Items[buildid]);
                this.buildlineend = GetLastLine(buildzone.Items[buildid + builditems.Count() - 1]);
                //AutoIntent(buildzone, buildid, builditems.Count());
                intentzone = buildzone;
                intentstart = buildid;
                intentcount = builditems.Count();
            }
            // 完成后插入预定位置的光标
            if (waitcaret != null)
            {
                while (waitcaret is ITextZone) waitcaret = ((ITextZone)waitcaret).Items.LastOrDefault();
                waitcaretitemindex = waitcaret.ToString().Length - waitcaretbackindex;
                selectedstart = new TextPosition() { Item = waitcaret, ItemIndex = waitcaretitemindex, Line = waitcaretline, Column = -1 };
                selectedend = new TextPosition() { Item = waitcaret, ItemIndex = waitcaretitemindex, Line = waitcaretline, Column = -1 };
                waitcaret = null;
                waitcaretitemindex = -1;
            }
            // 之前的光标无效
            else
            {
                selectedstart = null;
                selectedend = null;
            }
            // 确保所选位置始终确定
            if (selectedstart == null)
                selectedstart = GetFirstPosition();
            if (selectedend == null)
                selectedend = GetLastPosition();
            // 最后再执行一次自动缩进
            if (intentzone != null)
                AutoIntent(intentzone, intentstart, intentcount);
        }

        /// <summary> 往Build系统中添加结构集 </summary>
        /// <param name="items">添加的所有项</param>
        protected void BuildAppend(IEnumerable<ITextItem> items)
        {
            foreach (ITextItem _item in items)
            {
                ITextItem item = _item;
                // 假设要加入到生成队列中，预设置ID为队列位置
                item.ID = builditems.Count();
                // 预先处理一次非关键词非空白符的向前合并，因为可能会出现新的关键词
                if (!(item is ITextZone) && !(item is ITextKey) && !(item is ITextTrim))
                {
                    ITextItem last = builditems.LastOrDefault();
                    // 是否向前合并
                    bool combine = false;
                    // 向前合并后生成的新的关键词
                    string newword = null;
                    if (last is ITextKey)
                    {
                        ITextKey lastkey = (ITextKey)last;
                        char ch = item.ToString().FirstOrDefault();
                        newword = lastkey.KeyCore.Keyword + item.ToString();
                        combine |= (lastkey.KeyCore.Relation & TextKeyRelations.RightKey) == TextKeyRelations.None && TextChar.IsKey(ch);
                        combine |= (lastkey.KeyCore.Relation & TextKeyRelations.RightWord) == TextKeyRelations.None && TextChar.IsWord(ch);
                    }
                    else if (last is ITextWord && !(last is ITextTrim))
                    {
                        newword = last.ToString() + item.ToString();
                        combine = true;
                    }
                    // 新的关键词强制优先生成
                    if (newword != null && dictkey.ContainsKey(newword))
                    {
                        item = new TextKey(this, dictkey[newword]);
                        // 之前的等待光标项换成合并后的项
                        if (waitcaret != null && waitcaret == builditems.LastOrDefault())
                        {
                            waitcaretbackindex += newword.Length - waitcaret.ToString().Length;
                            waitcaret = item;
                        }
                        builditems.RemoveAt(builditems.Count() - 1);
                    }
                    // 可以向前合并
                    else if (combine)
                    {
                        item = new TextWord(this, newword);
                        // 之前的等待光标项换成合并后的项
                        if (waitcaret != null && waitcaret == builditems.LastOrDefault())
                        {
                            waitcaretbackindex += newword.Length - waitcaret.ToString().Length;
                            waitcaret = item;
                        }
                        builditems.RemoveAt(builditems.Count() - 1);
                    }
                }
                // 如果为关键词                
                if (item is ITextKey)
                {
                    ITextKey key = (ITextKey)(item);
                    ITextKeyCore keycore = key.KeyCore;
                    ITextItem last = builditems.LastOrDefault();
                    // 这个关键词可能会被破坏
                    bool destroy = false;
                    // 如果被破坏的话新生成的合并词
                    string newword = null;
                    // 如果左侧也是关键词，需要考虑是否互相破坏
                    if (last is ITextKey)
                    {
                        ITextKey lastkey = (ITextKey)last;
                        newword = lastkey.KeyCore.Keyword + keycore.Keyword;
                        destroy |= (lastkey.KeyCore.Relation & TextKeyRelations.RightKey) == TextKeyRelations.None
                            && (keycore.Relation & TextKeyRelations.LeftIsKey) != TextKeyRelations.None;
                        destroy |= (lastkey.KeyCore.Relation & TextKeyRelations.RightWord) == TextKeyRelations.None
                            && (keycore.Relation & TextKeyRelations.LeftIsWord) != TextKeyRelations.None;
                        destroy |= (keycore.Relation & TextKeyRelations.LeftKey) == TextKeyRelations.None
                            && (lastkey.KeyCore.Relation & TextKeyRelations.RightIsKey) != TextKeyRelations.None;
                        destroy |= (keycore.Relation & TextKeyRelations.LeftWord) == TextKeyRelations.None
                            && (lastkey.KeyCore.Relation & TextKeyRelations.RightIsWord) != TextKeyRelations.None;
                    }
                    // 获取最左侧字符，判断是否可以把这个关键词给破坏掉
                    else if (last != null)
                    {
                        ITextItem lasttail = last;
                        while (lasttail is ITextZone) lasttail = ((ITextZone)lasttail).Items.LastOrDefault();
                        newword = lasttail.ToString() + keycore.Keyword;
                        char ch = lasttail.ToString().LastOrDefault();
                        destroy |= (keycore.Relation & TextKeyRelations.LeftKey) == TextKeyRelations.None && TextChar.IsKey(ch);
                        destroy |= (keycore.Relation & TextKeyRelations.LeftWord) == TextKeyRelations.None && TextChar.IsWord(ch);
                    }

                    // 优先生成新的关键词，后面继续分析
                    if (newword != null && dictkey.ContainsKey(newword))
                    {
                        // 破坏掉的是注释区域的开始
                        if (last == cmtzone) cmtzone = null;
                        // 破坏掉的是注释行的开始
                        if (last == cmtline) cmtline = null;
                        // 生成新的关键词
                        key = new TextKey(this, dictkey[newword]);
                        keycore = key.KeyCore;
                        if (waitcaret != null && waitcaret == builditems.LastOrDefault())
                        {
                            waitcaretbackindex += newword.Length - waitcaret.ToString().Length;
                            waitcaret = key;
                        }
                        builditems.RemoveAt(builditems.Count() - 1);
                    }
                    // 这个关键词被破坏？
                    else if (destroy)
                    {
                        // 破坏掉的是注释区域的开始
                        if (last == cmtzone) cmtzone = null;
                        // 破坏掉的是注释行的开始
                        if (last == cmtline) cmtline = null;
                        // 后面停止分析，直接加入队列中
                        key = null;
                        keycore = null;
                        ITextWord word = new TextWord(this, newword);
                        if (waitcaret != null && waitcaret == builditems.LastOrDefault())
                        {
                            waitcaretbackindex += newword.Length - waitcaret.ToString().Length;
                            waitcaret = word;
                        }
                        builditems[builditems.Count() - 1] = word;
                    }
                    // 关键词还存在的话，继续分析...
                    if (key != null && keycore != null)
                    {
                        // 如果是域的右括号
                        if ((keycore.Feature & TextKeyFeatures.ZoneRight) != TextKeyFeatures.None)
                        {
                            // 判断这个右括号是否和左括号leftkey匹配，leftkeyid为左括号所在左括号队列的位置
                            bool _MatchLeftKey(ITextKey leftkey, int leftkeyid)
                            {
                                if (keycore.Collection.Items.Contains(leftkey.KeyCore.That))
                                {
                                    int start = leftkey.ID;
                                    ITextKey rightkey = new TextKey(this, leftkey.KeyCore.That);
                                    // 根据两个括号生成队列
                                    ITextZone zone = new TextZone(this, leftkey, rightkey);
                                    zone.Replace(1, 0, builditems.GetRange(start + 1, builditems.Count() - start - 1));
                                    // 在生成队列中替换为新的域
                                    builditems.RemoveRange(start, builditems.Count() - start);
                                    builditems.Add(zone);
                                    // 左括号队列中移除匹配到的左括号，以及后面的部分
                                    if (leftkeyid >= 0) leftkeys.RemoveRange(leftkeyid, leftkeys.Count() - leftkeyid);
                                    return true;
                                }
                                return false;
                            }
                            bool findleftkey = false;
                            // 注释行内无效
                            if (cmtline != null)
                            {
                            }
                            // 优先匹配注释域的左括号
                            else if (cmtzone != null)
                            {
                                findleftkey = _MatchLeftKey(cmtzone, -1);
                                if (findleftkey) cmtzone = null;
                            }
                            // 匹配左括号队列的左括号
                            else
                            {
                                for (int i = leftkeys.Count() - 1; i >= 0; i--)
                                {
                                    findleftkey = _MatchLeftKey(leftkeys[i], i);
                                    if (findleftkey) break;
                                }
                            }
                            // 未生成域，则单独加入这个右括号关键词
                            if (!findleftkey) builditems.Add(key);
                        }
                        // 如果是特殊行的起始符
                        else if ((keycore.Feature & TextKeyFeatures.LineStart) != TextKeyFeatures.None)
                        {
                            // 注释行和注释域内无效
                            if (cmtline != null || cmtzone != null)
                            {
                                builditems.Add(key);
                            }
                            // 注释行的起始符
                            else if ((keycore.Feature & TextKeyFeatures.LineComment) != TextKeyFeatures.None)
                            {
                                builditems.Add(key);
                                cmtline = key;
                            }
                            // 特殊行的起始符，加入行起始符栈内，等待换行符来处理
                            else
                            {
                                linekeys.Add(key);
                                builditems.Add(key);
                            }
                        }
                        // 如果是续行符
                        else if ((keycore.Feature & TextKeyFeatures.LineContinue) != TextKeyFeatures.None)
                        {
                            builditems.Add(key);
                            ctnline = key;
                        }
                        // 如果是左括号
                        else if ((keycore.Feature & TextKeyFeatures.ZoneLeft) != TextKeyFeatures.None)
                        {
                            // 注释行和注释域内无效
                            if (cmtline != null || cmtzone != null)
                            {
                                builditems.Add(key);
                            }
                            // 注释域左括号
                            else if ((keycore.Feature & TextKeyFeatures.ZoneComment) != TextKeyFeatures.None)
                            {
                                builditems.Add(key);
                                cmtzone = key;
                            }
                            // 普通域左括号
                            else
                            {
                                builditems.Add(key);
                                leftkeys.Add(key);
                            }
                        }
                        // 非结构影响的关键词
                        else
                            builditems.Add(key);
                    }
                }
                // 如果为空白符
                else if (item is ITextTrim)
                {
                    ITextTrim trim = (ITextTrim)(item);
                    ITextItem last = builditems.LastOrDefault();
                    // 和之前的末尾空白符合并
                    if (last is ITextTrim)
                    {
                        ITextTrim newtrim = new TextTrim(this, last.ToString() + trim.ToString());
                        // 之前的等待光标项换成合并后的项
                        if (waitcaret != null && waitcaret == builditems.LastOrDefault())
                        {
                            waitcaretbackindex += newtrim.ToString().Length - trim.ToString().Length;
                            waitcaret = newtrim;
                        }
                        builditems[builditems.Count() - 1] = last.Insert(last.ToString().Count(), trim.ToString());
                    }
                    // 作为新的空白符添加
                    else
                    {
                        builditems.Add(trim);
                    }
                    last = builditems.LastOrDefault();
                    trim = (ITextTrim)last;
                    // 换行符之前没有续行符
                    if (trim.GetEnterCount() > 0 && (ctnline == null || ctnline.ID < trim.ID - 1))
                    {
                        // 处理注释行
                        if (cmtline != null)
                        {
                            int start = cmtline.ID;
                            ITextLine line = new TextLine(this, cmtline, trim);
                            line.Replace(1, 0, builditems.GetRange(start + 1, builditems.Count() - start - 2));
                            builditems.RemoveRange(start, builditems.Count() - start);
                            builditems.Add(line);
                            trim = null;
                            cmtline = null;
                        }
                        // 处理普通行
                        foreach (ITextKey linekey in linekeys)
                        {
                            int start = linekey.ID;
                            ITextLine line = new TextLine(this, linekey, trim);
                            line.Replace(1, 0, builditems.GetRange(start + 1, builditems.Count() - start - 2));
                            builditems.RemoveRange(start, builditems.Count() - start);
                            builditems.Add(line);
                            trim = null;
                        }
                    }
                }
                // 如果是特殊行
                else if (item is ITextLine)
                {
                    ITextLine line = (ITextLine)item;
                    // 特殊行没有换行的话，拆开重新分析
                    if (!line.HasEnterEnd() || line.HasEnterContinue())
                        BuildDestoryZone(line);
                    else
                        builditems.Add(item);
                }
                // 其他类型的项直接添加
                else
                    builditems.Add(item);
            }
        }

        /// <summary> 往Build系统中添加非结构文本 </summary>
        /// <param name="text">添加的文本</param>
        /// <param name="endline">添加这段文本后最后一行的位置</param>
        protected string BuildAppend(string text, int start, int count)
        {
            waitcaret = null;
            waitcaretbackindex = 0;
            waitcaretbackline = 0;
            // 实际插入的文本
            StringBuilder actual = new StringBuilder();
            // 清空等待队列
            waititems.Clear();
            // 依次分析每个字符
            for (int chid = 0; chid < text.Length; chid++)
            {
                char ch = text[chid];
                // 等待队列的末尾，对这个尾巴进行一系列条件分析
                ITextItem lastitem = waititems.LastOrDefault();
                // 等待队列为空则获取生成队列的末尾
                if (lastitem == null) lastitem = builditems.LastOrDefault();
                // 需要转换制表符为空格符
                if (tabbyteperiod > 0)
                {
                    if (ch == '\t')
                    {
                        int tospacecount = tabbytecount > 0 ? tabbyteperiod - tabbytecount : tabbyteperiod;
                        string tospacetext = new string(' ', tospacecount);
                        if (lastitem is ITextTrim)
                        {
                            if (waititems.Count() > 0)
                                waititems[waititems.Count() - 1] = lastitem.Insert(lastitem.ToString().Count(), tospacetext);
                            else if (builditems.Count() > 0)
                                builditems[builditems.Count() - 1] = lastitem.Insert(lastitem.ToString().Count(), tospacetext);
                        }
                        else
                        {
                            ITextTrim trim = new TextTrim(this, tospacetext);
                            waititems.Add(trim);
                        }
                        if (chid >= start && chid < start + count)
                            actual.Append(tospacetext);
                        continue;
                    }
                    else if (ch == '\n')
                    {
                        tabbytecount = 0;
                        if (waitcaret != null && chid >= start && chid < start + count) waitcaretbackline++;
                    }
                    else
                    {
                        tabbytecount += TextChar.GetByteSpaceCount(ch);
                        while (tabbytecount >= tabbyteperiod) tabbytecount -= tabbyteperiod;
                    }
                }    
                // 末尾非区域类，单词类有延展性
                if (lastitem != null && !(lastitem is ITextZone))
                {
                    // 空白项和空白字符可以合并延展
                    if (lastitem is ITextTrim && TextChar.IsTrim(ch))
                    {
                        ITextTrim trim = new TextTrim(this, lastitem.ToString() + ch);
                        if (waitcaret == lastitem)
                        {
                            waitcaret = trim;
                            waitcaretbackindex++;
                        }
                        // 将延展后的项末尾替换
                        if (waititems.Count() > 0)
                            waititems[waititems.Count() - 1] = trim;
                        else if (builditems.Count() > 0)
                            builditems[builditems.Count() - 1] = trim;
                        if (chid >= start && chid < start + count)
                            actual.Append(ch);
                        continue;
                    }
                    // 单词项和单词字符可以合并延展，这里对可能生成的关键词优先处理
                    if (!(lastitem is ITextTrim) && !TextChar.IsTrim(ch))
                    {
                        string lastword = lastitem.ToString() + ch;
                        // 生成了关键词
                        if (dictkey.ContainsKey(lastword))
                        {
                            ITextKeyCore keycore = dictkey[lastword];
                            ITextKey key = new TextKey(this, keycore) { ID = lastitem.ID };
                            if (waititems.Count() > 0)
                                waititems[waititems.Count() - 1] = key;
                            else if (builditems.Count() > 0)
                                builditems[builditems.Count() - 1] = key;
                            // 对通过最后一个字符合并生成的左括号，自动填充一个空格和对应的右括号
                            if ((keycore.Feature & TextKeyFeatures.ZoneLeft) != TextKeyFeatures.None 
                                && count > 0 && chid == start + count - 1)
                            {
                                // 建议光标插入左括号后面，准备填写括号内容
                                waitcaret = key;
                                waitcaretbackindex = 0;
                                // 添加一个空格和右括号
                                keycore = keycore.That;
                                key = new TextKey(this, keycore);
                                waititems.Add(new TextTrim(this, " "));
                                waititems.Add(key);
                                actual.Append(" ");
                                actual.Append(keycore.Keyword);
                            }
                            else if (chid >= start && chid < start + count)
                            {
                                actual.Append(ch);
                            }
                            continue;
                        }
                    }
                }
                // 字符为关键字符，具有分割特权
                if (dictonekey.ContainsKey(ch))
                {
                    ITextKeyCore keycore = dictonekey[ch];
                    ITextKey key = new TextKey(this, keycore) { ID = builditems.Count() };
                    waititems.Add(key);
                    // 对通过最后一个字符合并生成的左括号，自动填充一个空格和对应的右括号
                    if ((keycore.Feature & TextKeyFeatures.ZoneLeft) != TextKeyFeatures.None
                        && count > 0 && chid == start + count - 1)
                    {
                        // 建议光标插入左括号后面，准备填写括号内容
                        waitcaret = key;
                        waitcaretbackindex = 0;
                        // 添加一个空格和右括号
                        keycore = keycore.That;
                        key = new TextKey(this, keycore);
                        waititems.Add(key);
                        actual.Append(keycore.Keyword);
                    }
                    else if (chid >= start && chid < start + count)
                        actual.Append(ch);
                    continue;
                }
                // 单词项和单词字符可以合并延展，这里对生成的新的单词进行处理
                if (lastitem != null && !(lastitem is ITextZone))
                {
                    if (!(lastitem is ITextTrim) && !TextChar.IsTrim(ch))
                    {
                        if (lastitem is ITextKey)
                        {
                            ITextKey key = (ITextKey)lastitem;
                            ITextKeyCore keycore = key.KeyCore;
                            // 左侧的关键词不具有【右侧允许普通字符】的特性，会破坏掉这个关键词生成新的普通单词，并且这个右侧字符确实为普通字符
                            // 特殊字符同理
                            if (((keycore.Relation & TextKeyRelations.RightWord) == TextKeyRelations.None && TextChar.IsWord(ch))
                             || ((keycore.Relation & TextKeyRelations.RightKey) == TextKeyRelations.None && TextChar.IsKey(ch)))
                            {
                                if (waititems.Count() > 0)
                                    waititems[waititems.Count() - 1] = lastitem.Insert(lastitem.ToString().Count(), ch.ToString());
                                else if (builditems.Count() > 0)
                                    builditems[builditems.Count() - 1] = lastitem.Insert(lastitem.ToString().Count(), ch.ToString());
                                if (chid >= start && chid < start + count)
                                    actual.Append(ch);
                                continue;
                            }
                        }
                        // 普通单词直接向后合并
                        else
                        {
                            if (waititems.Count() > 0)
                                waititems[waititems.Count() - 1] = lastitem.Insert(lastitem.ToString().Count(), ch.ToString());
                            else if (builditems.Count() > 0)
                                builditems[builditems.Count() - 1] = lastitem.Insert(lastitem.ToString().Count(), ch.ToString());
                            if (chid >= start && chid < start + count)
                                actual.Append(ch);
                            continue;
                        }
                    }
                }
                // 空白符生成空白项
                if (TextChar.IsTrim(ch))
                {
                    ITextTrim trim = new TextTrim(this, ch.ToString());
                    waititems.Add(trim);
                    if (chid >= start && chid < start + count)
                        actual.Append(ch);
                    continue;
                }
                // 单词符生成单词项
                {
                    ITextWord word = new TextWord(this, ch.ToString());
                    waititems.Add(word);
                    if (chid >= start && chid < start + count)
                        actual.Append(ch);
                    continue;
                }
            }
            // 给结构化处理
            BuildAppend(waititems);
            // 返回实际插入的文本
            return actual.ToString();
        }

        /// <summary> 拆开一个域重新分析 </summary>
        /// <param name="zone">要拆开的域</param>
        protected void BuildDestoryZone(ITextZone zone)
        {
            BuildAppend(zone.Items);
        }

        /// <summary> 等待Build系统完成后在此处插入光标 </summary>
        /// <param name="line">插入的光标所在的行</param>
        /// <param name="backindex">末尾退格</param>
        protected void BuildWaitCaret(int line, int backindex)
        {
            // 如果之前没有建议光标项，则应用此处设置
            if (waitcaret == null)
            {
                waitcaret = builditems.LastOrDefault();
                waitcaretitemindex = waitcaret != null ? waitcaret.ToString().Length - backindex : -1;
                waitcaretline = line;
                waitcaretbackindex = backindex;
            }
            // 之前有建议光标项，则只需要更新行的位置
            else
            {
                waitcaretitemindex = waitcaret.ToString().Length - waitcaretbackindex;
                waitcaretline = line - waitcaretbackline;
            }
        }
        
        /// <summary> 开启制表符转换功能 </summary>
        /// <param name="_bytecount"></param>
        protected void BuildTabStart(int _bytecount, int _byteperiod)
        {
            this.tabbytecount = _bytecount;
            this.tabbyteperiod = _byteperiod;
        }

        /// <summary> 关闭制表符转换功能 </summary>
        protected void BuildTabEnd()
        {
            this.tabbytecount = -1;
            //this.tabbyteperiod = -1;
        }

        #endregion

        #region Intent

        /// <summary> 是否允许缩进 </summary>
        private bool intentenable = true;
        /// <summary> 是否允许缩进 </summary>
        public bool IntentEnable { get { return this.intentenable; } set { this.intentenable = value; } }
        
        /// <summary> 对这个域里的的一部分内容进行自动缩进操作 </summary>
        /// <param name="zone">指定的域</param>
        /// <param name="start">域内自动缩进项的开始</param>
        /// <param name="count">域内自动缩进项的长度</param>
        /// <returns>执行的缩进动作</returns>
        public ITextIndentAction AutoIntent(ITextZone zone, int start, int count)
        {   
            if (!intentenable || count == 0) return null;
            int intentlinestart = GetFirstLine(zone.Items[start]);
            int intentlineend = GetLastLine(zone.Items[start + count - 1]);
            ITextIndentAction action = new TextIndentAction();
            action.NewLineStart = action.OldLineStart = selectedstart.Line;
            action.NewLineEnd = action.OldLineEnd = selectedend.Line;
            action.NewColumnStart = action.OldColumnStart = selectedstart.Column;
            action.NewColumnEnd = action.OldColumnEnd = selectedend.Column;
            action.IndentStartLine = intentlinestart;
            action.IndentSpaces = new int[intentlineend - intentlinestart + 1];
            action.IndentIndics = new int[intentlineend - intentlinestart + 1];
            _AutoIntent(zone, start, count, action, intentlinestart);
            if (action.NewLineStart >= intentlinestart && action.NewLineStart <= intentlineend)
            {
                int id = action.NewLineStart - action.IndentStartLine;
                action.NewColumnStart += action.IndentSpaces[id];
                action.NewColumnStart = Math.Max(action.NewColumnStart, action.IndentIndics[id] + 1);
            }
            if (action.NewLineEnd >= intentlinestart && action.NewLineEnd <= intentlineend)
            {
                int id = action.NewLineEnd - action.IndentStartLine;
                action.NewColumnEnd += action.IndentSpaces[id];
                action.NewColumnEnd = Math.Max(action.NewColumnEnd, action.IndentIndics[id] + 1);
            }
            selectedstart = GetPosition(action.NewLineStart, action.NewColumnStart);
            selectedend = GetPosition(action.NewLineEnd, action.NewColumnEnd);
            undocore.Add(action);
            return action; 
        }
        
        /// <summary> 对这个域里的的一部分内容进行自动缩进操作 </summary>
        /// <param name="zone">指定的域</param>
        /// <param name="start">域内自动缩进项的开始</param>
        /// <param name="count">域内自动缩进项的长度</param>
        /// <param name="action">执行的缩进动作</param>
        /// <param name="linestart">域的开始行</param>
        protected void _AutoIntent(ITextZone zone, int start, int count, ITextIndentAction action, int linestart)
        {
            for (int i = start; i < start + count; i++)
            {
                ITextItem item = zone.Items[i];
                if (item is ITextTrim)
                {
                    ITextTrim trim = (ITextTrim)item;
                    int entercount = trim.GetEnterCount();
                    if (entercount > 0)
                    {
                        string oldtext = trim.ToString();
                        int enterid = oldtext.IndexOf('\n');
                        int height = zone.Level + 1;
                        StringBuilder newtext = new StringBuilder(oldtext.Substring(0, enterid));
                        if (zone.Level >= 0 && i >= zone.Items.Count - 1) height--;
                        string fragment = '\n' + new string(' ', height * tabbyteperiod);
                        if (zone.Level >= 0 && i == zone.Items.Count - 2) height--;
                        string fragend = '\n' + new string(' ', height * tabbyteperiod);
                        for (int j = 0; j < entercount - 1; j++) newtext.Append(fragment);
                        newtext.Append(fragend);
                        zone.Items[i] = new TextTrim(this, newtext.ToString()) { Parent = zone, ID = i, Level = zone.Level + 1};
                        for (int ai = -1, j = 0; j < oldtext.Length; j++)
                        {
                            if (oldtext[j] == '\n')
                            {
                                ai = ai < 0 ? linestart - action.IndentStartLine + 1 : ai + 1;
                                action.IndentSpaces[ai] = (ai == linestart + entercount - action.IndentStartLine) ? (fragend.Length - 1) : (fragment.Length - 1);
                                action.IndentIndics[ai] = action.IndentSpaces[ai];
                            }
                            else if (ai >= 0)
                                action.IndentSpaces[ai]--;
                        }
                        linestart += entercount;
                    }
                }
                else if (item is ITextZone)
                {
                    ITextZone subzone = (ITextZone)item;
                    _AutoIntent(subzone, 0, subzone.Items.Count, action, linestart);
                    linestart += subzone.LineCount - 1;
                }
            }
        }

        /// <summary> 将缩在1行的域扩展为3行 </summary>
        /// <param name="zone">一行域</param>
        /// <returns>1to3动作</returns>
        public IText1to3LineAction To3Line(ITextZone zone, ITextPosition pos)
        {
            IText1to3LineAction action = null;
            ITextPosition _selectedstart = selectedstart.Clone();
            ITextPosition _selectedend = selectedend.Clone();
            //if (zone.Parent != null && !(zone.Parent is ITextLine) && !(zone.Parent is ITextBoxCore) && zone.Parent.LineCount == 1)
            //    action = To3Line(zone.Parent, pos);
            //else
                action = new Text1to3LineAction() { SourceLine = pos.Line, ToLineNumber = 1, Zone = zone };
            action.OldLineStart = _selectedstart.Line;
            action.OldLineEnd = _selectedstart.Line;
            action.OldColumnStart = _selectedstart.Column;
            action.OldColumnEnd = _selectedend.Column;
            action.ToLineNumber += 2;
            if (pos.Item is ITextTrim)
                zone.Replace(pos.Item.ID, 1, new ITextTrim[] { new TextTrim(this, "\n") });
            else
                zone.Replace(pos.Item.ID, 0, new ITextItem[] { new TextTrim(this, "\n") });
            if (zone.Items[zone.Items.Count - 2] is ITextTrim)
            {
                zone.Replace(zone.Items.Count - 2, 1, new ITextTrim[] { new TextTrim(this, zone.Items[zone.Items.Count - 2] + "\n") });
                selectedstart = GetLastPosition(zone.Items[zone.Items.Count - 2]).Move(-1);
            }
            else
            {
                zone.Replace(zone.Items.Count - 2, 0, new ITextItem[] { new TextTrim(this, "\n") });
                selectedstart = GetFirstPosition(zone.Items[zone.Items.Count - 2]);
            }
            selectedend = selectedstart.Clone();
            action.NewLineStart = selectedstart.Line;
            action.NewLineEnd = selectedend.Line;
            action.NewColumnStart = selectedstart.Column;
            action.NewColumnEnd = selectedend.Column;
            this.originlinestart = pos.Line;
            this.originlineend = pos.Line;
            this.buildlinestart = pos.Line;
            this.buildlineend = pos.Line + action.ToLineNumber - 1;
            return action;
        }

        #endregion

        #region Position

        /// <summary> 获取第一个位置坐标 </summary>
        /// <returns></returns>
        public ITextPosition GetFirstPosition()
        {
            ITextItem item = items.FirstOrDefault();
            while (item is ITextZone)
                item = ((ITextZone)item).Items.FirstOrDefault();
            if (item == null) return null;
            return new TextPosition() { Item = item, ItemIndex = 0, Line = 1, Column = 1 };
        }

        /// <summary> 获取最后一个位置坐标 </summary>
        /// <returns></returns>
        public ITextPosition GetLastPosition()
        {
            ITextItem item = items.LastOrDefault();
            while (item is ITextZone)
                item = ((ITextZone)item).Items.LastOrDefault();
            if (item == null) return null;
            return new TextPosition() { Item = item, ItemIndex = item.ToString().Length, Line = LineCount, Column = -1 };
        }

        /// <summary> 获取第line行的第一个位置坐标 </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public ITextPosition GetLinePosition(int line)
        {
            ITextPosition tpos = GetFirstPosition();
            for (int i = 2; i <= line; i++)
            {
                if (tpos == null) return null;
                tpos = tpos.NextLine();
            }
            return tpos;
        }

        /// <summary> 获取(line,column)的位置坐标 </summary>
        /// <param name="line"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public ITextPosition GetPosition(int line, int column)
        {
            ITextPosition tpos = GetFirstPosition();
            tpos = tpos?.MoveLine(line - 1);
            tpos = tpos?.Move(column - 1);
            /*
            for (int i = 2; i <= line; i++)
            {
                if (tpos == null) return null;
                tpos = tpos.NextLine();
            }
            for (int i = 2; i <= column; i++)
            {
                if (tpos == null) return null;
                tpos = tpos.Next();
            }
            */
            return tpos;
        }
        
        /// <summary> 获取该项的第一行 </summary>
        /// <param name="item">文本项</param>
        /// <returns>行数</returns>
        public int GetFirstLine(ITextItem item)
        {
            if (item.Parent == null) return 1;
            int line = GetFirstLine(item.Parent);
            for (int i = 0; i < item.ID; i++)
            {
                ITextItem prev = item.Parent.Items[i];
                if (prev is ITextTrim) line += ((ITextTrim)prev).GetEnterCount();
                if (prev is ITextZone) line += ((ITextZone)prev).LineCount - 1;
            }
            return line;
        }

        /// <summary> 获取该项的最后一行 </summary>
        /// <param name="item">文本项</param>
        /// <returns>行数</returns>
        public int GetLastLine(ITextItem item)
        {
            if (item.Parent == null) return LineCount;
            int line = GetLastLine(item.Parent);
            for (int i = item.ID + 1; i < item.Parent.Items.Count; i++)
            {
                ITextItem next = item.Parent.Items[i];
                if (next is ITextTrim) line -= ((ITextTrim)next).GetEnterCount();
                if (next is ITextZone) line -= ((ITextZone)next).LineCount - 1;
            }
            return line;
        }

        /// <summary> 获取这个文本项的起始位置对象，若该项不为底部基本项，返回项内最靠前位置 </summary>
        /// <param name="item">文本项</param>
        /// <returns>起始位置对象</returns>
        public ITextPosition GetFirstPosition(ITextItem item)
        {
            if (item.Parent == null) return GetFirstPosition();
            int line = GetFirstLine(item);
            while (item is ITextZone) item = ((ITextZone)item).Items.FirstOrDefault();
            return new TextPosition() { Item = item, ItemIndex = 0, Line = line, Column = -1 };

        }

        /// <summary> 获取这个文本项的终止位置对象，若该项不为底部基本项，返回项内最靠后位置 </summary>
        /// <param name="item">文本项</param>
        /// <returns>终止位置对象</returns>
        public ITextPosition GetLastPosition(ITextItem item)
        {
            if (item.Parent == null) return GetLastPosition();
            int line = GetLastLine(item);
            while (item is ITextZone) item = ((ITextZone)item).Items.LastOrDefault();
            return new TextPosition() { Item = item, ItemIndex = item.ToString().Length, Line = line, Column = -1 };
        }

        #endregion

        #region Text

        /// <summary> 获取选择范围内的文本</summary>
        /// <returns></returns>
        public string GetSelectedText()
        {
            return GetText(selectedstart, selectedend);
        }

        /// <summary> 获取(start,end)范围内的文本 </summary>
        /// <param name="start">左边界</param>
        /// <param name="end">右边界</param>
        /// <returns></returns>
        public string GetText(ITextPosition start, ITextPosition end)
        {
            int poscmp = start.CompareTo(end);
            if (poscmp > 0) return null;
            if (poscmp == 0) return String.Empty;
            if (start.Item == end.Item) return start.Item.ToString().Substring(start.ItemIndex, end.ItemIndex - start.ItemIndex);
            StringBuilder sb = new StringBuilder();
            sb.Append(start.RightPart);
            ITextPosition current = start.NextItem();
            while (current != null && current.Item != end.Item)
            {
                sb.Append(current.Item.ToString());
                current = current.NextItem();
            }
            sb.Append(end.LeftPart);
            return sb.ToString();
        }
        
        /// <summary> 获取全部文本 </summary>
        /// <returns></returns>
        public string GetAllText()
        {
            return GetText(GetFirstPosition(), GetLastPosition());
        }

        #endregion

        #region LoadXml

        /// <summary> 读取显示配置 </summary>
        /// <param name="filepath">文件路径</param>
        public void LoadXmlConfig(string filepath)
        {
            XDocument xdoc = XDocument.Load(filepath);
            LoadXmlConfig(xdoc);
        }

        public void LoadXmlConfig(Stream stream)
        {
            XDocument xdoc = XDocument.Load(stream);
            LoadXmlConfig(xdoc);
        }

        public void LoadXmlConfig(XDocument xdoc)
        {
            XElement xroot = xdoc.Root;
            XAttribute xattr = null;
            XElement xele = null;
            {
                xele = xroot.Element("ColorCollection");
                if (xele != null) LoadXmlColors(xele);
                xele = xroot.Element("ValueCollection");
                if (xele != null) LoadXmlValues(xele);
            }
        }

        /// <summary> 读取代码文档 </summary>
        /// <param name="filepath">文件路径</param>
        public void LoadXmlCodeDocument(string filepath)
        {
            docscore = new Class_DocsCore(filepath);
            LoadKeywords();
        }
        
        public void LoadXmlCodeDocument(Stream stream)
        {
            docscore = new Class_DocsCore(stream);
            LoadKeywords();
        }

        /// <summary> 读取xml文档中的颜色 </summary>
        /// <param name="xcolors"></param>
        protected void LoadXmlColors(XElement xcolors)
        {
            foreach (XElement xele in xcolors.Elements("Color"))
            {
                XAttribute xaname = xele.Attribute("name");
                if (xaname == null) continue;
                XAttribute xacolor = xele.Attribute("value");
                if (xacolor == null) continue;
                string name = xaname.Value;
                Color color = Class_DocsCore.GetColor(xacolor.Value);
                Brush brush = new SolidColorBrush(color);
                dictbrush.Add(name, brush);
            }
        }

        /// <summary> 读取xml文档中的值 </summary>
        /// <param name="xvalues"></param>
        protected void LoadXmlValues(XElement xvalues)
        {
            foreach (XElement xele in xvalues.Elements("Value"))
            {
                XAttribute xaname = xele.Attribute("name");
                if (xaname == null) continue;
                XAttribute xavalue = xele.Attribute("value");
                if (xavalue == null) continue;
                string name = xaname.Value;
                double value = 0.0;
                double.TryParse(xavalue.Value, out value);
                dictvalue.Add(name, value);
            }
        }

        /// <summary> 读取文档中的所有关键字 </summary>
        protected void LoadKeywords()
        {
            dictkey.Clear();
            dictonekey.Clear();
            foreach (IDocsItem docitem in docscore.Dict.Values)
            {
                if (docitem is IDocsKeyWord)
                {
                    IDocsKeyWord keyword = (IDocsKeyWord)docitem;
                    ITextKeyCore keynew = keyword.Key;
                    ITextKeyCore keyold = null;
                    if (keynew != null)
                    {
                        if (keynew.Keyword.Length == 1)
                        {
                            if (dictonekey.TryGetValue(keynew.Keyword[0], out keyold))
                            {
                                keyold.Collection.Items.Add(keynew);
                                keynew.Collection = keyold.Collection;
                            }
                            else
                                dictonekey.Add(keynew.Keyword[0], keynew);
                        }
                        else if (keynew.Keyword.Length > 1)
                        {
                            if (dictkey.TryGetValue(keynew.Keyword, out keyold))
                            {
                                keyold.Collection.Items.Add(keynew);
                                keynew.Collection = keyold.Collection;
                            }
                            else
                                dictkey.Add(keynew.Keyword, keynew);
                        }
                    }
                }
            }
        }

        #endregion

        #region Command

        /// <summary> 文本复制 </summary>
        public void Copy()
        {
            string text = GetSelectedText();
            DataObject data = new DataObject(text);
            try { Clipboard.SetDataObject(data, true); } catch (ExternalException exce) { return; }
        }

        /// <summary> 文本剪切 </summary>
        public void Cut()
        {
            Copy();
            Delete();
        }

        /// <summary> 文本粘贴 </summary>
        public void Paste()
        {
            IDataObject data = null;
            string text = null;
            try { data = Clipboard.GetDataObject(); } catch (ExternalException exce) { return; }
            try { text = data.GetData(DataFormats.UnicodeText).ToString(); } catch (OutOfMemoryException exce) { return; }
            Replace(selectedstart, selectedend, text);
        }

        /// <summary> 文本删除 </summary>
        public void Delete()
        {
            Replace(selectedstart, selectedend, String.Empty);
        }

        /// <summary> 全选 </summary>
        public void SelectAll()
        {
            selectedstart = GetFirstPosition();
            selectedend = GetLastPosition();
        }

        /// <summary> 是否可以撤销 </summary>
        /// <returns></returns>
        public bool CanUndo()
        {
            return undocore != null && undocore.Undos.Count > 0;
        }

        /// <summary> 是否可以恢复 </summary>
        /// <returns></returns>
        public bool CanRedo()
        {
            return undocore != null && undocore.Redos.Count > 0;
        }

        /// <summary> 撤销 </summary>
        public void Undo()
        {
            ITextAction _action = undocore.Undo();
            if (_action is ITextUndoRedoAction)
            {
                ITextUndoRedoAction action = (ITextUndoRedoAction)_action;
                if (action.Backspace)
                {
                    ITextPosition bppos = GetPosition(action.OldLineStart, action.OldColumnStart);
                    for (int i = 0; i < action.BackspaceText.Length; i++) bppos = bppos?.Prev();
                    Replace(bppos, bppos, action.BackspaceText);
                }
                else
                {
                    ITextPosition posstart = GetPosition(action.OldLineStart, action.OldColumnStart);
                    ITextPosition posend = posstart;
                    for (int i = 0; i < action.ReplacedText.Length; i++) posend = posend?.Next();
                    if (posstart == null || posend == null) return;
                    Replace(posstart, posend, action.RemovedText);
                }
            }
        }
        
        /// <summary> 恢复 </summary>
        public void Redo()
        {
            ITextAction _action = undocore.Undo();
            if (_action is ITextUndoRedoAction)
            {
                ITextUndoRedoAction action = (ITextUndoRedoAction)_action;
                if (action.Backspace)
                {
                    ITextPosition bppos = GetPosition(action.OldLineStart, action.OldColumnStart);
                    if (bppos == null) return;
                    selectedstart = bppos;
                    selectedend = bppos;
                    Backspace(action.BackspaceText.Length);
                }
                else
                {
                    ITextPosition posstart = GetPosition(action.OldLineStart, action.OldColumnStart);
                    ITextPosition posend = posstart;
                    for (int i = 0; i < action.RemovedText.Length; i++) posend = posend?.Next();
                    if (posstart == null || posend == null) return;
                    Replace(posstart, posend, action.ReplacedText);
                }
            }
        }

        /// <summary> 清空撤销栈 </summary>
        public void UndoClear()
        {
            undocore?.Clear();
        }

        #endregion

        #endregion

    }
}
