using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Morenan.MRATextBox.Counselor
{
    #region Input (TextItem)

    /// <summary> 对顾问的描述输入项 </summary>
    public interface IMRATextItem
    {
        /// <summary> 项名称，一般对应xchd文档的元素名称 </summary>
        string Name { get; }
        /// <summary> 项的父亲，一般对应xchd文档的元素的上一层元素 </summary>
        IMRATextItem Parent { get; }
        /// <summary> 项的类型 </summary>
        MRATextItemTypes ItemType { get; }
        /// <summary> 项的特性 </summary>
        MRATextItemFeatures Feature { get; }
    }

    /// <summary> 单词项，对于输入上下文对应原文本的单词项，对于doc匹配结果对应xchd文档中的单词元素，通过ToString获得单词的具体文本 </summary>
    public interface IMRATextWord : IMRATextItem
    {
    }

    /// <summary> 集合项，对于输入上下文对应原文本的区域，对于doc匹配结果对应xchd文档中的容器元素 </summary>
    public interface IMRATextCollection : IMRATextItem, IEnumerable<IMRATextItem>
    {
        /// <summary> 列表大小 </summary>
        int Count { get; }
        /// <summary> 列表访问 </summary>
        /// <param name="index">索引</param>
        /// <returns>项</returns>
        IMRATextItem this[int index] { get; }
    }

    /// <summary> 对顾问的输入上下文 </summary>
    public interface IMRATextInputContext : IMRATextCollection
    {
        /// <summary> 正在输入第几个项（从0开始，即列表的索引） </summary>
        int ItemIndex { get; }
        /// <summary> 正在输入当前单词的第几个char </summary>
        int WordIndex { get; }
        /// <summary> 获取上一层的输入上下文 </summary>
        /// <returns>上一层的输入上下文</returns>
        IMRATextInputContext GetParent();
        /// <summary> 获取根部的输入上下文 </summary>
        /// <returns>根部的输入上下文</returns>
        IMRATextInputContext GetRoot();
        /// <summary> 获取项的输入上下文 </summary>
        /// <param name="item">指定项</param>
        /// <returns>项的输入上下文</returns>
        IMRATextInputContext GetContext(IMRATextItem item);
        /// <summary> 根据xchd文档来匹配实例 </summary>
        /// <param name="docname">xchd文档元素名称</param>
        /// <returns>所有匹配合法的结果</returns>
        IEnumerable<IMRADocMatchResult> DocMatch(string docname);
        /// <summary> 根据xchd文档来匹配实例（指定检查范围） </summary>
        /// <param name="docname">xchd文档元素名称</param>
        /// <param name="start">检查范围的开始</param>
        /// <param name="count">检查范围的个数</param>
        /// <returns>所有匹配合法的结果</returns>
        IEnumerable<IMRADocMatchResult> DocMatch(string docname, int start, int count);
    }

    /// <summary> Doc文档匹配的结果 </summary>
    public interface IMRADocMatchResult
    {
        /// <summary> 匹配结果项 </summary>
        IMRATextItem Result { get; }
        /// <summary> 对应上下文的索引开始 </summary>
        int Start { get; }
        /// <summary> 对应上下文的长度 </summary>
        int Count { get; }
    }
    
    #endregion
    
    #region Output (Completation : Clt)

    /// <summary> 代码补全系统（Clt）的指示项 </summary>
    public interface IMRACltItem
    {
        /// <summary> 名称 </summary>
        string Name { get; }
        /// <summary> 补全单词 </summary>
        string Text { get; }
        /// <summary> 注释 </summary>
        string Comment { get; }
        /// <summary> 类型 </summary>
        MRACltItemTypes ItemType { get; }
        /// <summary> 特性 </summary>
        MRACltItemFeatures Feature { get; }
    }

    /// <summary> 代码补全系统（Clt）的数据类型项 </summary>
    public interface IMRACltType : IMRACltItem
    {
    }

    /// <summary> 代码补全系统（Clt）的可用变量项 </summary>
    public interface IMRACltVar : IMRACltItem
    {
        /// <summary> 变量的数据类型 </summary>
        IMRACltType Type { get; }
    }

    /// <summary> 代码补全系统（Clt）的调用函数项 </summary>
    public interface IMRACltFunc : IMRACltItem
    {
        /// <summary> 函数的返回类型 </summary>
        IMRACltType Type { get; }
        /// <summary> 函数的参数列表 </summary>
        IList<IMRACltVar> Params { get; }
    }

    /// <summary> 代码补全系统（Clt）的调用函数正在输入的上下文 </summary>
    public interface IMRACltFuncParamsContext : IMRACltFunc
    {
        /// <summary> 正在输入第几个参数，从0开始 </summary>
        int Index { get; }
    }

    #endregion

    #region Zone

    public interface IMRAZoneContext
    {
        string Name { get; }
        MRAZoneAction Action { get; }
        IMRATextInputContext InputContext { get; }
    }

    public interface IMRAZoneResult
    {
        bool Cancel { get; }
    }

    public interface IMRAZoneSkipResult
    {
        int ShowStart { get; }
        int ShowEnd { get; }
    }

    #endregion

}
