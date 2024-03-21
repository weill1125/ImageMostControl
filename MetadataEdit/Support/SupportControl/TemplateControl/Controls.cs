using System.Windows.Forms;
using Com.Boc.Icms.MetadataEdit.Base.Component;

namespace Com.Boc.Icms.MetadataEdit.Support.SupportControl.TemplateControl
{
    /// <summary>
    /// 控件类
    /// </summary>
    public class Controls : BaseTemplateControl
    {
        public Controls()
        {
            this.ControlCollection.AddRange(new Control[]{ this.Button, this.TLabel, this.ComboBox, this.TextBox, this.RichTextBox});
        }

        public new Label TLabel
        {
            get
            {
                Label label = new Label();
                label.Text = "Label：";
                return label;
            }
        }
    }
}
