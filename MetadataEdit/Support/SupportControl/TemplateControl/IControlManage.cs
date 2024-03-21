using System.Windows.Forms;

namespace Com.Boc.Icms.MetadataEdit.Support.SupportControl.TemplateControl
{
    /// <summary>
    /// 构建并显示控件模板中的控件接口
    /// </summary>
    interface IControlManage
    {
        /// <summary>
        /// 外部控件数组
        /// 描述：设置外部控件的Tag(需要符合内部指定Tag规则)；
        /// 内部事件将执行与内部控件统一处理，或者在自定义事件中执行需要的处理
        /// </summary>
        Control[] ExternalControls { get; set; }

        /// <summary>
        /// 内部与外部的控件集合
        /// </summary>
        Control[] ControlCollection { get; }

        /// <summary>
        /// 初始化对象，解析XML控件模板，加载控件
        /// </summary>
        /// <param name="templateXml">XML格式字符串</param>
        void Init(string templateXml);
    }
}
