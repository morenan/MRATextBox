using System;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Morenan.MRATextBox.Core;
using Morenan.MRATextBox.Core.Documents;

namespace Morenan.MRATextBox.Counselor
{
    /// <summary> MRATextBox输入环境上下文 </summary>
    internal class MRATextInputContext : IMRATextInputContext
    {
        /// <summary> 构造函数 </summary>
        /// <param name="_zone">当前正在编辑的区域</param>
        /// <param name="_itemindex">正在编辑单词的第几个字符</param>
        /// <param name="_wordindex">正在编辑当前区域的第几个单词</param>
        public MRATextInputContext(ITextZone _zone, int _itemindex, int _wordindex)
        {
            this.zone = _zone;
            this.itemindex = _itemindex;
            this.wordindex = _wordindex;
        }

        #region Number

        /// <summary> 当前正在编辑的区域 </summary>
        private ITextZone zone;
        /// <summary> 当前正在编辑的区域 </summary>
        public ITextZone Zone { get { return this.zone; } }

        /// <summary> 正在编辑单词的第几个字符 </summary>
        private int itemindex;
        /// <summary> 正在编辑单词的第几个字符 </summary>
        public int ItemIndex { get { return this.itemindex; } }

        /// <summary> 正在编辑当前区域的第几个单词 </summary>
        private int wordindex;
        /// <summary> 正在编辑当前区域的第几个单词 </summary>
        public int WordIndex { get { return this.wordindex; } }

        #endregion

        #region IMRATextItem

        string IMRATextItem.Name { get { return "{MRATextInputContext}"; } }

        IMRATextItem IMRATextItem.Parent { get { return null; } }

        MRATextItemTypes IMRATextItem.ItemType { get { return MRATextItemTypes.InputContext; } }

        MRATextItemFeatures IMRATextItem.Feature { get { return MRATextItemFeatures.None; } }
        
        #endregion

        #region IMRATextCollection
        
        public int Count { get { return zone.Items.Count(); } }

        public IMRATextItem this[int index] { get { return new MRATextInputItem(this, zone.Items[index]); } }

        public IEnumerator<IMRATextItem> GetEnumerator()
        {
            foreach (ITextItem item in zone.Items)
                yield return new MRATextInputItem(this, item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region IMRATextInputContext

        /// <summary> 获取上一层的输入上下文 </summary>
        /// <returns>上一层的输入上下文</returns>
        public IMRATextInputContext GetParent()
        {
            return zone.Parent == null ? null : new MRATextInputContext(zone.Parent, zone.ID, -1);
        }

        /// <summary> 获取根部的输入上下文 </summary>
        /// <returns>根部的输入上下文</returns>
        public IMRATextInputContext GetRoot()
        {
            IMRATextInputContext root = GetParent();
            if (root == null) return this;
            IMRATextInputContext _root = root.GetParent();
            while (_root != null) { root = _root; _root = root.GetParent(); }
            return root;
        }

        /// <summary> 获取项的输入上下文 </summary>
        /// <param name="item">指定项</param>
        /// <returns>项的输入上下文</returns>
        public IMRATextInputContext GetContext(IMRATextItem item)
        {
            return item is MRATextInputItem && item.ItemType == MRATextItemTypes.InputContextZone
                ? new MRATextInputContext((ITextZone)(((MRATextInputItem)item).Item), -1, -1) : null;
        }

        /// <summary> 根据xchd文档来匹配实例 </summary>
        /// <param name="docname">xchd文档元素名称</param>
        /// <returns>所有匹配合法的结果</returns>
        public IEnumerable<IMRADocMatchResult> DocMatch(string docname)
        {
            return DocMatch(docname, 0, Count);
        }

        /// <summary> 根据xchd文档来匹配实例（指定检查范围） </summary>
        /// <param name="docname">xchd文档元素名称</param>
        /// <param name="start">检查范围的开始</param>
        /// <param name="count">检查范围的个数</param>
        /// <returns>所有匹配合法的结果</returns>
        public IEnumerable<IMRADocMatchResult> DocMatch(string docname, int start, int count)
        {
            ITextBoxCore core = zone is ITextBoxCore ? (ITextBoxCore)zone : zone.Core;
            IDocsCore doccore = core?.DocsCore;
            if (doccore == null) yield break;
            IDocsItem docitem = doccore.GetItem(docname);
            if (docitem is IDocsCollection)
            {
                int ni = 0;
                for (int i = start; i < start + count; i++)
                {
                    MRADocMatchCollection match = _DocMatchGroup(zone.Items[i], docitem, ref ni);
                    if (match != null) yield return new MRADocMatchResult(match, i, ni - i);
                }
            }
            for (int i = start; i < start + count; i++)
            {
                MRADocMatchItem match = _DocMatch(zone.Items[i], docitem);
                if (match != null) yield return new MRADocMatchResult(match, i, 1);
            }
        }

        /// <summary> 一（文档）对一（实例） </summary>
        /// <param name="textitem">实例文本</param>
        /// <param name="docitem">文档元素</param>
        /// <returns>匹配结果</returns>
        protected static MRADocMatchItem _DocMatch(ITextItem textitem, IDocsItem docitem)
        {
            // 实例是否具有区域性
            bool textiszone = textitem is ITextZone;
            // 文档是否具有区域性
            bool dociszone = docitem is IDocsCollection;
            // 单词一对一
            if (!textiszone && !dociszone)
            {
                if (docitem is IDocsWord)
                {
                    IDocsWord docword = (IDocsWord)docitem;
                    Match match = docword.Regex.Match(textitem.ToString());
                    if (match.Success) return new MRADocMatchWord(docword.Name, textitem);
                    return null;
                }
                if (docitem is IDocsKeyWord)
                {
                    IDocsKeyWord dockeyword = (IDocsKeyWord)docitem;
                    if (dockeyword.Key.Keyword.Equals(textitem.ToString()))
                        return new MRADocMatchWord(dockeyword.Name, textitem);
                    return null;
                }
                return null;
            }
            // 实例区域性
            if (textiszone)
            {
                ITextZone textzone = (ITextZone)textitem;
                // 文档为单个区域块
                if (docitem is IDocsZone)
                {
                    IDocsZone doczone = (IDocsZone)docitem;
                    // 匹配左右括号
                    if (doczone.Left == null || doczone.Right == null) return null;
                    ITextItem textleft = textzone.Items.FirstOrDefault();
                    ITextItem textright = textzone.Items.LastOrDefault();
                    if (!(textleft is ITextKey)) return null;
                    if (!(textright is ITextKey)) return null;
                    ITextKey keyleft = (ITextKey)textleft;
                    ITextKey keyright = (ITextKey)textright;
                    if (keyleft.KeyCore != doczone.Left.Key) return null;
                    if (keyright.KeyCore != doczone.Right.Key) return null;
                    // 内部匹配
                    MRADocMatchCollection matchcolle = new MRADocMatchCollection(doczone.Name, textzone);
                    for (int i = 1, j = 0; i < textzone.Items.Count - 1; i++)
                    {
                        ITextItem sub = textzone.Items[i];
                        if (sub is ITextTrim) continue;
                        // 内部一（文档）对一（实例）匹配
                        {
                            MRADocMatchItem match = _DocMatch(sub, doczone.Items[j]);
                            if (match != null) { matchcolle.Items.Add(match); j++; continue; }
                        }
                        // 内部一（文档）对多（实例）匹配
                        if (doczone.Items[j] is IDocsCollection)
                        {
                            int ni = i + 1;
                            MRADocMatchCollection match = _DocMatchGroup(sub, doczone.Items[j], ref ni);
                            if (match != null) { matchcolle.Items.Add(match); j++; i = ni - 1; continue; }
                        }
                    }
                    if (matchcolle.Items.Count() != doczone.Items.Count()) return null;
                    return matchcolle;
                }
                // 文档为单个区域行
                if (docitem is IDocsLine)
                {
                    IDocsLine docline = (IDocsLine)docitem;
                    if (docline.Left == null) return null;
                    ITextItem textleft = textzone.Items.FirstOrDefault();
                    if (!(textleft is ITextKey)) return null;
                    ITextKey keyleft = (ITextKey)textleft;
                    if (keyleft.KeyCore != docline.Left.Key) return null;
                    MRADocMatchCollection matchcolle = new MRADocMatchCollection(docline.Name, textzone);
                    for (int i = 1, j = 0; i < textzone.Items.Count; i++)
                    {
                        ITextItem sub = textzone.Items[i];
                        if (sub is ITextTrim) continue;
                        MRADocMatchItem match = _DocMatch(sub, docline.Items[j++]);
                        if (match == null) return null;
                        matchcolle.Items.Add(match);
                    }
                    if (matchcolle.Items.Count() != docline.Items.Count()) return null;
                    return matchcolle;
                }
                // 文档为序列组
                if (docitem is IDocsGroup)
                {
                    IDocsGroup docgroup = (IDocsGroup)docitem;
                    MRADocMatchCollection matchcolle = new MRADocMatchCollection(docgroup.Name, textzone);
                    for (int i = 0, j = 0; i < textzone.Items.Count; i++)
                    {
                        ITextItem sub = textzone.Items[i];
                        if (sub is ITextTrim) continue;
                        {
                            MRADocMatchItem match = _DocMatch(sub, docgroup.Items[j]);
                            if (match != null) { matchcolle.Items.Add(match); j++; continue; }
                        }
                        if (docgroup.Items[j] is IDocsCollection)
                        {
                            int ni = i + 1;
                            MRADocMatchCollection match = _DocMatchGroup(sub, docgroup.Items[j], ref ni);
                            if (match != null) { matchcolle.Items.Add(match); j++; i = ni - 1; continue; }
                        }
                        return null;
                    }
                    if (matchcolle.Items.Count() != docgroup.Items.Count()) return null;
                    return matchcolle;
                }
                // 文档为循环组
                if (docitem is IDocsCycle)
                {
                    IDocsCycle doccycle = (IDocsCycle)docitem;
                    MRADocMatchCollection matchcolle = new MRADocMatchCollection(doccycle.Name, textzone);
                    int i = 0, j = doccycle.IgnoreStart;
                    for (; i < textzone.Items.Count; i++)
                    {
                        ITextItem sub = textzone.Items[i];
                        if (sub is ITextTrim) continue;
                        if (j >= doccycle.Items.Count) j = 0;
                        {
                            MRADocMatchItem match = _DocMatch(sub, doccycle.Items[j]);
                            if (match != null) { matchcolle.Items.Add(match); j++; continue; }
                        }
                        if (doccycle.Items[j] is IDocsCollection)
                        {
                            int ni = i + 1;
                            MRADocMatchCollection match = _DocMatchGroup(sub, doccycle.Items[j], ref ni);
                            if (match != null) { matchcolle.Items.Add(match); j++; i = ni - 1; continue; }
                        }
                        return null;
                    }
                    if (j != doccycle.Items.Count - doccycle.IgnoreEnd) return null;
                    return matchcolle;
                }
                return null;
            }
            return null;
        }

        /// <summary> 一（文档）对多（实例） </summary>
        /// <param name="textitem">实例文本</param>
        /// <param name="docitem">文档元素</param>
        /// <param name="nextindex">下一个文本元素的索引</param>
        /// <returns>匹配结果</returns>
        protected static MRADocMatchCollection _DocMatchGroup(ITextItem textitem, IDocsItem docitem, ref int nextindex)
        {
            bool textiszone = textitem is ITextZone;
            bool dociszone = docitem is IDocsCollection;
            if (textiszone) return null;
            if (!dociszone) return null;
            ITextZone textzone = textitem.Parent;
            List<MRADocMatchItem> matchs = new List<MRADocMatchItem>();
            if (docitem is IDocsGroup)
            {
                IDocsGroup docgroup = (IDocsGroup)docitem;
                MRADocMatchCollection matchcolle = new MRADocMatchCollection(docgroup.Name, textzone);
                int i = textitem.ID, j = 0;
                for ( ; i < textzone.Items.Count && j < docgroup.Items.Count; i++)
                {
                    ITextItem sub = textzone.Items[i];
                    if (sub is ITextTrim) continue;
                    {
                        MRADocMatchItem match = _DocMatch(sub, docgroup.Items[j]);
                        if (match != null) { matchcolle.Items.Add(match); j++; continue; }
                    }
                    if (docgroup.Items[j] is IDocsCollection)
                    {
                        int ni = i + 1;
                        MRADocMatchCollection match = _DocMatchGroup(sub, docgroup.Items[j], ref ni);
                        if (match != null) { matchcolle.Items.Add(match); j++; i = ni - 1; continue; }
                    }
                    return null;
                }
                if (matchcolle.Items.Count() != docgroup.Items.Count()) return null;
                nextindex = i;
                return matchcolle;
            }
            if (docitem is IDocsCycle)
            {
                IDocsCycle doccycle = (IDocsCycle)docitem;
                MRADocMatchCollection matchcolle = new MRADocMatchCollection(doccycle.Name, textzone);
                int i = textitem.ID, j = doccycle.IgnoreStart;
                for (; i < textzone.Items.Count; i++)
                {
                    ITextItem sub = textzone.Items[i];
                    if (sub is ITextTrim) continue;
                    if (j == doccycle.Items.Count - doccycle.IgnoreEnd) nextindex = i;
                    if (j >= doccycle.Items.Count) j = 0;
                    {
                        MRADocMatchItem match = _DocMatch(sub, doccycle.Items[j]);
                        if (match != null) { matchcolle.Items.Add(match); j++; continue; }
                    }
                    if (doccycle.Items[j] is IDocsCollection)
                    {
                        int ni = i + 1;
                        MRADocMatchCollection match = _DocMatchGroup(sub, doccycle.Items[j], ref ni);
                        if (match != null) { matchcolle.Items.Add(match); j++; i = ni - 1; continue; }
                    }
                    return null;
                }
                if (j == doccycle.Items.Count - doccycle.IgnoreEnd) nextindex = i;
                if (matchcolle.Items.Count < doccycle.Items.Count - doccycle.IgnoreStart - doccycle.IgnoreEnd) return null;
                int count = matchcolle.Items.Count;
                count -= doccycle.Items.Count - doccycle.IgnoreStart - doccycle.IgnoreEnd;
                count -= count % doccycle.Items.Count;
                count += doccycle.Items.Count - doccycle.IgnoreStart - doccycle.IgnoreEnd;
                matchcolle.Items.RemoveRange(count, matchcolle.Items.Count() - count);
                return matchcolle;
            }
            return null;
        }
        
        #endregion
    }
    
}
