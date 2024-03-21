using System;
using Com.Boc.Icms.MetadataEdit.Base.Event;

namespace Com.Boc.Icms.MetadataEdit.Support.ProviderEvent
{
    /// <summary>
    /// 按钮事件类
    /// 事件规则：系统事件名_功能名
    /// 例如：
    /// 实现点击按钮创建新窗体功能
    /// 1.继承此类，定义新的事件Click_CreateForm
    /// 2.调用CreateDelegate创建此事件的委托对象，并添加到listDelegate
    /// 3.实例化此类，调用AddEvent传递参数Click_CreateForm，即可实现控件自绑定
    /// </summary>
    public class ButtonEvent : ControlEvent
    {
        public event EventHandler ClickOk;
        public event EventHandler ClickNo;
        public event EventHandler ClickCancel;

        public ButtonEvent()
            : base()
        {
            this.ListDelegate.Add(this.CreateDelegate("Click_OK"));
            this.ListDelegate.Add(this.CreateDelegate("Click_NO"));
            this.ListDelegate.Add(this.CreateDelegate("Click_Cancel"));
        }

        public virtual void Click_OK(object sender, EventArgs e)
        {
            this.ClickOk(sender, e);
        }

        public virtual void Click_NO(object sender, EventArgs e)
        {
            this.ClickNo(sender, e);
        }

        public virtual void Click_Cancel(object sender, EventArgs e)
        {
            this.ClickCancel(sender, e);
        }

        public override void AddEvent(System.Windows.Forms.Control control, string name)
        {
            string sysEvent = System.Text.RegularExpressions.Regex.Replace(name, @"_.*$", "");
            control.GetType().GetEvent(sysEvent).AddEventHandler(control, this.Find(name));
        }
    }
}
