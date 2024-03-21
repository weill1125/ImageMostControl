using System;
using System.Collections;
using System.Collections.Generic;

namespace Com.Boc.Icms.MetadataEdit.Base.Event
{
    /// <summary>
    /// 事件抽象类
    /// 如果要定义新的事件，请继承此类，并将事件委托添加到listDelegate
    /// 例如：
    /// 实现点击按钮创建新窗体功能
    /// 定义事件规则：系统事件名_功能名
    /// 1.继承ControlEvent，定义新的事件Click_CreateForm
    /// 2.调用CreateDelegate创建此事件的委托对象，并添加到listDelegate
    /// 3.重写AddEvent，处理规则
    /// （如：根据'系统事件名'搜索按钮事件E1，根据'系统事件名_功能名'搜索自定义的事件E2，将E1与E2进行绑定）
    /// 4.实例化此ControlEvent派生类，调用AddEvent传递参数Click_CreateForm，即可实现控件自绑定
    /// </summary>
    public abstract class ControlEvent : IEnumerable
    {
        protected List<Delegate> ListDelegate = new List<Delegate>();

        public ControlEvent() 
        {
        }

        /// <summary>
        /// 创建事件委托对象
        /// </summary>
        /// <param name="name">自定义事件名</param>
        /// <returns></returns>
        protected Delegate CreateDelegate(string name)
        {
            return Delegate.CreateDelegate(typeof(EventHandler),
                this,
                this.GetType().GetMethod(name));
        }

        /// <summary>
        /// 查找事件委托 
        /// </summary>
        /// <param name="name">自定义系统事件名</param>
        /// <returns></returns>
        public virtual Delegate Find(string name)
        {
            Delegate findDelegate = null;
            name = name.ToUpperInvariant();

            this.ListDelegate.ForEach(a =>
            {
                if (a.Method.Name.ToUpperInvariant() == name)
                {
                    findDelegate = a;
                }
            });

            return findDelegate;
        }

        /*/// <summary>
        /// 绑定控件事件
        /// 重写此类，定义绑定规则
        /// </summary>
        /// <param name="control">控件对象</param>
        /// <param name="name">自定义事件名</param>
        public abstract void AddEvent(System.Windows.Forms.Control control, string name);*/

        public IEnumerator GetEnumerator()
        {
            return this.ListDelegate.GetEnumerator();
        }
    }
}
