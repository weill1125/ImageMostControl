using System.Windows.Forms;

namespace Com.Boc.Icms.MetadataEdit.Base.Component
{
    /// <summary>
    /// 基容器类
    /// </summary>
    public class BasePanel : AbstractBasePanel
    {
        /// <summary>
        /// 构造一个流布局基容器
        /// </summary>
        /// <param name="parentControl">父容器</param>
        /// <param name="Name">控件名</param>
        /// <returns></returns>
        public override FlowLayoutPanel BuildBasePanel(Control parentControl, string name)
        {
            FlowLayoutPanel flPanel = new FlowLayoutPanel();
            flPanel.Name = name;
            flPanel.Dock = DockStyle.Fill;
            flPanel.WrapContents = false;
            flPanel.AutoScroll = true;
            flPanel.AutoSize = false;

            parentControl.Controls.Add(flPanel);

            return flPanel;
        }
    }
}
