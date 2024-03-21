using System.Windows.Forms;
using Com.Boc.Icms.MetadataEdit.Base.Component;

namespace Com.Boc.Icms.MetadataEdit.Support.SupportControl.PanelControl
{
    /// <summary>
    /// 主容器类
    /// </summary>
    class MainPanel : BasePanel
    {
        /// <summary>
        /// 构造一个控件主容器
        /// </summary>
        /// <param name="parentControl">父容器</param>
        /// <param name="Name">控件名</param>
        /// <returns></returns>
        public override FlowLayoutPanel BuildBasePanel(Control parentControl, string name)
        {
            parentControl.Controls.Clear();
            FlowLayoutPanel flPanel = base.BuildBasePanel(parentControl, name);
            flPanel.FlowDirection = FlowDirection.TopDown;
          
            return flPanel;
        }
    }
}
