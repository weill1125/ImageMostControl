using System;
using Com.Boc.Icms.MetadataEdit.Support.ProviderEvent;

namespace Com.Boc.Icms.MetadataEdit.Business.CustomizeEvent
{
    /// <summary>
    /// CustomEvent控件事件类
    /// 事件规则：系统事件名_功能名
    /// 继承ButtonEvent，CustomEvent事件命名规则同ButtonEvent
    /// </summary>
    public class CustomEvent : ButtonEvent
    {
        public event EventHandler LostFocus;

        public CustomEvent()
            : base()
        {
            this.ListDelegate.Add(this.CreateDelegate("LostFocus_Validate"));
        }

        public virtual void LostFocus_Validate(object sender, EventArgs e)
        {
            this.LostFocus(sender, e);
        }
    }
}
