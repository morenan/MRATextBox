using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Morenan.MRATextBox.Counselor
{
    /// <summary> MRATextBox的顾问接口 </summary>
    public interface IMRATextBoxCounselor
    {
        /// <summary> 获取所有可行的代码补全项 </summary>
        /// <param name="ictx">输入上下文</param>
        /// <returns>所有项的结果</returns>
        IEnumerable<IMRACltItem> GetCltItems(IMRATextInputContext ictx);

        /// <summary> 输入一段文本后，判断是否开启代码补全 </summary>
        /// <param name="ictx">输入上下文</param>
        /// <param name="inputtext">输入的文本</param>
        /// <returns>是否开启</returns>
        bool CanCltOpen(IMRATextInputContext ictx, string inputtext);

        /// <summary> 输入一段文本后，判断是否关闭代码补全 </summary>
        /// <param name="ictx">输入上下文</param>
        /// <param name="inputtext">输入的文本</param>
        /// <returns>是否关闭</returns>
        bool CanCltClose(IMRATextInputContext ictx, string inputtext);

        /// <summary> 退格删除字符后，判断是否关闭代码补全  </summary>
        /// <param name="ictx">输入上下文</param>
        /// <param name="backtext">退格掉的文本</param>
        /// <returns>是否关闭</returns>
        bool CanCltCloseByBackspace(IMRATextInputContext ictx, string backtext);

        /// <summary> 是否可以对这个域(Zone)进行操作 </summary>
        /// <param name="zctx">域的上下文</param>
        /// <returns>域的回答结果</returns>
        IMRAZoneResult ZoneAction(IMRAZoneContext zctx);
    }
}
