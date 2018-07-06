using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Morenan.MRATextBox.Core.Documents;

namespace Morenan.MRATextBox.Core
{
    internal class TextKeyCore : ITextKeyCore
    {
        #region Resource
        
        #endregion

        public TextKeyCore()
        {
            this.collection = new TextKeyCollection(this);
        }

        #region Number

        private TextKeyFeatures feature;
        public TextKeyFeatures Feature
        {
            get { return this.feature; }
            set { this.feature = value; }
        }

        private TextKeyRelations relation;
        public TextKeyRelations Relation
        {
            get { return this.relation; }
            set { this.relation = value; }
        }
        
        private string keyword;
        public string Keyword
        {
            get { return this.keyword; }
            set { this.keyword = value; }
        }

        private ITextKeyCore that;
        public ITextKeyCore That
        {
            get { return this.that; }
            set { this.that = value; }
        }

        private ITextKeyCore into;
        public ITextKeyCore Into
        {
            get { return this.into; }
            set { this.into = value; }
        }

        private ITextKeyInfo info;
        public ITextKeyInfo Info
        {
            get { return this.info; }
            set { this.info = value; }
        }

        private ITextKeyCollection collection;
        public ITextKeyCollection Collection
        {
            get { return this.collection; }
            set { this.collection = value; }
        }

        private IDocsItem doc;
        public IDocsItem Doc
        {
            get { return this.doc; }
            set { this.doc = value; }
        }
        
        #endregion
    }

    internal enum TextKeyFeatures
    {
        /// <summary> 空特性 </summary>
        None = 0x00000000,
        /// <summary> 域特性 </summary>
        Zone = 0x00000001,
        /// <summary> 域的左标志 </summary>
        ZoneLeft = 0x00000002,
        /// <summary> 域的右标志 </summary>
        ZoneRight = 0x00000004,
        /// <summary> 域具有注释特性，域内当作纯文本对待 </summary>
        ZoneComment = 0x00000008,
        /// <summary> 域具有空间特性，域内声明仅限内部使用 </summary>
        ZoneSpace = 0x00000010,
        /// <summary> 域具有语句容器特性，域内主要由语句构成 </summary>
        ZoneStmt = 0x00000020,
        /// <summary> 域的转义标志，能够取消掉域内部的符号 </summary>
        ZoneTran = 0x00000040,
        /// <summary> 行特性 </summary>
        Line = 0x00000100,
        /// <summary> 行的开始标志 </summary>
        LineStart = 0x00000200,
        /// <summary> 行的跨行延续标志 </summary>
        LineContinue = 0x00000400,
        /// <summary> 行具有注释特性，行内当作纯文本对待 </summary>
        LineComment = 0x00000800,
        /// <summary> 声明特性 </summary>
        Define = 0x00010000,
        /// <summary> 声明的开始标志 </summary>
        DefineStart = 0x00020000,
        /// <summary> 声明的结束标志 </summary>
        DefineEnd = 0x00040000,
        /// <summary> 声明的主体 </summary>
        DefineObject = 0x00080000,
        /// <summary> 声明具有空间特性，仅限当前空间使用 </summary>
        DefineSpace = 0x00100000,
        /// <summary> 声明的连续标志 </summary>
        DefineConnect = 0x00200000,
        /// <summary> 声明的赋值标志 </summary>
        DefineAssign = 0x00400000,
        /// <summary> 语句特性 </summary>
        Stmt = 0x01000000,
        /// <summary> 语句的结束符 </summary>
        StmtEnd = 0x020000000
    }

    internal enum TextKeyRelations
    {
        None = 0x00000000,
        /// <summary> 左侧加入普通字符不会破坏这个关键词的关键词性 </summary>
        LeftWord = 0x00000001,
        /// <summary> 左侧加入关键符号不会破坏这个关键词的关键词性 </summary>
        LeftKey = 0x0000002,
        /// <summary> 右侧加入普通字符不会破坏这个关键词的关键词性 </summary>
        RightWord = 0x00000010,
        /// <summary> 右侧加入关键符号不会破坏这个关键词的关键词性 </summary>
        RightKey = 0x0000020,
        /// <summary> 两侧加入普通字符不会破坏这个关键词的关键词性 </summary>
        SideWord = 0x00000011,
        /// <summary> 两侧加入关键符号不会破坏这个关键词的关键词性 </summary>
        SideKey = 0x00000022,
        /// <summary> 两侧允许所有字符 </summary>
        All = 0x00000033,
        /// <summary> 这个关键词的左侧字符是普通字符 </summary>
        LeftIsWord = 0x00000004,
        /// <summary> 这个关键词的左侧字符是特殊符号 </summary>
        LeftIsKey = 0x00000008,
        /// <summary> 这个关键词的右侧字符是普通字符 </summary>
        RightIsWord = 0x00000040,
        /// <summary> 这个关键词的右侧字符是特殊符号 </summary>
        RightIsKey = 0x00000080,
        /// <summary> 这个关键词由普通字符构成 </summary>
        IsWord = 0x00000044,
        /// <summary> 这个关键词由特殊符号构成 </summary>
        IsKey = 0x00000088
    }
}
