using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace Com.Boc.Icms.MetadataEdit
{
    /// <summary>
    /// 元数据编辑自定义控件类
    /// </summary>
    public partial class MetadataEdit : DockContent
    {
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // CommonControlTemplateWidget
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Name = "CommonControlTemplateWidget";
            this.ResumeLayout(false);
            AutoScaleMode = AutoScaleMode.Dpi;
        }
    }
}
