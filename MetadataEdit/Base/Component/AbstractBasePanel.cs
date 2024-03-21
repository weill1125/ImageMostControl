using System.Windows.Forms;

namespace Com.Boc.Icms.MetadataEdit.Base.Component
{
    /// <summary>
    /// 基容器抽象类
    /// </summary>
    public abstract class AbstractBasePanel
    {
        /// <summary>
        /// 构造一个流布局基容器
        /// </summary>
        /// <param name="parentControl">父容器</param>
        ///  <param name="Name">控件名</param>
        /// <returns></returns>
        public abstract FlowLayoutPanel BuildBasePanel(Control parentControl, string name);
    }
}
