using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Com.Boc.Icms.MetadataEdit.Base.Component;
using Com.Boc.Icms.MetadataEdit.Base.Event;
using Com.Boc.Icms.MetadataEdit.Support.SupportControl.PanelControl;
using Com.Boc.Icms.MetadataEdit.Support.Template;

namespace Com.Boc.Icms.MetadataEdit.Support.SupportControl.TemplateControl
{
    /// <summary>
    /// 构建并显示控件模板中的控件
    /// </summary>
    public class ControlManage : IControlManage
    {
        private readonly BaseTemplateControl _btControl;
        private readonly ControlEvent _controlEvent;
        private XmlTemplateOperate _xtOperate;
        private readonly ContainerControl _topMostControl;

        private Control[] _externalControls;

        /// <summary>
        /// 外部控件数组
        /// 描述：设置外部控件的Tag(需要符合内部指定Tag规则)；
        /// 内部事件将执行与内部控件统一处理，或者在自定义事件中执行需要的处理
        /// </summary>
        public Control[] ExternalControls
        {
            get { return this._externalControls; }
            set { this._externalControls = value; }
        }

        private Control[] _controlCollection;

        /// <summary>
        /// 内部与外部的控件集合
        /// </summary>
        public Control[] ControlCollection
        {
            get { return this._controlCollection; }
        }

        /// <summary>
        ///  构建并显示控件模板中的控件构造函数
        /// </summary>
        /// <param name="topMostControl">顶级面板容器(模板控件的容器)</param>
        /// <param name="controlEvent">模板控件的事件类对象</param>
        /// <param name="baseTemplateControl">控件类</param>
        public ControlManage(ContainerControl topMostControl, ControlEvent controlEvent, BaseTemplateControl baseTemplateControl)
        {
            this._topMostControl = topMostControl;
            this._controlEvent = controlEvent;
            this._btControl = baseTemplateControl;
        }

        /// <summary>
        /// 初始化对象，解析XML控件模板，加载控件
        /// </summary>
        /// <param name="templateXml">XML格式字符串</param>
        public void Init(string templateXml)
        {
            this._xtOperate = new XmlTemplateOperate(this._btControl, this._controlEvent, templateXml);

            //加载控件
            this.InitXmlControl(this._xtOperate);
        }

        /// <summary>
        /// 初始化面板并填充控件
        /// </summary>
        /// <param name="xtOperate">XML处理类对象</param>
        /// <param name="docID">文档ID</param>
        private void InitXmlControl(XmlTemplateOperate xtOperate)
        {
            //获取顶级节点的限定名
            string rootName = xtOperate.Root.Name;

            //构建顶级面板
            MainPanel mPanel = new MainPanel();
            FlowLayoutPanel flPanel = mPanel.BuildBasePanel(this._topMostControl, rootName);
            xtOperate.SetControlTag(flPanel, xtOperate.Root);

            //根据控件数组创建子面板并填充控件
            ChildPanel cPanel = new ChildPanel();
            Control[] controlArray = xtOperate.GetControlCollection();
            foreach (Control control in controlArray)
            {
                cPanel.BuildBasePanel(flPanel, rootName + "_" + control.Name, control);
            }

            //判断是否处理外部控件
            if (this._externalControls != null)
            {
                List<Control> listControl = controlArray.ToList();
                listControl.AddRange(this._externalControls.ToList());
                controlArray = listControl.ToArray();
            }

            this._controlCollection = controlArray;
        }
    }
}
