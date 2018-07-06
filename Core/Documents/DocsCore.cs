using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Windows;
using System.Windows.Media;
using System.Text.RegularExpressions;
using System.IO;

namespace Morenan.MRATextBox.Core.Documents
{
    /// <summary> 文档核 </summary>
    internal class DocsCore : IDocsCore
    {
        /// <summary> 构造函数 </summary>
        /// <param name="filepath">读取的文档文件路径</param>
        public DocsCore(string filepath)
        {
            this.name = String.Empty;
            this.extensions = new string[] { };
            this.dict = new Dictionary<string, IDocsItem>();
            this.dictsys = new Dictionary<string, IDocsItem>();
            //this.listgroup = new List<IDocsGroup>();
            InitializeSystemDefault();
            Load(filepath);
        }

        public DocsCore(Stream stream)
        {
            this.name = String.Empty;
            this.extensions = new string[] { };
            this.dict = new Dictionary<string, IDocsItem>();
            this.dictsys = new Dictionary<string, IDocsItem>();
            //this.listgroup = new List<IDocsGroup>();
            InitializeSystemDefault();
            XDocument xdoc = XDocument.Load(stream);
            Load(xdoc);
        }

        #region Number

        /// <summary> 文档名称，一般是支持的语言的名称（例如CPP,Java,Ruby,Python等） </summary>
        private string name;
        /// <summary> 文档名称，一般是支持的语言的名称（例如CPP,Java,Ruby,Python等） </summary>
        public string Name { get { return this.name; } }

        /// <summary> 代码文件扩展名（例如C/C++支持*.c;*.cpp;*.h等） </summary>
        private string[] extensions;
        /// <summary> 代码文件扩展名（例如C/C++支持*.c;*.cpp;*.h等） </summary>
        public string[] Extensions { get { return this.extensions; } }

        /// <summary> 普通项字典 </summary>
        private Dictionary<string, IDocsItem> dict;
        /// <summary> 普通项字典 </summary>
        public IDictionary<string, IDocsItem> Dict { get { return this.dict; } }

        /// <summary> 系统项字典 </summary>
        private Dictionary<string, IDocsItem> dictsys;
        /// <summary> 系统项字典 </summary>
        public IDictionary<string, IDocsItem> DictSys { get { return this.dictsys; } }

        /// <summary> 结构组列表 </summary>
        //private List<IDocsGroup> listgroup;
        /// <summary> 结构组列表 </summary>
        //public IList<IDocsGroup> ListGroup { get { return this.listgroup; } }
        
        #endregion

        #region Method

        /// <summary> 初始化系统默认的设置 </summary>
        protected void InitializeSystemDefault()
        {
            // 代码局部区域（例如C/C++的大括号'{','}'，中间可视作一个内部空间）
            {
                IDocsZone zone_block = new DocsZone("{block_zone}", null, null);
                IDocsKeyWord zone_block_left = new DocsKeyWord("{block_zone_left}", null, zone_block);
                IDocsKeyWord zone_block_right = new DocsKeyWord("{block_zone_right}", null, zone_block);
                ITextKeyCore key_left = new TextKeyCore() { Keyword = "{", Feature = TextKeyFeatures.ZoneLeft, Relation = TextKeyRelations.All };
                ITextKeyCore key_right = new TextKeyCore() { Keyword = "}", Feature = TextKeyFeatures.ZoneRight, Relation = TextKeyRelations.All };
                key_left.That = key_right;
                key_right.That = key_left;
                zone_block_left.Key = key_left;
                zone_block_right.Key = key_right;
                zone_block.Left = zone_block_left;
                zone_block.Right = zone_block_right;
                dictsys.Add("{block_zone}", zone_block);
            }
            // 参数局部区域（例如C/C++的圆括号'(',')'，中间一般存放声明参数或者带入参数）
            {
                IDocsZone zone_params = new DocsZone("{params_zone}", null, null);
                IDocsKeyWord zone_params_left = new DocsKeyWord("{params_zone_left}", null, zone_params);
                IDocsKeyWord zone_params_right = new DocsKeyWord("{params_zone_right}", null, zone_params);
                ITextKeyCore key_left = new TextKeyCore() { Keyword = "(", Feature = TextKeyFeatures.ZoneLeft, Relation = TextKeyRelations.All };
                ITextKeyCore key_right = new TextKeyCore() { Keyword = ")", Feature = TextKeyFeatures.ZoneRight, Relation = TextKeyRelations.All };
                key_left.That = key_right;
                key_right.That = key_left;
                zone_params_left.Key = key_left;
                zone_params_right.Key = key_right;
                zone_params.Left = zone_params_left;
                zone_params.Right = zone_params_right;
                dictsys.Add("{params_zone}", zone_params);
            }
            // 容器索引区域（例如C/C++的方括号'[',']'，中间一般是通过索引获取容器元素的运算符
            {
                IDocsZone zone_array = new DocsZone("{array_zone}", null, null);
                IDocsKeyWord zone_array_left = new DocsKeyWord("{array_zone_left}", null, zone_array);
                IDocsKeyWord zone_array_right = new DocsKeyWord("{array_zone_right}", null, zone_array);
                ITextKeyCore key_left = new TextKeyCore() { Keyword = "[", Feature = TextKeyFeatures.ZoneLeft, Relation = TextKeyRelations.All };
                ITextKeyCore key_right = new TextKeyCore() { Keyword = "]", Feature = TextKeyFeatures.ZoneRight, Relation = TextKeyRelations.All };
                key_left.That = key_right;
                key_right.That = key_left;
                zone_array_left.Key = key_left;
                zone_array_right.Key = key_right;
                zone_array.Left = zone_array_left;
                zone_array.Right = zone_array_right;
                dictsys.Add("{array_zone}", zone_array);
            }
            // 字符区域（例如C/C++的字符边界'，内部为一个字符）
            {
                IDocsZone zone_char = new DocsZone("{char_zone}", null, null);
                IDocsKeyWord zone_char_left = new DocsKeyWord("{char_zone_left}", null, zone_char);
                IDocsKeyWord zone_char_right = new DocsKeyWord("{char_zone_right}", null, zone_char);
                ITextKeyCore key_left = new TextKeyCore() { Keyword = "'", Feature = TextKeyFeatures.ZoneLeft, Relation = TextKeyRelations.All };
                ITextKeyCore key_right = new TextKeyCore() { Keyword = "'", Feature = TextKeyFeatures.ZoneRight, Relation = TextKeyRelations.All };
                key_left.That = key_right;
                key_right.That = key_left;
                zone_char_left.Key = key_left;
                zone_char_right.Key = key_right;
                zone_char.Left = zone_char_left;
                zone_char.Right = zone_char_right;
                dictsys.Add("{char_zone}", zone_char);
            }
            // 字符串区域（例如C/C++的字符串边界"，内部为一个字符串）
            {
                IDocsZone zone_string = new DocsZone("{string_zone}", null, null);
                IDocsKeyWord zone_string_left = new DocsKeyWord("{string_zone_left}", null, zone_string);
                IDocsKeyWord zone_string_right = new DocsKeyWord("{string_zone_right}", null, zone_string);
                ITextKeyCore key_left = new TextKeyCore() { Keyword = "\"", Feature = TextKeyFeatures.ZoneLeft, Relation = TextKeyRelations.All };
                ITextKeyCore key_right = new TextKeyCore() { Keyword = "\"", Feature = TextKeyFeatures.ZoneRight, Relation = TextKeyRelations.All };
                key_left.That = key_right;
                key_right.That = key_left;
                zone_string_left.Key = key_left;
                zone_string_right.Key = key_right;
                zone_string.Left = zone_string_left;
                zone_string.Right = zone_string_right;
                dictsys.Add("{string_zone}", zone_string);
            }
            // 注释区域（例如C/C++的注释边界/*核*/，中间为仅有文本含义的注释）
            {
                IDocsZone zone_comment = new DocsZone("{comment_zone}", null, null);
                IDocsKeyWord zone_comment_left = new DocsKeyWord("{comment_zone_left}", null, zone_comment);
                IDocsKeyWord zone_comment_right = new DocsKeyWord("{comment_zone_right}", null, zone_comment);
                ITextKeyCore key_left = new TextKeyCore() { Keyword = "\"", Feature = TextKeyFeatures.ZoneLeft, Relation = TextKeyRelations.All };
                ITextKeyCore key_right = new TextKeyCore() { Keyword = "\"", Feature = TextKeyFeatures.ZoneRight, Relation = TextKeyRelations.All };
                key_left.That = key_right;
                key_right.That = key_left;
                zone_comment_left.Key = key_left;
                zone_comment_right.Key = key_right;
                zone_comment.Left = zone_comment_left;
                zone_comment.Right = zone_comment_right;
                dictsys.Add("{comment_zone}", zone_comment);
            }
            // 注释行（例如C/C++的注释行开头//，后面整行为注释）
            {
                IDocsLine line_comment = new DocsLine("{comment_line}", null, null);
                ITextKeyCore key_comnment = new TextKeyCore() { Keyword = "//", Relation = TextKeyRelations.All };
            }
            // 通用的运算符（加减乘除移位比较等）
            {
                IDocsKeyWord doc_add = new DocsKeyWord("{key_add}", null, null);
                IDocsKeyWord doc_sub = new DocsKeyWord("{key_sub}", null, null);
                IDocsKeyWord doc_mul = new DocsKeyWord("{key_mul}", null, null);
                IDocsKeyWord doc_div = new DocsKeyWord("{key_div}", null, null);
                IDocsKeyWord doc_less = new DocsKeyWord("{key_less}", null, null);
                IDocsKeyWord doc_great = new DocsKeyWord("{key_great}", null, null);
                IDocsKeyWord doc_equal = new DocsKeyWord("{key_equal}", null, null);
                IDocsKeyWord doc_notequal = new DocsKeyWord("{key_notequal}", null, null);
                IDocsKeyWord doc_lessequal = new DocsKeyWord("{key_lessequal}", null, null);
                IDocsKeyWord doc_greatequal = new DocsKeyWord("{key_greatequal}", null, null);
                IDocsKeyWord doc_shl = new DocsKeyWord("{key_shl}", null, null);
                IDocsKeyWord doc_shr = new DocsKeyWord("{key_shr}", null, null);
                IDocsKeyWord doc_assign = new DocsKeyWord("{key_assign}", null, null);
                IDocsKeyWord doc_not = new DocsKeyWord("{key_not}", null, null);
                IDocsKeyWord doc_and = new DocsKeyWord("{key_and}", null, null);
                IDocsKeyWord doc_or = new DocsKeyWord("{key_or}", null, null);
                IDocsKeyWord doc_xor = new DocsKeyWord("{key_xor}", null, null);
                ITextKeyCore key_add = new TextKeyCore() { Keyword = "+", Relation = TextKeyRelations.All };
                ITextKeyCore key_sub = new TextKeyCore() { Keyword = "-", Relation = TextKeyRelations.All };
                ITextKeyCore key_mul = new TextKeyCore() { Keyword = "*", Relation = TextKeyRelations.All };
                ITextKeyCore key_div = new TextKeyCore() { Keyword = "/", Relation = TextKeyRelations.All };
                ITextKeyCore key_less = new TextKeyCore() { Keyword = "<", Relation = TextKeyRelations.All };
                ITextKeyCore key_great = new TextKeyCore() { Keyword = ">", Relation = TextKeyRelations.All };
                ITextKeyCore key_equal = new TextKeyCore() { Keyword = "==", Relation = TextKeyRelations.All };
                ITextKeyCore key_notequal = new TextKeyCore() { Keyword = "!=", Relation = TextKeyRelations.All };
                ITextKeyCore key_lessequal = new TextKeyCore() { Keyword = "<=", Relation = TextKeyRelations.All };
                ITextKeyCore key_greatequal = new TextKeyCore() { Keyword = ">=", Relation = TextKeyRelations.All };
                ITextKeyCore key_shl = new TextKeyCore() { Keyword = "<<", Relation = TextKeyRelations.All };
                ITextKeyCore key_shr = new TextKeyCore() { Keyword = ">>", Relation = TextKeyRelations.All };
                ITextKeyCore key_assign = new TextKeyCore() { Keyword = "=", Relation = TextKeyRelations.All };
                ITextKeyCore key_not = new TextKeyCore() { Keyword = "~", Relation = TextKeyRelations.All };
                ITextKeyCore key_and = new TextKeyCore() { Keyword = "&", Relation = TextKeyRelations.All };
                ITextKeyCore key_or = new TextKeyCore() { Keyword = "|", Relation = TextKeyRelations.All };
                ITextKeyCore key_xor = new TextKeyCore() { Keyword = "^", Relation = TextKeyRelations.All };
                doc_add.Key = key_add;
                doc_sub.Key = key_sub;
                doc_mul.Key = key_mul;
                doc_div.Key = key_div;
                doc_less.Key = key_less;
                doc_great.Key = key_great;
                doc_equal.Key = key_equal;
                doc_notequal.Key = key_notequal;
                doc_lessequal.Key = key_lessequal;
                doc_greatequal.Key = key_greatequal;
                doc_shl.Key = key_shl;
                doc_shr.Key = key_shr;
                doc_assign.Key = key_assign;
                doc_not.Key = key_not;
                doc_and.Key = key_and;
                doc_or.Key = key_or;
                doc_xor.Key = key_xor;
                dictsys.Add("{key_add}", doc_add);
                dictsys.Add("{key_sub}", doc_sub);
                dictsys.Add("{key_mul}", doc_mul);
                dictsys.Add("{key_div}", doc_div);
                dictsys.Add("{key_less}", doc_less);
                dictsys.Add("{key_great}", doc_great);
                dictsys.Add("{key_equal}", doc_equal);
                dictsys.Add("{key_notequal}", doc_notequal);
                dictsys.Add("{key_lessequal}", doc_lessequal);
                dictsys.Add("{key_greatequal}", doc_greatequal);
                dictsys.Add("{key_shl}", doc_shl);
                dictsys.Add("{key_shr}", doc_shr);
                dictsys.Add("{key_assign}", doc_assign);
                dictsys.Add("{key_not}", doc_not);
                dictsys.Add("{key_and}", doc_and);
                dictsys.Add("{key_or}", doc_or);
                dictsys.Add("{key_xor}", doc_xor);
            }
            // 面向对象支持（C/C++在使用类型成员访问'.'或者指针成员访问'->'时，需要获取访问的类的信息）
            {
                IDocsKeyWord doc_visit_number = new DocsKeyWord("{key_visit_number}", null, null);
                IDocsKeyWord doc_visit_pointer = new DocsKeyWord("{key_visit_pointer}", null, null);
                ITextKeyCore key_visit_number = new TextKeyCore() { Keyword = ".", Relation = TextKeyRelations.All };
                ITextKeyCore key_visit_pointer = new TextKeyCore() { Keyword = "->", Relation = TextKeyRelations.All };
                doc_visit_number.Key = key_visit_number;
                doc_visit_pointer.Key = key_visit_pointer;
                dictsys.Add("{key_visit_number}", doc_visit_number);
                dictsys.Add("{key_visit_pointer}", doc_visit_pointer);
            }
            // 条件/循环控制关键词（C/C++的if/while语句）
            {
                IDocsKeyWord doc_if = new DocsKeyWord("{key_if}", null, null);
                ITextKeyCore key_if = new TextKeyCore() { Keyword = "if", Relation = TextKeyRelations.LeftKey | TextKeyRelations.RightKey };
                doc_if.Key = key_if;
                dictsys.Add("{key_if}", doc_if);
            }
            // 跳出控制关键词（C/C++的break/continue/return/goto语句）
            {
                IDocsKeyWord doc_break = new DocsKeyWord("{key_break}", null, null);
                IDocsKeyWord doc_continue = new DocsKeyWord("{key_continue}", null, null);
                IDocsKeyWord doc_return = new DocsKeyWord("{key_return}", null, null);
                IDocsKeyWord doc_goto = new DocsKeyWord("{key_goto}", null, null);
                ITextKeyCore key_break = new TextKeyCore() { Keyword = "break", Relation = TextKeyRelations.LeftKey | TextKeyRelations.RightKey };
                ITextKeyCore key_continue = new TextKeyCore() { Keyword = "continue", Relation = TextKeyRelations.LeftKey | TextKeyRelations.RightKey };
                ITextKeyCore key_return = new TextKeyCore() { Keyword = "return", Relation = TextKeyRelations.LeftKey | TextKeyRelations.RightKey };
                ITextKeyCore key_goto = new TextKeyCore() { Keyword = "goto", Relation = TextKeyRelations.LeftKey | TextKeyRelations.RightKey };
                doc_break.Key = key_break;
                doc_continue.Key = key_continue;
                doc_return.Key = key_return;
                doc_goto.Key = key_goto;
                dictsys.Add("{key_break}", doc_break);
                dictsys.Add("{key_continue}", doc_continue);
                dictsys.Add("{key_return}", doc_return);
                dictsys.Add("{key_goto}", doc_goto);
            }
            // 基本类型关键词（C/C++的char/bool/short/int/long/float/double等）
            {
                IDocsKeyWord doc_basetype = new DocsKeyWord("{key_basetype}", null, null);
                IDocsKeyWord doc_void = new DocsKeyWord("{key_void}", doc_basetype, null);
                IDocsKeyWord doc_char = new DocsKeyWord("{key_char}", doc_basetype, null);
                IDocsKeyWord doc_bool = new DocsKeyWord("{key_bool}", doc_basetype, null);
                IDocsKeyWord doc_short = new DocsKeyWord("{key_short}", doc_basetype, null);
                IDocsKeyWord doc_int = new DocsKeyWord("{key_int}", doc_basetype, null);
                IDocsKeyWord doc_long = new DocsKeyWord("{key_long}", doc_basetype, null);
                IDocsKeyWord doc_float = new DocsKeyWord("{key_float}", doc_basetype, null);
                IDocsKeyWord doc_double = new DocsKeyWord("{key_double}", doc_basetype, null);
                ITextKeyCore key_void = new TextKeyCore() { Keyword = "bool", Relation = TextKeyRelations.LeftKey | TextKeyRelations.RightKey };
                ITextKeyCore key_char = new TextKeyCore() { Keyword = "char", Relation = TextKeyRelations.LeftKey | TextKeyRelations.RightKey };
                ITextKeyCore key_bool = new TextKeyCore() { Keyword = "bool", Relation = TextKeyRelations.LeftKey | TextKeyRelations.RightKey };
                ITextKeyCore key_short = new TextKeyCore() { Keyword = "short", Relation = TextKeyRelations.LeftKey | TextKeyRelations.RightKey };
                ITextKeyCore key_int = new TextKeyCore() { Keyword = "int", Relation = TextKeyRelations.LeftKey | TextKeyRelations.RightKey };
                ITextKeyCore key_long = new TextKeyCore() { Keyword = "long", Relation = TextKeyRelations.LeftKey | TextKeyRelations.RightKey };
                ITextKeyCore key_float = new TextKeyCore() { Keyword = "float", Relation = TextKeyRelations.LeftKey | TextKeyRelations.RightKey };
                ITextKeyCore key_double = new TextKeyCore() { Keyword = "double", Relation = TextKeyRelations.LeftKey | TextKeyRelations.RightKey };
                doc_void.Key = key_void;
                doc_char.Key = key_char;
                doc_bool.Key = key_bool;
                doc_short.Key = key_short;
                doc_int.Key = key_int;
                doc_long.Key = key_long;
                doc_float.Key = key_float;
                doc_double.Key = key_double;
                dictsys.Add("{key_void}", doc_void);
                dictsys.Add("{key_char}", doc_char);
                dictsys.Add("{key_bool}", doc_bool);
                dictsys.Add("{key_short}", doc_short);
                dictsys.Add("{key_int}", doc_int);
                dictsys.Add("{key_long}", doc_long);
                dictsys.Add("{key_float}", doc_float);
                dictsys.Add("{key_double}", doc_double);
            }
            // 类型定义关键词（C/C++的class/struct）
            {
                IDocsKeyWord doc_class = new DocsKeyWord("{key_class}", null, null);
                IDocsKeyWord doc_struct = new DocsKeyWord("{key_struct}", null, null);
                ITextKeyCore key_class = new TextKeyCore() { Keyword = "class", Relation = TextKeyRelations.LeftKey | TextKeyRelations.RightKey };
                ITextKeyCore key_struct = new TextKeyCore() { Keyword = "struct", Relation = TextKeyRelations.LeftKey | TextKeyRelations.RightKey };
                doc_class.Key = key_class;
                doc_struct.Key = key_struct;
                dictsys.Add("{key_class}", doc_class);
                dictsys.Add("{key_struct}", doc_struct);
            }
            // 语法相关词
            {
                IDocsWord doc_gram_type = new DocsWord("{gram_word_type}", null, null);
                IDocsWord doc_gram_var = new DocsWord("{gram_word_var}", null, null);
                IDocsWord doc_gram_func = new DocsWord("{gram_word_func}", null, null);
                IDocsWord doc_func_para_type = new DocsWord("{gram_func_para_type}", doc_gram_type, null);
                IDocsWord doc_func_para_var = new DocsWord("{gram_func_para_var}", doc_gram_var, null);
                dictsys.Add("{gram_word_type}", doc_gram_type);
                dictsys.Add("{gram_word_var}", doc_gram_var);
                dictsys.Add("{gram_word_func}", doc_gram_func);
                dictsys.Add("{gram_func_para_type}", doc_func_para_type);
                dictsys.Add("{gram_func_para_var}", doc_func_para_var);
            }
            // 语法相关结构
            {
                IDocsZone doc_class_define = new DocsZone("{class_define}", null, null);
                IDocsZone doc_func_params = new DocsZone("{gram_func_params}", null, null);
                IDocsZone doc_class_block = new DocsZone("{class_block_zone}", null, null);
                dictsys.Add("{class_define}", doc_class_define);
                dictsys.Add("{gram_func_params}", doc_func_params);
                dictsys.Add("{class_block_zone}", doc_class_block);
            }
        }

        /// <summary> 读取本地文档 </summary>
        /// <param name="xchdfile">文件路径</param>
        public void Load(string xchdfile)
        {
            XDocument xdoc = XDocument.Load(xchdfile);
            Load(xdoc);
        }

        public void Load(XDocument xdoc)
        {
            XElement xroot = xdoc.Root;
            XAttribute xattr = null;
            XElement xele = null;
            // 代码名称，扩展名
            {
                xattr = xroot.Attribute("name");
                if (xattr != null) name = xattr.Value;
                xattr = xroot.Attribute("extensions");
                if (xattr != null) extensions = xattr.Value.Split(';');
            }
            // 资源，定义，语法，补全系统
            {
                xele = xroot.Element("Resources");
                if (xele != null) LoadResource(xele);
                xele = xroot.Element("Definitions");
                if (xele != null) LoadDefinition(xele);
                xele = xroot.Element("Grammers");
                if (xele != null) LoadGrammer(xele);
                foreach (XElement xi in xele.Elements("Completation"))
                    LoadCompletation(xi);
            }
        }

        /// <summary> 读取所有资源 </summary>
        /// <param name="xele"></param>
        protected void LoadResource(XElement xele)
        {
            foreach (XElement xsub in xele.Elements())
                LoadItem(xsub, LoadEnvs.Resource);
        }

        /// <summary> 读取所有定义 </summary>
        /// <param name="xele"></param>
        protected void LoadDefinition(XElement xele)
        {
            foreach (XElement xsub in xele.Elements())
                LoadItem(xsub, LoadEnvs.Definition);
        }

        /// <summary> 读取所有语法 </summary>
        /// <param name="xele"></param>
        protected void LoadGrammer(XElement xele)
        {
            foreach (XElement xsub in xele.Elements())
                LoadItem(xsub, LoadEnvs.Grammer);
        }

        /// <summary> 读取所有代码补全系统 </summary>
        /// <param name="xele"></param>
        protected void LoadCompletation(XElement xele)
        {

        }

        /// <summary> 读取具有集合接口的文档元素内部的子元素 </summary>
        /// <param name="colle">集合接口的文档</param>
        /// <param name="xchildren">所有子元素的xml文档集合</param>
        /// <param name="env">读取环境</param>
        protected void LoadItemCollection(IDocsCollection colle, IEnumerable<XElement> xchildren, LoadEnvs env)
        {
            if (xchildren.Count() > 0) colle.Items = new List<IDocsItem>();
            foreach (XElement xc in xchildren)
            {
                IDocsItem docitem = LoadItem(xc, env);
                if (docitem != null) colle.Items.Add(docitem);
            }
        }

        /// <summary> 项读取的环境 </summary>
        protected enum LoadEnvs { Resource, Definition, Grammer, KeyWordCollection, Zone };

        /// <summary> 读取一个文档项 </summary>
        /// <param name="xele">Xml元素</param>
        /// <param name="env">读取环境</param>
        /// <returns>读取到的文档项</returns>
        protected IDocsItem LoadItem(XElement xele, LoadEnvs env)
        {
            XAttribute xattr = null;
            string name = null;
            string[] baseon = null;
            string parent = null;
            IDocsItem item = null;
            IDocsItem baseonitem = null;
            IDocsItem parentitem = null;
            // 基本属性（名称，基类，父亲）
            {
                xattr = xele.Attribute("name");
                if (xattr != null) name = xattr.Value;
                xattr = xele.Attribute("baseon");
                if (xattr != null) baseon = xattr.Value.Split(';');
                xattr = xele.Attribute("parent");
                if (xattr != null) parent = xattr.Value;
            }
            // 名称为空，不用新建项而直接获取现有项
            if (name == null)
            {
                if (baseon == null) return null;
                foreach (string b in baseon)
                {
                    if (b[0] == '{' && b.Last() == '}') continue;
                    dict.TryGetValue(b, out item);
                    if (item != null) break;
                }
                if (item == null)
                {
                    foreach (string b in baseon)
                    {
                        if (b[0] != '{' || b.Last() != '}') continue;
                        dictsys.TryGetValue(b, out item);
                        if (item != null) break;
                    }
                }
                //if (item != null) name = item.Name;
            }
            // 基类不为空
            else if (baseon != null)
            {
                // 优先获取用户添加的作为基类
                foreach (string b in baseon)
                {
                    if (b[0] == '{' && b.Last() == '}') continue;
                    dict.TryGetValue(b, out baseonitem);
                    if (baseonitem != null) break;
                }
                // 其次才是系统内部的
                if (baseonitem == null)
                {
                    foreach (string b in baseon)
                    {
                        if (b[0] != '{' || b.Last() != '}') continue;
                        dictsys.TryGetValue(b, out baseonitem);
                        while (baseonitem != null && !baseonitem.Name.Equals(b))
                            baseonitem = baseonitem.BaseOn;
                        if (baseonitem != null) break;
                    }
                }
            }
            // 父亲不为空
            if (parent != null)
            {
                parentitem = GetItem(parent);
            }
            // 根据xml元素名称来决定新建项的类型
            if (item == null)
            {
                switch (xele.Name.LocalName)
                {
                    case "Color":
                        {
                            IDocsColor color = new DocsColor(name, baseonitem, parentitem);
                            xattr = xele.Attribute("color");
                            if (xattr != null) color.Foreground = GetColor(xattr.Value);
                            xattr = xele.Attribute("fill");
                            if (xattr != null) color.Background = GetColor(xattr.Value);
                            xattr = xele.Attribute("weight");
                            if (xattr != null) color.FontWeight = GetWeight(xattr.Value);
                            xattr = xele.Attribute("style");
                            if (xattr != null) color.FontStyle = GetStyle(xattr.Value);
                            xattr = xele.Attribute("stretch");
                            if (xattr != null) color.FontStretch = GetStretch(xattr.Value);
                            xattr = xele.Attribute("size");
                            if (xattr != null) color.FontSize = double.Parse(xattr.Value);
                            xattr = xele.Attribute("font");
                            if (xattr != null) color.FontFamily = new FontFamily(xattr.Value);
                            item = color;
                        }
                        break;
                    case "Keywords":
                        {
                            IDocsKeyWordCollection kwcl = new DocsKeyWordCollection(name, baseonitem, parentitem);
                            List<IDocsKeyWord> kws = new List<IDocsKeyWord>();
                            TextKeyFeatures features = TextKeyFeatures.None;
                            TextKeyRelations relation = TextKeyRelations.None;
                            IDocsColor color = null;
                            xattr = xele.Attribute("color");
                            if (xattr != null) color = (IDocsColor)GetItem(xattr.Value); 
                            xattr = xele.Attribute("features");
                            if (xattr != null) features = GetTextKeyFeatures(xattr.Value);
                            xattr = xele.Attribute("relation");
                            if (xattr != null) relation = GetTextKeyRelation(xattr.Value);
                            foreach (XElement xkw in xele.Elements("Word"))
                            {
                                IDocsItem _kw = LoadItem(xkw, LoadEnvs.KeyWordCollection);
                                if (!(_kw is IDocsKeyWord)) continue;
                                IDocsKeyWord kw = (IDocsKeyWord)_kw;
                                kws.Add(kw);
                                xattr = xkw.Attribute("color");
                                if (xattr == null) kw.Fill = color;
                                xattr = xkw.Attribute("features");
                                if (xattr == null) kw.Key.Feature = features;
                                xattr = xkw.Attribute("relation");
                                if (xattr == null) kw.Key.Relation = relation;
                                kw.Parent = kwcl;
                            }
                            kwcl.KeyWords = kws;
                            item = kwcl;
                        }
                        break;
                    case "Zone":
                        {
                            IDocsZone zone = new DocsZone(name, baseonitem, parentitem);
                            XElement xleft = xele.Element("Left");
                            XElement xright = xele.Element("Right");
                            string attribute = String.Empty;
                            xattr = xele.Attribute("attribute");
                            if (xattr != null) attribute = xattr.Value;
                            xattr = xele.Attribute("fill");
                            if (xattr != null) zone.Fill = (IDocsColor)GetItem(xattr.Value);
                            if (xleft != null)
                            {
                                foreach (XElement xlc in xleft.Elements())
                                {
                                    IDocsItem li = LoadItem(xlc, LoadEnvs.Zone);
                                    if (li is IDocsKeyWord)
                                    {
                                        zone.Left = (IDocsKeyWord)li;
                                        li.Parent = zone;
                                    }
                                }
                            }
                            if (xright != null)
                            {
                                foreach (XElement xrc in xright.Elements())
                                {
                                    IDocsItem ri = LoadItem(xrc, LoadEnvs.Zone);
                                    if (ri is IDocsKeyWord)
                                    {
                                        zone.Right = (IDocsKeyWord)ri;
                                        ri.Parent = zone;
                                    }
                                }
                            }
                            if (zone.Left != null)
                            {
                                zone.Left.Key.Feature |= TextKeyFeatures.ZoneLeft;
                                if (attribute.Contains("comment")) zone.Left.Key.Feature |= TextKeyFeatures.ZoneComment;
                            }
                            if (zone.Right != null)
                            {
                                zone.Right.Key.Feature |= TextKeyFeatures.ZoneRight;
                                if (attribute.Contains("comment")) zone.Right.Key.Feature |= TextKeyFeatures.ZoneComment;
                            }
                            if (zone?.Left.Key != null && zone?.Right.Key != null)
                            {
                                zone.Left.Key.That = zone.Right.Key;
                                zone.Right.Key.That = zone.Left.Key;
                            }
                            if (env == LoadEnvs.Grammer)
                            {
                                IEnumerable<XElement> xchildren = xele.Elements().Where(xc => xc != xleft && xc != xright);
                                LoadItemCollection(zone, xchildren, env);
                            }
                            item = zone;
                        }
                        break;
                    case "Line":
                        {
                            IDocsLine line = new DocsLine(name, baseonitem, parentitem);
                            XElement xleft = xele.Element("Left");
                            string attribute = String.Empty;
                            xattr = xele.Attribute("attribute");
                            if (xattr != null) attribute = xattr.Value;
                            xattr = xele.Attribute("fill");
                            if (xattr != null) line.Fill = (IDocsColor)GetItem(xattr.Value);
                            if (xleft != null)
                            {
                                foreach (XElement xlc in xleft.Elements())
                                {
                                    IDocsItem li = LoadItem(xlc, LoadEnvs.Zone);
                                    if (li is IDocsKeyWord)
                                    {
                                        line.Left = (IDocsKeyWord)li;
                                        li.Parent = line;
                                    }
                                }
                            }
                            if (line.Left != null)
                            {
                                line.Left.Key.Feature |= TextKeyFeatures.LineStart;
                                if (attribute.Contains("comment")) line.Left.Key.Feature |= TextKeyFeatures.LineComment;
                            }
                            if (env == LoadEnvs.Grammer)
                            {
                                IEnumerable<XElement> xchildren = xele.Elements().Where(xc => xc != xleft);
                                LoadItemCollection(line, xchildren, env);
                            }
                            item = line;
                        }
                        break;
                    case "Group":
                        {
                            IDocsGroup group = new DocsGroup(name, baseonitem, parentitem);
                            if (env == LoadEnvs.Grammer)
                            {
                                IEnumerable<XElement> xchildren = xele.Elements();
                                LoadItemCollection(group, xchildren, env);
                            }
                            //listgroup.Add(group);
                            item = group;
                        }
                        break;
                    case "Cycle":
                        {
                            IDocsCycle cycle = new DocsCycle(name, baseonitem, parentitem);
                            List<IDocsItem> items = new List<IDocsItem>();
                            int ignorestart = 0;
                            int ignoreend = 0;
                            foreach (XElement xitem in xele.Elements())
                            {
                                IDocsItem subitem = LoadItem(xitem, LoadEnvs.KeyWordCollection);
                                items.Add(subitem);
                                xattr = xitem.Attribute("showstart");
                                if (xattr != null) ignorestart++;
                                xattr = xitem.Attribute("showend");
                                if (xattr != null) ignoreend++;
                            }
                            cycle.Items = items;
                            cycle.IgnoreStart = ignorestart;
                            cycle.IgnoreEnd = ignoreend;
                            item = cycle;
                        }
                        break;
                    case "Word":
                        switch (env)
                        {
                            case LoadEnvs.KeyWordCollection:
                                {
                                    IDocsKeyWord kw = new DocsKeyWord(name, baseonitem, parentitem);
                                    kw.Key = new TextKeyCore() { Keyword = xele.Value, Doc = kw };
                                    if (kw.Key.Keyword.Length == 0)
                                    {
                                        xattr = xele.Attribute("text");
                                        if (xattr != null) kw.Key.Keyword = xattr.Value;
                                    }
                                    xattr = xele.Attribute("color");
                                    if (xattr != null) kw.Fill = (IDocsColor)GetItem(xattr.Value);
                                    xattr = xele.Attribute("features");
                                    if (xattr != null) kw.Key.Feature = GetTextKeyFeatures(xattr.Value);
                                    xattr = xele.Attribute("relation");
                                    if (xattr != null) kw.Key.Relation = GetTextKeyRelation(xattr.Value);
                                    item = kw;
                                }
                                break;
                            default:
                                {
                                    IDocsWord word = new DocsWord(name, baseonitem, parentitem);
                                    xattr = xele.Attribute("regex");
                                    if (xattr != null) word.Regex = new Regex(xattr.Value);
                                    item = word;
                                }
                                break;
                        }
                        break;
                    case "Keyword":
                        {
                            IDocsKeyWord kw = new DocsKeyWord(name, baseonitem, parentitem);
                            kw.Key = new TextKeyCore() { Keyword = xele.Value, Doc = kw};
                            if (kw.Key.Keyword.Length == 0)
                            {
                                xattr = xele.Attribute("text");
                                if (xattr != null) kw.Key.Keyword = xattr.Value;
                            }
                            xattr = xele.Attribute("color");
                            if (xattr != null) kw.Fill = (IDocsColor)GetItem(xattr.Value);
                            xattr = xele.Attribute("features");
                            if (xattr != null) kw.Key.Feature = GetTextKeyFeatures(xattr.Value);
                            xattr = xele.Attribute("relation");
                            if (xattr != null) kw.Key.Relation = GetTextKeyRelation(xattr.Value);
                            item = kw;
                        }
                        break;
                    case "Completation":
                        {
                            IDocsCompletation comp = new DocsCompletation(name, baseonitem, parentitem);
                            item = comp;
                        }
                        break;
                }
            }
            // 将新建的项加入到字典中
            if (item != null && !dict.ContainsKey(item.Name))
            {
                dict.Add(item.Name, item);
            }
            // 如果有指定系统项作为基类，覆盖系统字典
            if (item != null && baseon != null)
            {
                foreach (string b in baseon)
                {
                    if (b[0] != '{' || b.Last() != '}') continue;
                    if (dictsys.ContainsKey(b)) dictsys[b] = item;
                }
            }
            return item;
        }

        /// <summary> 根据名称获取项（带大括号{}为系统项名称） </summary>
        /// <param name="name">名称</param>
        /// <returns>获取到的项</returns>
        public IDocsItem GetItem(string name)
        {
            IDocsItem item = null;
            if (name.Length == 0) return null;
            if (name[0] == '{' && name.Last() == '}')
                dictsys.TryGetValue(name, out item);
            else
                dict.TryGetValue(name, out item);
            return item;
        }

        /// <summary> 根据ARGB字符串格式(#FF00FF00)获取颜色</summary>
        /// <param name="argb">ARGB字符串</param>
        /// <returns>WPF颜色</returns>
        public static Color GetColor(string argb)
        {
            try
            {
                return Color.FromArgb(
                    byte.Parse(argb.Substring(1, 2), System.Globalization.NumberStyles.HexNumber),
                    byte.Parse(argb.Substring(3, 2), System.Globalization.NumberStyles.HexNumber),
                    byte.Parse(argb.Substring(5, 2), System.Globalization.NumberStyles.HexNumber),
                    byte.Parse(argb.Substring(7, 2), System.Globalization.NumberStyles.HexNumber));
            }
            catch (Exception)
            {
                return Colors.Transparent;
            }
        }

        /// <summary> 获取文本粗细度 </summary>
        /// <param name="name">属性信息</param>
        /// <returns>文本粗细度</returns>
        public static FontWeight GetWeight(string name)
        {
            try
            {
                switch (name)
                {
                    case "normal":
                        return FontWeights.Normal;
                    case "bold":
                        return FontWeights.Bold;
                    case "light":
                        return FontWeights.Light;
                    default:
                        return FontWeight.FromOpenTypeWeight(int.Parse(name));
                }
            }
            catch (Exception)
            {
                return FontWeights.Normal;
            }
        }

        /// <summary> 获取文本横纵比 </summary>
        /// <param name="name">属性信息</param>
        /// <returns>文本横纵比</returns>
        public static FontStretch GetStretch(string name)
        {
            try
            {
                switch (name)
                {
                    case "normal":
                        return FontStretches.Normal;
                    case "expand":
                        return FontStretches.Expanded;
                    case "condensed":
                        return FontStretches.Condensed;
                    default:
                        return FontStretch.FromOpenTypeStretch(int.Parse(name));
                }
            }
            catch (Exception)
            {
                return FontStretches.Normal;
            }
        }

        /// <summary> 获取文本风格 </summary>
        /// <param name="name">属性信息</param>
        /// <returns>文本风格</returns>
        public static FontStyle GetStyle(string name)
        {
            try
            {
                switch (name)
                {
                    case "normal":
                        return FontStyles.Normal;
                    case "italic":
                        return FontStyles.Italic;
                    case "oblique":
                        return FontStyles.Oblique;
                    default:
                        return FontStyles.Normal;
                }
            }
            catch (Exception)
            {
                return FontStyles.Normal;
            }
        }

        /// <summary> 获取关键字特性 </summary>
        /// <param name="name">属性信息</param>
        /// <returns>关键字特性</returns>
        public static TextKeyFeatures GetTextKeyFeatures(string name)
        {
            string[] paras = name.Split(';');
            TextKeyFeatures features = TextKeyFeatures.None;
            foreach (string para in paras)
            {
                TextKeyFeatures feature = TextKeyFeatures.None;
                Enum.TryParse(para, true, out feature);
                features |= feature;
            }
            return features;
        }

        /// <summary> 获取关键字关联性 </summary>
        /// <param name="name">属性信息</param>
        /// <returns>关键字关联性</returns>
        public static TextKeyRelations GetTextKeyRelation(string name)
        {
            string[] paras = name.Split(';');
            TextKeyRelations relations = TextKeyRelations.None;
            foreach (string para in paras)
            {
                TextKeyRelations relation = TextKeyRelations.None;
                Enum.TryParse(para, true, out relation);
                relations |= relation;
            }
            return relations;
        }

        #endregion
    }
}
