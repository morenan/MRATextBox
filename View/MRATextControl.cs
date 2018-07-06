using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Input;
using Morenan.MRATextBox.Core;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace Morenan.MRATextBox.View
{
    /// <summary> 充当文本编辑器的容器面板 </summary>
    internal class MRATextControl : ListBox
    {
        static MRATextControl()
        {
            //DefaultStyleKeyProperty.OverrideMetadata(typeof(MRATextControl), new FrameworkPropertyMetadata(default(MRATextControl)));
        }

        /// <summary> 构造函数 </summary>
        public MRATextControl()
        {
            this.core = null;
            this.ui_stack = null;
            this.items = new List<IMRATextItemInfo>();
            this.skips = new List<IMRAZoneSkipInfo>();
            this.hitcache = null;
            this.slidetimer = new DispatcherTimer(new TimeSpan(0, 0, 0, 0, 100), DispatcherPriority.Normal, OnTimerSlide, Dispatcher);
            this.structbarzone = null;
            this.structbarstart = 0;
            this.structbarend = -1;
            Background = Brushes.Transparent;
            Style = (Style)ResourceManager.Get("Style_MRATextControl");
            ItemContainerStyle = (Style)ResourceManager.Get("Style_MRATextControl_Item");
            ItemsSource = items;
        }
        
        #region Number

        /// <summary> 文本核 </summary>
        private TextBoxCore core;
        /// <summary> 文本核 </summary>
        public TextBoxCore Core
        {
            get
            {
                return this.core;
            }
            set
            {
                this.core = value;
                InvalidateItems();
            }
        }
        public MRATextBox ViewParent { get { return core?.View; } }

        /// <summary> 虚拟化栈容器 </summary>
        private MRATextVirtualizeStackPanel ui_stack;
        /// <summary> 虚拟化栈容器 </summary>
        public MRATextVirtualizeStackPanel UI_Stack
        {
            get { return this.ui_stack; }
            set {  this.ui_stack = value; }
        }
        
        private ScrollBar ui_hscroll;
        public ScrollBar UI_HScroll { get { return this.ui_hscroll; } }

        private ScrollBar ui_vscroll;
        public ScrollBar UI_VScroll { get { return this.ui_vscroll; } }

        /// <summary> 所有文本行项 </summary>
        private List<IMRATextItemInfo> items;
        /// <summary> 所有省略行项 </summary>
        private List<IMRAZoneSkipInfo> skips;

        /// <summary> 所有鼠标动作 </summary>
        private enum MouseActions { Up = 0, Down = 1, Drag = 3 };
        /// <summary> 所有滑动动作 </summary>
        private enum SlideActions { None = 0, Up = 1, Down = 2, Left = 4, Right = 8 };

        /// <summary> 滑动处理定时器 </summary>
        private DispatcherTimer slidetimer;
        /// <summary> 鼠标动作 </summary>
        private MouseActions mouseaction;
        /// <summary> 滑动动作 </summary>
        private SlideActions slideaction;
        /// <summary> 命中缓存，始终为上一次命中的目标 </summary>
        private IMRATextItemInfo hitcache;
        /// <summary> 鼠标拖动选择时的出发点 </summary>
        private ITextPosition selectedstart;
        /// <summary> 左侧结构栏的鼠标命中的域项 </summary>
        private ITextZone structbarzone;
        /// <summary> 左侧结构栏的鼠标命中的域项的起始索引 </summary>
        private int structbarstart;
        /// <summary> 左侧结构栏的鼠标命中的域项的结束索引 </summary>
        private int structbarend;
        
        #endregion

        #region Method
        
        /// <summary> 当前行项无效化，重新生成一遍 </summary>
        public void InvalidateItems()
        {
            foreach (IMRATextItemInfo item in items) item?.Dispose();
            items = new List<IMRATextItemInfo>();
            skips = new List<IMRAZoneSkipInfo>();
            // 获取所有的省略区域
            _GatherSkipZone(core);
            // 第1行第1列的位置
            ITextPosition current = core.GetFirstPosition();
            // 检查到的当前省略行索引
            int skipid = 0;
            if (core != null)
                for (int i = 1; i <= core.LineCount; i++)
                {
                    if (skipid < skips.Count && skips[skipid].LineStart == i)
                    {
                        IMRAZoneSkipInfo skip = skips[skipid++];
                        skip.ID = items.Count();
                        skip.Start = current;
                        for (int j = skip.LineStart; j <= skip.LineEnd; j++)
                            current = current?.NextLine();
                        skip.End = current?.Prev() ?? core.GetLastPosition();
                        i = skip.LineEnd;
                        items.Add(skip);
                    }
                    else
                    {
                        MRATextItemInfo item = new MRATextItemInfo(core, items.Count(), i);
                        // 第i行第1列设为起始位置
                        item.Start = current;
                        // 第i+1行第1列的位置
                        current = current?.NextLine();
                        // 上移一格，获取第i行最后一列的位置为结束位置（备选结果为文本的最后位置）
                        item.End = current?.Prev() ?? core.GetLastPosition();
                        items.Add(item);
                    }
                }
            
            ItemsSource = items;
            this.hitcache = null;
            this.selectedstart = Core.SelectedStart;
        }

        /// <summary> 因为鼠标事件导致当前光标无效化，重新生成一遍 </summary>
        /// <param name="e"></param>
        protected void InvalidateCursor(MouseEventArgs e)
        {
            Point p = e.GetPosition(this);
            if (p.X >= ViewParent.MarginLeft)
                Cursor = Cursors.IBeam;
            else
                Cursor = Cursors.Arrow;
        }

        public IMRATextItemInfo GetItem(int line)
        {
            int l = 0, r = items.Count() - 1;
            IMRATextItemInfo item = null;
            while (r - l > 1)
            {
                int m = (l + r) >> 1;
                item = items[m];
                if (line < item.Line)
                    r = m - 1;
                else if (item is IMRAZoneSkipInfo)
                {
                    if (((IMRAZoneSkipInfo)item).LineEnd < line)
                        l = m + 1;
                    else
                        return item;
                }
                else if (item.Line < line)
                    l = m + 1;
                else
                    return item;
            }
            item = items[l];
            if (item is IMRAZoneSkipInfo && item.Line <= line && line <= ((IMRAZoneSkipInfo)item).LineEnd) return item;
            if (item.Line == line) return item;
            item = items[r];
            if (item is IMRAZoneSkipInfo && item.Line <= line && line <= ((IMRAZoneSkipInfo)item).LineEnd) return item;
            if (item.Line == line) return item;
            return null;
        }

        /// <summary> 更改选择区域，并重新更新显示 </summary>
        /// <param name="_start">选择的开始位置</param>
        /// <param name="_end">选择的结束位置</param>
        public void SelectedChange(ITextPosition _start, ITextPosition _end)
        {
            // 实际区域未更改？
            if (core.SelectedStart.CompareTo(_start) == 0 && core.SelectedEnd.CompareTo(_end) == 0) return;
            // 旧的选择区域
            int oldlinestart = core.SelectedStart.Line;
            int oldlineend = core.SelectedEnd.Line;
            // 新的选择区域
            int newlinestart = _start.Line;
            int newlineend = _end.Line;
            // 两代区域的并集
            int efflinestart = Math.Min(oldlinestart, newlinestart);
            int efflineend = Math.Max(oldlineend, newlineend);
            // 两代区域的交集
            int keplinestart = Math.Max(oldlinestart, newlinestart) + 1;
            int keplineend = Math.Min(oldlineend, newlineend) - 1;
            // 对应的实际项的区域
            IMRATextItemInfo itemstart = null;
            IMRATextItemInfo itemend = null;
            // 设置新区域
            core.SelectedStart = _start;
            core.SelectedEnd = _end;
            // 两个区域行位置相同，更新同一个内部
            if (oldlinestart == newlinestart && oldlineend == newlineend)
            {
                itemstart = GetItem(oldlinestart);
                itemend = GetItem(oldlineend);
                for (int i = itemstart.ID; i <= itemend.ID; i++)
                    items[i]?.View?.InvalidateSelection();
            }
            // 交集不为空，取差集更新
            else if (keplineend >= keplinestart)
            {
                itemstart = GetItem(efflinestart);
                itemend = GetItem(keplinestart);
                for (int i = itemstart.ID; i <= itemend.ID ; i++)
                    items[i]?.View?.InvalidateSelection();
                itemstart = GetItem(keplineend);
                itemend = GetItem(efflineend);
                for (int i = itemstart.ID; i <= itemend.ID; i++)
                    items[i]?.View?.InvalidateSelection();
            }
            // 交集为空，两个区域分开更新
            else
            {
                itemstart = GetItem(oldlinestart);
                itemend = GetItem(oldlineend);
                for (int i = itemstart.ID; i <= itemend.ID; i++)
                    items[i]?.View?.InvalidateSelection();
                itemstart = GetItem(newlinestart);
                itemend = GetItem(newlineend);
                for (int i = itemstart.ID; i <= itemend.ID; i++)
                    items[i]?.View?.InvalidateSelection();
            }
        }

        /// <summary> 将选择区域替换为给定文本 </summary>
        /// <param name="text"></param>
        public void SelectedReplace(string text)
        {
            // 更改区域的上一行下一行的位置，不会受到影响的外围可以维持
            //ITextPosition start = core.SelectedStart?.PrevLine()?.PrevLine()?.Next() ?? core.GetFirstPosition();
            ITextPosition start = core.SelectedStart?.PrevLine();
            start = start?.PrevLine();
            start = start?.Next();
            ITextPosition end = core.SelectedEnd?.NextLine();
            // 旧的更改区域下一行，为空一般是到达了文本末尾
            int? poldline = end?.Line;
            // 旧的行总数
            int oldlinecount = core.LineCount;
            core.Replace(core.SelectedStart, core.SelectedEnd, text);
            // 新的更改区域下一行
            int? pnewline = core.SelectedEnd.NextLine()?.Line;
            // 新的行总数
            int newlinecount = core.LineCount;
            // CASE 1 : 修改前后都没有到达文本末尾，并且行数没有被更改，为了节省ItemsSource开销，直接修改区域内行项更新
            if (poldline != null && pnewline != null && poldline.Value == pnewline.Value)
            {
                int startline = start?.Line ?? 1;
                int endline = pnewline.Value;
                ITextPosition current = start ?? core.GetFirstPosition();
                IMRATextItemInfo startitem = GetItem(startline);
                IMRATextItemInfo enditem = GetItem(endline);
                for (int i = startitem.ID; i <= enditem.ID; i++)
                {
                    items[i].Start = current;
                    if (items[i] is IMRAZoneSkipInfo)
                    {
                        IMRAZoneSkipInfo skitem = (IMRAZoneSkipInfo)(items[i]);
                        for (int j = skitem.LineStart; j <= skitem.LineEnd; j++)
                            current = current?.NextLine();
                    }
                    else
                    {
                        current = current?.NextLine();
                    }
                    items[i].End = current?.Prev() ?? core.GetLastPosition();
                    items[i].View?.InvalidateVisual();
                }
            }
            // CASE 2: 修改前后都到达文本末尾，并且行数没有被更改
            else if (poldline == null && pnewline == null && oldlinecount == newlinecount)
            {
                int startline = start?.Line ?? 1;
                int endline = newlinecount;
                ITextPosition current = start ?? core.GetFirstPosition();
                for (int i = startline - 1; i < endline; i++)
                {
                    items[i].Start = current;
                    if (items[i] is IMRAZoneSkipInfo)
                    {
                        IMRAZoneSkipInfo skitem = (IMRAZoneSkipInfo)(items[i]);
                        for (int j = skitem.LineStart; j <= skitem.LineEnd; j++)
                            current = current?.NextLine();
                    }
                    else
                    {
                        current = current?.NextLine();
                    }
                    items[i].End = current?.Prev() ?? core.GetLastPosition();
                    items[i].View?.InvalidateVisual();
                }
            }
            // CASE 3: 其他的情况，进行ItemsSource的更新
            else
            {
                InvalidateItems();
            }
            this.selectedstart = core.SelectedStart;
        }

        /// <summary> 执行一次退格 </summary>
        /// <returns>退格掉的文本</returns>
        public string Backspace()
        {
            int poscmp = Core.SelectedStart.CompareTo(Core.SelectedEnd);
            string backsapcetext = null;
            // 选择一段区域时，退格用删除代替
            if (poscmp < 0)
            {
                backsapcetext = Core.GetSelectedText();
                SelectedReplace(String.Empty);
                return backsapcetext;
            }
            // 执行一次退格，根据退格的行数来更新界面
            backsapcetext = Core.Backspace();
            int linecount = backsapcetext.Count(c => c == '\n');
            if (linecount == 0)
            {
                IMRATextItemInfo item = GetItem(Core.SelectedStart.Line);
                item.Start = Core.SelectedStart.PrevLine()?.Next() ?? Core.GetFirstPosition();
                item?.View?.InvalidateVisual();
            }
            else
            {
                InvalidateItems();
            }
            return backsapcetext;
        }

        /// <summary> 将指定索引所在项滚动至可视范围 </summary>
        /// <param name="index">索引</param>
        public void ScrollIntoView(int line)
        {
            IMRATextItemInfo item = GetItem(line);
            if (item != null) ScrollIntoView(item);
        }

        public void ScrollIntoView(int line, int column)
        {
            IMRATextItemInfo item = GetItem(line);
            if (item != null) ScrollIntoView(item);
            if (item?.View != null && ui_stack != null)
            {
                Rect rect = item.View.GetColumnActualRect(column);
                if (rect.Left < ui_stack.HorizontalOffset)
                    ui_stack.SetHorizontalOffset(rect.Left);
                if (rect.Right > ui_stack.HorizontalOffset + ui_stack.ViewportWidth)
                    ui_stack.SetHorizontalOffset(rect.Right - ui_stack.ViewportWidth);
            }
        }

        public void ScrollIntoView(ITextPosition txtpos)
        {
            ScrollIntoView(txtpos.Line, txtpos.Column);
        }


        /// <summary> 搜集省略域 </summary>
        /// <param name="zone">当前域</param>
        /// <param name="startline">所在行</param>
        protected void _GatherSkipZone(ITextZone zone, int startline = 1)
        {
            // 当前域省略
            if (zone.IsSkip)
            {
                IMRAZoneSkipInfo skip = new MRAZoneSkipInfo(core, skips.Count(), startline, startline + zone.LineCount - 1, zone);
                skips.Add(skip);
                return;
            }
            // 搜集下一层
            foreach (ITextItem item in zone.Items)
            {
                if (item is ITextTrim)
                {
                    ITextTrim trim = (ITextTrim)item;
                    startline += trim.GetEnterCount();
                }
                if (item is ITextZone)
                {
                    ITextZone subzone = (ITextZone)item;
                    if (subzone.SkipCount > 0) _GatherSkipZone(subzone, startline);
                    startline += subzone.LineCount - 1;
                }
            }
        }

        #endregion

        #region Event Handler

        /// <summary> WinAPI : 获取键盘状态 </summary>
        /// <param name="pbKeyState"></param>
        /// <returns></returns>
        [DllImport("user32.dll", EntryPoint = "GetKeyBoardState")]
        public static extern int GetKeyboardState(byte[] pbKeyState);

        /// <summary> 是否是大写？ </summary>
        /// <returns>是</returns>
        protected bool IsCapslock()
        {
            byte[] bs = new byte[256];
            GetKeyboardState(bs);
            return bs[0x14] == 1;
        }

        /// <summary> 根据鼠标事件获取文本位置，同时更新鼠标对文本域的经过 </summary>
        /// <param name="e">鼠标事件</param>
        /// <returns>文本位置</returns>
        protected ITextPosition GetTextPosition(MouseEventArgs e)
        {
            IMRATextItemInfo item = GetTextItemInfo(e);
            if (item?.View == null) return null;
            Point p = e.GetPosition(item.View);
            ITextZone _structbarzone = null;
            // 获取新的鼠标经过的文本域
            if (item.View.StructBarMouseOver)
            {
                if (item.View.OpenZone != null)
                    _structbarzone = item.View.OpenZone;
                else if (item.View.CloseZone != null)
                    _structbarzone = item.View.CloseZone;
                else if (item.View.IntoZone != null)
                    _structbarzone = item.View.IntoZone;
            }
            // 两代不同，更新界面
            if (structbarzone != _structbarzone)
            {
                // 取消掉旧界面
                if (structbarzone != null)
                {
                    for (int i = structbarstart; i <= structbarend; i++)
                        if (items[i]?.View != null) items[i].View.StructBarMouseOver = false;
                }
                // 初始化新界面信息
                structbarzone = _structbarzone;
                structbarstart = structbarend = item.Line - 1;
                // 更新新界面，向上下查找
                if (structbarzone != null)
                {
                    for (int i = 0; i < items.Count(); i++)
                    {
                        IMRATextItemInfo previtem = hitcache.Line - i - 1 >= 0 ? items[item.Line - i - 1] : null;
                        IMRATextItemInfo nextitem = hitcache.Line + i - 1 < items.Count() ? items[item.Line + i - 1] : null;
                        if (previtem?.View == null && nextitem?.View == null) break;
                        // 上方被当前文本域覆盖？
                        if (previtem?.View != null)
                        {
                            if (structbarzone.IsAncestorOf(previtem.View.OpenZone)
                             || structbarzone.IsAncestorOf(previtem.View.CloseZone)
                             || structbarzone.IsAncestorOf(previtem.View.IntoZone))
                            {
                                previtem.View.StructBarMouseOver = true;
                                structbarstart = item.Line - i - 1;
                            }
                        }
                        // 下方被当前文本域覆盖？
                        if (nextitem?.View != null)
                        {
                            if (structbarzone.IsAncestorOf(nextitem.View.OpenZone)
                             || structbarzone.IsAncestorOf(nextitem.View.CloseZone)
                             || structbarzone.IsAncestorOf(nextitem.View.IntoZone))
                            {
                                nextitem.View.StructBarMouseOver = true;
                                structbarend = item.Line + i - 1;
                            }
                        }
                    }
                }
            }
            return item.View.GetTextPosition(p);
        }

        /// <summary> 获取鼠标命中的行项 </summary>
        /// <param name="e">鼠标事件</param>
        /// <returns>命中行项</returns>
        protected IMRATextItemInfo GetTextItemInfo(MouseEventArgs e)
        {
            // 命中缓存没有实际可见？
            if (hitcache?.View == null)
            {
                // 重新从可见顶层开始查找
                int viewstart = ui_stack != null ? (int)(ui_stack.VerticalOffset) : 0;
                //int viewend = ui_stack != null ? (int)(ui_stack.VerticalOffset + ui_stack.ViewportHeight) : 0;
                for (int i = viewstart; i < items.Count(); i++)
                {
                    if (items[i].View == null) break;
                    Point p = e.GetPosition(items[i].View);
                    if (p.Y >= 0 && p.Y <= items[i].View.ActualHeight)
                    {
                        hitcache = items[i];
                        hitcache.View.UpdateMouseOver(p);
                        return hitcache;
                    }
                }
            }
            // 从命中缓存向外扩散查找
            else
            {
                Point p = e.GetPosition(hitcache.View);
                if (p.Y >= 0 && p.Y <= hitcache.View.ActualHeight)
                {
                    hitcache.View.UpdateMouseOver(p);
                    return hitcache;
                }
                // 鼠标已不再当前缓存了，释放鼠标信息
                hitcache.View.ReleaseMouseOver();
                // 向外扩散查找
                for (int i = 1; i < items.Count(); i++)
                {
                    IMRATextItemInfo previtem = hitcache.ID - i >= 0 ? items[hitcache.ID - i] : null;
                    IMRATextItemInfo nextitem = hitcache.ID + i < items.Count() ? items[hitcache.ID + i] : null;
                    if (previtem?.View == null && nextitem?.View == null) break;
                    // 向上找到新的命中项
                    if (previtem?.View != null)
                    {
                        p = e.GetPosition(previtem.View);
                        if (p.Y >= 0 && p.Y <= previtem.View.ActualHeight)
                        {
                            hitcache = previtem;
                            hitcache.View.UpdateMouseOver(p);
                            return hitcache;
                        }
                    }
                    // 向下找到新的命中项
                    if (nextitem?.View != null)
                    {
                        p = e.GetPosition(nextitem.View);
                        if (p.Y >= 0 && p.Y <= nextitem.View.ActualHeight)
                        {
                            hitcache = nextitem;
                            hitcache.View.UpdateMouseOver(p);
                            return hitcache;
                        }
                    }
                }
            }
            // 找不到命中项
            return null;
        }

        /// <summary> 输入文本时 </summary>
        /// <param name="e">输入事件</param>
        protected override void OnTextInput(TextCompositionEventArgs e)
        {
            base.OnTextInput(e);
            SelectedReplace(e.Text.Equals("\r") ? "\n" : e.Text);
            ViewParent?.CltClose(e.Text);
            ViewParent?.CltOpen(e.Text);
            ViewParent?.CltUpdate();    
        }
        
        /// <summary> 键盘提前按下时 </summary>
        /// <param name="e">键盘事件</param>
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            switch (e.Key)
            {
                case Key.Tab:
                    e.Handled = true;
                    SelectedReplace("\t");
                    break;
                case Key.Back:
                    e.Handled = true;
                    string backspacetext = Backspace();
                    ViewParent?.CltCloseByBackspace(backspacetext);
                    ViewParent?.CltUpdate();
                    break;
                case Key.Up:
                    e.Handled = true;
                    if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
                    {
                        if (ui_stack != null)
                        {
                            ui_stack.LineUp();
                            if (core.SelectedStart.Line - 1 >= (int)(ui_stack.VerticalOffset + ui_stack.ViewportHeight))
                            {
                                ITextPosition newpos = core.SelectedStart.Up();
                                if (newpos != null)
                                {
                                    SelectedChange(newpos, newpos);
                                    selectedstart = newpos;
                                }
                            }
                        }
                    }
                    else if (e.KeyboardDevice.Modifiers == ModifierKeys.Shift)
                    {
                        if (selectedstart != null)
                        {
                            if (selectedstart.CompareTo(core.SelectedEnd) == 0)
                            {
                                ITextPosition newstart = core.SelectedStart.Up();
                                if (newstart != null)
                                {
                                    SelectedChange(newstart, core.SelectedEnd);
                                    ScrollIntoView(newstart);
                                }
                            }
                            else if (selectedstart.CompareTo(core.SelectedStart) == 0)
                            {
                                ITextPosition newend = core.SelectedEnd.Up();
                                if (newend != null && newend.CompareTo(core.SelectedStart) >= 0)
                                {
                                    SelectedChange(core.SelectedStart, newend);
                                    ScrollIntoView(newend);
                                }
                                else
                                {
                                    SelectedChange(core.SelectedStart, core.SelectedStart);
                                    ScrollIntoView(core.SelectedStart);
                                }
                            }
                        }
                    }
                    else
                    {
                        ITextPosition newpos = core.SelectedStart.Up();
                        if (newpos == null) newpos = core.SelectedStart;
                        SelectedChange(newpos, newpos);
                        selectedstart = newpos;
                        ScrollIntoView(newpos);
                    }
                    break;
                case Key.Down:
                    e.Handled = true;
                    if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
                    {
                        if (ui_stack != null)
                        {
                            ui_stack.LineDown();
                            if (core.SelectedEnd.Line - 1 < (int)(ui_stack.VerticalOffset))
                            {
                                ITextPosition newpos = core.SelectedEnd.Down();
                                if (newpos != null)
                                {
                                    SelectedChange(newpos, newpos);
                                    selectedstart = newpos;
                                }
                            }
                        }
                    }
                    else if (e.KeyboardDevice.Modifiers == ModifierKeys.Shift)
                    {
                        if (selectedstart != null)
                        {
                            if (selectedstart.CompareTo(core.SelectedStart) == 0)
                            {
                                ITextPosition newend = core.SelectedEnd.Down();
                                if (newend != null)
                                {
                                    SelectedChange(core.SelectedStart, newend);
                                    ScrollIntoView(newend);
                                }
                            }
                            else if (selectedstart.CompareTo(core.SelectedEnd) == 0)
                            {
                                ITextPosition newstart = core.SelectedStart.Down();
                                if (newstart != null && newstart.CompareTo(core.SelectedEnd) <= 0)
                                {
                                    SelectedChange(newstart, core.SelectedEnd);
                                    ScrollIntoView(newstart);
                                }
                                else
                                {
                                    SelectedChange(core.SelectedEnd, core.SelectedEnd);
                                    ScrollIntoView(core.SelectedEnd);
                                }
                            }
                        }
                    }
                    else
                    {
                        ITextPosition newpos = core.SelectedStart.Down();
                        if (newpos == null) newpos = core.SelectedStart;
                        SelectedChange(newpos, newpos);
                        selectedstart = newpos;
                        ScrollIntoView(newpos);
                    }
                    break;
                case Key.Left:
                    e.Handled = true;
                    switch (e.KeyboardDevice.Modifiers)
                    {
                        case ModifierKeys.None:
                            {
                                ITextPosition newpos = core.SelectedStart.Prev();
                                if (newpos == null) newpos = core.SelectedStart;
                                SelectedChange(newpos, newpos);
                                selectedstart = newpos;
                                ScrollIntoView(newpos);
                            }
                            break;
                        case ModifierKeys.Control:
                            {
                                ITextPosition newpos = core.SelectedStart.PrevItem();
                                if (newpos == null) newpos = core.SelectedStart;
                                SelectedChange(newpos, newpos);
                                selectedstart = newpos;
                                ScrollIntoView(newpos);
                            }
                            break;
                        case ModifierKeys.Shift:
                            if (selectedstart != null)
                            {
                                if (selectedstart.CompareTo(core.SelectedEnd) == 0)
                                {
                                    ITextPosition newstart = core.SelectedStart.Prev();
                                    if (newstart != null)
                                    {
                                        SelectedChange(newstart, core.SelectedEnd);
                                        ScrollIntoView(newstart);
                                    }
                                }
                                else if (selectedstart.CompareTo(core.SelectedStart) == 0)
                                {
                                    ITextPosition newend = core.SelectedEnd.Prev();
                                    if (newend != null && newend.CompareTo(core.SelectedStart) >= 0)
                                    {
                                        SelectedChange(Core.SelectedStart, newend);
                                        ScrollIntoView(newend);
                                    }
                                    else
                                    {
                                        SelectedChange(Core.SelectedStart, Core.SelectedStart);
                                        ScrollIntoView(Core.SelectedStart);
                                    }
                                }
                            }
                            break;
                        case ModifierKeys.Shift | ModifierKeys.Control:
                            if (selectedstart != null)
                            {
                                if (selectedstart.CompareTo(core.SelectedEnd) == 0)
                                {
                                    ITextPosition newstart = core.SelectedStart.PrevItem();
                                    if (newstart != null)
                                    {
                                        SelectedChange(newstart, core.SelectedEnd);
                                        ScrollIntoView(newstart);
                                    }
                                }
                                else if (selectedstart.CompareTo(core.SelectedStart) == 0)
                                {
                                    ITextPosition newend = core.SelectedEnd.PrevItem();
                                    if (newend != null && newend.CompareTo(core.SelectedStart) >= 0)
                                    {
                                        SelectedChange(Core.SelectedStart, newend);
                                        ScrollIntoView(newend);
                                    }
                                    else
                                    {
                                        SelectedChange(Core.SelectedStart, Core.SelectedStart);
                                        ScrollIntoView(Core.SelectedStart);
                                    }
                                }
                            }
                            break;
                    }
                    break;
                case Key.Right:
                    e.Handled = true;
                    switch (e.KeyboardDevice.Modifiers)
                    {
                        case ModifierKeys.None:
                            {
                                ITextPosition newpos = core.SelectedEnd.Next();
                                if (newpos == null) newpos = core.SelectedEnd;
                                SelectedChange(newpos, newpos);
                                selectedstart = newpos;
                                ScrollIntoView(newpos);
                            }
                            break;
                        case ModifierKeys.Control:
                            {
                                ITextPosition newpos = core.SelectedStart.NextItem();
                                if (newpos == null) newpos = core.SelectedEnd;
                                SelectedChange(newpos, newpos);
                                selectedstart = newpos;
                                ScrollIntoView(newpos);
                            }
                            break;
                        case ModifierKeys.Shift:
                            if (selectedstart != null)
                            {
                                if (selectedstart.CompareTo(core.SelectedStart) == 0)
                                {
                                    ITextPosition newend = core.SelectedEnd.Next();
                                    if (newend != null)
                                    {
                                        SelectedChange(core.SelectedStart, newend);
                                        ScrollIntoView(newend);
                                    }
                                }
                                else if (selectedstart.CompareTo(core.SelectedEnd) == 0)
                                {
                                    ITextPosition newstart = core.SelectedStart.Next();
                                    if (newstart != null && newstart.CompareTo(core.SelectedEnd) <= 0)
                                    {
                                        SelectedChange(newstart, Core.SelectedEnd);
                                        ScrollIntoView(newstart);
                                    }
                                    else
                                    {
                                        SelectedChange(Core.SelectedEnd, Core.SelectedEnd);
                                        ScrollIntoView(Core.SelectedEnd);
                                    }
                                }
                            }
                            break;
                        case ModifierKeys.Shift | ModifierKeys.Control:
                            if (selectedstart != null)
                            {
                                if (selectedstart.CompareTo(core.SelectedStart) == 0)
                                {
                                    ITextPosition newend = core.SelectedEnd.NextItem();
                                    if (newend != null)
                                    {
                                        SelectedChange(core.SelectedStart, newend);
                                        ScrollIntoView(newend);
                                    }
                                }
                                else if (selectedstart.CompareTo(core.SelectedEnd) == 0)
                                {
                                    ITextPosition newstart = core.SelectedStart.NextItem();
                                    if (newstart != null && newstart.CompareTo(core.SelectedEnd) <= 0)
                                    {
                                        SelectedChange(newstart, Core.SelectedEnd);
                                        ScrollIntoView(newstart);
                                    }
                                    else
                                    {
                                        SelectedChange(Core.SelectedEnd, Core.SelectedEnd);
                                        ScrollIntoView(Core.SelectedEnd);
                                    }
                                }
                            }
                            break;
                    }
                    break;
                case Key.PageDown:
                    e.Handled = true;
                    ui_stack.PageDown();
                    break;
                case Key.PageUp:
                    e.Handled = true;
                    ui_stack.PageUp();
                    break;
                case Key.Home:
                    e.Handled = true;
                    ui_stack.SetVerticalOffset(0);
                    break;
                case Key.End:
                    e.Handled = true;
                    ui_stack.SetVerticalOffset(ui_stack.ExtentHeight - ui_stack.ViewportHeight);
                    break;
            }
        }

        /// <summary> 键盘提前松开时 </summary>
        /// <param name="e">键盘事件</param>
        protected override void OnPreviewKeyUp(KeyEventArgs e)
        {
            base.OnPreviewKeyUp(e);
        }

        /// <summary> 鼠标左键提前按下时 </summary>
        /// <param name="e">鼠标事件</param>
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (mouseaction == MouseActions.Up)
            {
                mouseaction = MouseActions.Down;
                slideaction = SlideActions.None;
                selectedstart = null;
                CaptureMouse();
            }
            if (mouseaction == MouseActions.Down)
            {
                Keyboard.Focus(this);
                selectedstart = GetTextPosition(e);
                if (selectedstart != null) SelectedChange(selectedstart, selectedstart);
            }
        }

        /// <summary> 鼠标左键提前松开时 </summary>
        /// <param name="e">鼠标事件</param>
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            if (mouseaction == MouseActions.Down)
            {
                mouseaction = MouseActions.Up;
                slideaction = SlideActions.None;
                selectedstart = null;
                ReleaseMouseCapture();
            }
        }

        /// <summary> 鼠标提前移动时 </summary>
        /// <param name="e"></param>
        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            base.OnPreviewMouseMove(e);
            if (mouseaction == MouseActions.Down)
            {
                if (selectedstart != null)
                {
                    IMRATextItemInfo item = GetTextItemInfo(e);
                    ITextPosition tpos = GetTextPosition(e);
                    Point p = e.GetPosition(this);
                    if (tpos != null)
                    {
                        if (tpos.CompareTo(selectedstart) < 0)
                            SelectedChange(tpos, selectedstart);
                        else
                            SelectedChange(selectedstart, tpos);
                    }
                    else if (item != null)
                    {
                        if (p.X < core.View.MarginLeft)
                        {
                            tpos = item.Start;
                            slideaction = SlideActions.Left;
                        }
                        else
                        {
                            tpos = item.End;
                            slideaction = SlideActions.Right;
                        }
                        if (tpos.CompareTo(selectedstart) < 0)
                            SelectedChange(tpos, selectedstart);
                        else
                            SelectedChange(selectedstart, tpos);
                    }
                    else if (p.Y < 0)
                    {
                        slideaction = SlideActions.Up;
                        if (ui_stack != null)
                        {
                            for (int i = (int)(ui_stack.VerticalOffset); i < items.Count(); i++)
                            {
                                if (items[i].View == null) continue;
                                tpos = items[i].Start;
                                if (tpos.CompareTo(selectedstart) < 0)
                                    SelectedChange(tpos, selectedstart);
                                else
                                    SelectedChange(selectedstart, tpos);
                            }
                        }
                    }
                    else if (p.Y > ActualHeight)
                    {
                        slideaction = SlideActions.Down;
                        if (ui_stack != null)
                        {
                            for (int i = Math.Min(items.Count() - 1, (int)(ui_stack.VerticalOffset + ui_stack.ViewportHeight)); i >= 0; i--)
                            {
                                if (items[i].View == null) continue;
                                tpos = items[i].End;
                                if (tpos.CompareTo(selectedstart) < 0)
                                    SelectedChange(tpos, selectedstart);
                                else
                                    SelectedChange(selectedstart, tpos);
                            }
                        }
                    }
                }
            }
            else
            {
                InvalidateCursor(e);
            }
        }

        /// <summary> 鼠标离开时 </summary>
        /// <param name="e"></param>
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
        }

        /// <summary> 鼠标进入时 </summary>
        /// <param name="e"></param>
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            InvalidateCursor(e);
        }

        /// <summary> 定时处理滑动 </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTimerSlide(object sender, EventArgs e)
        {
            switch (slideaction)
            {
                case SlideActions.Down:
                    ui_stack?.LineDown();
                    break;
                case SlideActions.Up:
                    ui_stack?.LineUp();
                    break;
                case SlideActions.Left:
                    ui_stack?.LineLeft();
                    break;
                case SlideActions.Right:
                    ui_stack?.LineRight();
                    break;
            }
        }
        
        #endregion
    }
}
