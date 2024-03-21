using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Com.Boc.Icms.MetadataEdit.Base.Event;
using Com.Boc.Icms.MetadataEdit.Support.GlobalCache;
//using Com.Boc.Icms.LogDLL;
using Com.Boc.Icms.MetadataEdit.Base.Xml;
using Com.Boc.Icms.MetadataEdit.Base.Event;
using Com.Boc.Icms.MetadataEdit.Base.Xml;

namespace Com.Boc.Icms.MetadataEdit.Support.Template
{
    // 本类处理控件模板示例
    //<Template Name="Template1" AutoSave="True">
    //   <Item Name="combobox1" Caption="控件1" Type="Combobox" Style="Height:50;Width:100;" Visible = "True" Enable = "True">
    //       <SelectedItem>下拉选项1</SelectedItem>
    //       <SelectedItem>下拉选项2</SelectedItem>
    //   </Item>
    //   <Item Name="textbox1" Caption="控件2" Type="TextBox" Style="Height:50;Width:150;" Validate="^\d+$" ValidateGroup="1" Text="12345678" ErrorMessage="请输入数字！" EventName="LostFocus_Validate" />
    //   <Item Name="textbox2" Caption="控件3" Type="TextBox" Style="Height:50;Width:200;" Validate="^\w+$" ValidateGroup="1" Text="aaabbbbccc" EventName="LostFocus_Validate"/>
    //   <Item Name="btn1" Caption="控件4" Type="Button" style="" Text="保存" EventName="Click_OK"/>
    //</Template>

    /// <summary>
    /// XML控件模板处理类
    /// </summary>
    public class XmlTemplateOperate : XmlOperate
    {

        protected ControlEvent ControlEvent;
        private readonly string _proIdAndThreadId = string.Empty;

        /// <summary>
        /// XML控件模板处理类构造函数
        /// </summary>
        /// <param name="btControl">控件类对象</param>
        /// <param name="controlEvent">事件类对象</param>
        /// <param name="templateXml">XML格式的字符串</param>
        public XmlTemplateOperate(ControlEvent controlEvent, string templateXml)
            : base()
        {
            this.ControlEvent = controlEvent;
            this.LoadXml(templateXml);
            this._proIdAndThreadId = System.Diagnostics.Process.GetCurrentProcess().Id + System.Threading.Thread.CurrentThread.ManagedThreadId.ToString();
        }

        /*/// <summary>
        /// 根据模板生成控件
        /// </summary>
        public Control[] GetControlCollection()
        {
            List<Control> listControl = new List<Control>();

            try
            {
                this.Root.ChildNodes.Cast<XmlNode>()
                    .ToList<XmlNode>().ForEach(a =>
                    {
                        if (a.NodeType == XmlNodeType.Element)
                        {
                            Control control = this.CreateControl(a.Attributes["Type"].Value, (XmlElement)a);
                            listControl.Add(control);
                        }
                    });
            }
            catch (Exception ex)
            {
                SysLog.Write(6146, ex, this._proIdAndThreadId);
            }

            return listControl.ToArray();
        }
*/
        /*/// <summary>
        /// 创建控件
        /// </summary>
        /// <param name="controlType">控件类型</param>
        /// <param name="element">XML控件节点</param>
        /// <returns></returns>
        public virtual Control CreateControl(string controlType, XmlElement element)
        {
            Control control = this.BtControl.Find(controlType);

            this.FillControlData(control, element);
            this.SetControlProperty(control, element);
            this.SetControlTag(control, element);
            this.BindControlEvent(control, element);

            return control;
        }*/

        /*/// <summary>
        /// 填充控件数据
        /// </summary>
        /// <param name="control">控件对象</param>
        /// <param name="element">XML控件节点</param>
        public virtual void FillControlData(Control control, XmlElement element)
        {
            //判断类型，并特殊处理
            if (control.GetType() == typeof(ComboBox))
            {
                
                ComboBox cbx = (ComboBox)control;
                //下拉样式都固定设定为只能选择，不能输入。
                cbx.DropDownStyle = ComboBoxStyle.DropDownList;

                if (!element.HasChildNodes) return;

                //加载Combobox下拉项
                cbx.BeginUpdate();
                
                element.ChildNodes
                    .Cast<XmlElement>().ToList().ForEach(a =>
                    {
                        cbx.Items.Add(a.InnerText);
                    });

                if (element.HasAttribute("Selected"))
                {
                    cbx.SelectedItem = element.Attributes["Selected"].Value;
                }
                
                else
                {
                    //默认选中第一项
                    cbx.SelectedIndex = 0;
                }
                
            }
            else if (control.GetType() == typeof(TextBox))
            {
                TextBox textBox = (TextBox)control;
                if (element.HasAttribute("MaxLength"))
                    textBox.MaxLength = int.Parse(element.Attributes["MaxLength"].Value);
            }
        }

        /// <summary>
        /// 设置控件属性
        /// </summary>
        /// <param name="control">控件对象</param>
        /// <param name="element">控件对应的XML节点</param>
        public virtual void SetControlProperty(Control control, XmlElement element)
        {
            if (element.HasAttribute("Caption"))
                control.AccessibleName = element.Attributes["Caption"].Value;
            if (element.HasAttribute("Name"))
                control.Name = element.Attributes["Name"].Value;
            if (element.HasAttribute("Text"))
                control.Text = element.Attributes["Text"].Value;
            if (element.HasAttribute("Visible"))
                control.Visible = bool.Parse(element.Attributes["Visible"].Value);
            if (element.HasAttribute("Enable"))
                control.Enabled = bool.Parse(element.Attributes["Enable"].Value);
            if (element.HasAttribute("Style")) this._commFunc.SetControlStyle(control, element.Attributes["Style"].Value);
        }
*/
        /*/// <summary>
        /// 将控件事件所需的XML属性放入Tag属性
        /// </summary>
        /// <param name="control">控件对象</param>
        /// <param name="element">XML控件节点</param>
        public virtual void SetControlTag(Control control, XmlElement element)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            string[] names = Enum.GetNames(typeof(SupportEnumType.EnumControlTag));

            element.Attributes
                .Cast<XmlAttribute>().ToList().ForEach(a =>
                {
                    if (element.HasAttribute(a.Name) && names.Contains(a.Name))
                        dic.Add(a.Name, a.Value);
                });

            control.Tag = dic as object;
        }*/

        /*/// <summary>
        /// 绑定事件
        /// </summary>
        /// <param name="control">控件对象</param>
        /// <param name="element">XML控件节点</param>
        public virtual void BindControlEvent(Control control, XmlElement element)
        {
            if (element.HasAttribute("EventName"))
            {
                string eventName = element.Attributes["EventName"].Value;
                this.ControlEvent.AddEvent(control, eventName);
            }
        }*/
    }
}
