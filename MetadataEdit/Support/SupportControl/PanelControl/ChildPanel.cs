using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using Com.Boc.Icms.MetadataEdit.Base.Component;

namespace Com.Boc.Icms.MetadataEdit.Support.SupportControl.PanelControl
{
    /// <summary>
    /// 子容器类
    /// </summary>
    public class ChildPanel : BasePanel, IEnumerable
    {
        /// <summary>
        /// 控件子容器集合
        /// </summary>
        protected List<FlowLayoutPanel> ListChildPanel = new List<FlowLayoutPanel>();

        /// <summary>
        /// 构造控件子容器，并用指定的控件填充子容器
        /// 描述：XML控件模板中的每个控件都对应一个子容器，并在控件的前面添加此控件的标注名
        /// 即一个面板对应两个控件（Lable对象(标注名)，控件对象）
        /// </summary>
        /// <param name="parentControl">父容器</param>
        /// <param name="name">控件名</param>
        /// <param name="control">待添加到子容器中的控件对象</param>
        public FlowLayoutPanel BuildBasePanel(Control parentControl, string name, Control control)
        {
            FlowLayoutPanel flPanel = this.BuildBasePanel(parentControl, name);

            Label label = new Label();
            label.Name = "Label_" + control.Name;

            if (control.AccessibleName != string.Empty)
                label.Text = control.AccessibleName + "：";

            label.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            label.Width = 160;
            label.Visible = control.Visible;

            flPanel.Controls.AddRange(new[] { (Control)label, control });

            this.ListChildPanel.Add(flPanel);

            return flPanel;
        }

        /// <summary>
        /// 添加多个控件到子容器中
        /// </summary>
        /// <param name="parentControl">父容器</param>
        /// <param name="name">控件名</param>
        /// <param name="control">待添加到子容器中的控件对象数组</param>
        public FlowLayoutPanel BuildBasePanel(Control parentControl, string name, Control[] control)
        {
            FlowLayoutPanel flPanel = this.BuildBasePanel(parentControl, name);

            flPanel.Controls.AddRange(control);

            this.ListChildPanel.Add(flPanel);

            return flPanel;
        }

        /// <summary>
        /// 构造一个空的控件子容器
        /// </summary>
        /// <param name="parentControl">父容器</param>
        /// <param name="Name">控件名</param>
        /// <returns></returns>
        public override FlowLayoutPanel BuildBasePanel(Control parentControl, string name)
        {
            FlowLayoutPanel flPanel = base.BuildBasePanel(parentControl, name);
            flPanel.Dock = DockStyle.None;
            flPanel.FlowDirection = FlowDirection.TopDown;
            flPanel.AutoSize = true;
            flPanel.AutoScroll = false;

            return flPanel;
        }

        public IEnumerator GetEnumerator()
        {
            return this.ListChildPanel.GetEnumerator();
        }
    }
}
