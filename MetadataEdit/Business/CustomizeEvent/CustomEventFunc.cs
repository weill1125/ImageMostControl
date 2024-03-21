using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Com.Boc.Icms.LogDLL;
using Com.Boc.Icms.MetadataEdit.Business.BusinessData;
using Com.Boc.Icms.MetadataEdit.Support.GlobalCache;
using Com.Boc.Icms.MetadataEdit.Support.ProviderEffect;
using System.Xml;
using Com.Boc.Icms.MetadataEdit.Base.Xml;
using Com.Boc.Icms.MetadataEdit.Services;
using Com.Boc.Icms.MetadataEdit.DataTables;

namespace Com.Boc.Icms.MetadataEdit.Business.CustomizeEvent
{
    /// <summary>
    /// 自定义事件处理类
    /// </summary>
    class CustomEventFunc
    {
        private readonly XmlElement _xmlRoot = null;
        private readonly Control[] _controlArray = null;
        private readonly int _parentId = 0;
        private readonly string _parentNodeName = string.Empty;

        private DataServies dataServies;
        //在验证的过程中启用或者禁用的控件
        public List<Control> EnableControl = null;
        //当前控件对象
        public Control CurrentControl = null;

        /// <summary>
        /// 当文本值验证时发生
        /// 注意：添加此事件，将不再激活内部消息提示框
        /// </summary>
        public event ShowMessage TextValidated = null;

        /// <summary>
        /// 当唯一性值验证时发生
        /// 注意：添加此事件，将不再激活内部消息提示框
        /// </summary>
        //public event ShowMessage TextMesage = null;

        /// <summary>
        /// 当文本值验证时发生
        /// 注意：添加此事件，将不再激活内部消息提示框
        /// </summary>
        public event UpdateDirectory BizMetadata1Changed = null;

        //改变树节点的前景颜色
        public event UpdateDirectory ChangeTreeNodeForeColor = null;

        /// <summary>
        /// 当外部验证XML时发生
        /// </summary>
        public event ValidateXml XmlValidated = null;

        private readonly string _proIdAndThreadId = string.Empty;

        private EnumType.TableType tableType;

        private DataRow dataRow;

        /// <summary>
        /// 自定义事件处理类构造器
        /// </summary>
        /// <param name="dataManage">数据管理类对象</param>
        /// <param name="controlArray">控件数组对象</param>
        /// <param name="parentId">控件的顶级节点编号</param>
        public CustomEventFunc(DataServies dataServies, Control[] controlArray, EnumType.TableType tableType, DataRow dataRow, XmlElement xmlInfo)
        {
            this._proIdAndThreadId = System.Diagnostics.Process.GetCurrentProcess().Id + System.Threading.Thread.CurrentThread.ManagedThreadId.ToString();
            this._controlArray = controlArray;
            //this._parentId = parentId;
            //this._parentNodeName = dataManage.SelectNodeRow(parentId)["Child"].ToString();
            this.dataServies = dataServies;
            this._xmlRoot = xmlInfo;
            this.tableType = tableType;
            this.dataRow = dataRow;
            this.BindDefaultEvent();
        }

        public void Click_OK(object sender, EventArgs e)
        {
            //判断控件是否在数据结构中存在
            //bool isExists = false;

            //判断控件是否修改元数据
            bool isEdit = false;
            int checkup = 0;
            Dictionary<string, string> dic = new Dictionary<string, string>();

            //判断是否弹出提示框并且将变红的Doc节点置黑
            //bool isHint = false;
            string hintMessage = string.Empty;

            Control ctl = sender as Control;
            string message = "";
            #region 注释
            //if (ctl is Button)
            //{

            //    checkup = ReadXmlData(string.Empty, false);

            //    if (checkup == 1)
            //    {
            //        //检查失败，返回
            //        return;
            //    }
            //    if (checkup == 2)
            //    {
            //        //检查通过，但不保存
            //        return;
            //    }
            //    //如果以上两者都不是，那么走下边保存
            //}
            //else
            //{
            //    string name = ctl.Name;
            //    string text = ctl.Text;

            //    checkup = ReadXmlData(name, true);

            //    if (checkup == 1)
            //    {
            //        //检查失败，返回
            //        return;
            //    }
            //    if (checkup == 2)
            //    {
            //        //检查通过，但不保存
            //        return;
            //    }
            //    //如果以上两者都不是，那么走下边保存
            //}
            #endregion

            if (ctl is Button)
            {
                foreach (Control ctrl in this._controlArray)
                {
                    if (ctrl is Button)
                    {
                        continue;
                    }

                    checkup = this.ReadXmlData(ctrl.Name, false, ctrl.Text, this._parentId,ref message);

                    if (checkup == 1)
                    {
                        //检查失败，返回
                        this.ShowMsg(message, false);
                        return;
                    }
                }
            }
            else
            {
                //dic.Add(ctl.Name, ctl.Text);
                checkup = this.ReadXmlData(ctl.Name, false, ctl.Text, this._parentId, ref message);

                if (checkup == 1)
                {
                    //检查失败，返回
                    return;
                }
            }
            //这里checkup 的值是 0或3的时候是通过的，0.不进行检查；3，检查通过，但保存

            //检测是否自动保存
            if (!this.CheckAutoSave((Control)sender,
                ((Control)sender).Tag as Dictionary<string, string>))
            {                
                if (!this.ValidateAllControlByClick()) return;
                if (!this.ValidateGroupXmlByClick()) return;
            }

            //置黑
            if (this.ChangeTreeNodeForeColor != null)
            {
                this.ChangeTreeNodeForeColor(null);
            }

            try
            {
                ////新添加的控件在数据结构中的索引
                //int index = this.DataManage.GetMaxIndex(this._parentId,
                //    SupportEnumType.EnumNodeType.Control);

                //执行保存操作
                dataRow.BeginEdit();
                this._controlArray.Where(a => a.GetType() != typeof(Button))
                    .ToList().ForEach(a =>
                    {
                        if (this.dataRow.Table.Columns.Contains(a.Name) 
                            && !a.Text.Trim().Equals(this.dataRow[a.Name]))
                        {
                            isEdit = true;

                            dataRow.BeginEdit();
                            //数据结构中存在相同的控件节点记录，则执行修改
                            string oldbizname = this.dataRow[a.Name].ToString();
                            this.dataRow[a.Name] =  a.Text.Trim();
                            dataRow.EndEdit();
                            //如果为交易号信息，更新左边文件夹
                            if (a.Name.Equals("biz_metadata1"))
                            {
                                DocTable doc =(DocTable)this.dataServies.GetDataTable(EnumType.TableType.DocTable);
                                DataRow[] dataRows =  doc.Select("["+a.Name +"]='"+ oldbizname + "'");                                
                                for(int i=0;i< dataRows.Length; i++)
                                    dataRows[i][a.Name] = a.Text.Trim();
                                                                   
                                if (this.BizMetadata1Changed != null)
                                {
                                    this.BizMetadata1Changed(a.Text.Trim());
                                }
                                
                            }
                        }
                        ////isExists = false;
                        ////在数据结构检索对应的控件名，执行数据的修改操作
                        //this.DataManage.GetRowsByType(this._parentId,
                        //    SupportEnumType.EnumNodeType.Control)
                        //    .ToList().ForEach(b =>
                        //    {

                        //        if (b["Child"].Equals(a.Name))
                        //        {
                        //            //isExists = true;

                        //            string tmpText = b["Value"].ToString();

                        //            //要写入数据和原来数据不同，则更改表中数据
                        //            if (a.Text.Trim() != b["Value"].ToString())
                        //            {
                        //                isEdit = true;
                        //                //数据结构中存在相同的控件节点记录，则执行修改
                        //                this.DataManage.UpdateValue((int)b["ID"], a.Text.Trim());

                        //                //如果为交易号信息，更新左边文件夹
                        //                if (a.Name.Equals("biz_metadata1"))
                        //                {
                        //                    if (this.BizMetadata1Changed != null)
                        //                    {
                        //                        this.BizMetadata1Changed(a.Text.Trim());
                        //                    }
                        //                }
                        //            }
                        //        }
                        //    });

                        #region 注释
                        //不存在，则执行添加
                        //注：如果控件模板是动态传递，
                        //    控件节点在保存的时候才入库的情况，则需要开启添加功能
                        //if (!isExists)
                        //{
                        //    isEdit = true;
                        //    dataManage.AddRow(parentID,
                        //        a.Name,
                        //        a.Text.Trim(),
                        //        SupportEnumType.EnumNodeType.Control,
                        //        ++index);
                        //}
                        #endregion

                    });

                if (isEdit)
                {
                    //数据结构中存在相同的控件节点记录，则执行修改
                   
                    if(this.dataRow.Table.Columns.Contains("oper_type") &&!"A".Equals(this.dataRow["oper_type"]))
                    {
                        this.dataRow["oper_type"] = "E";
                    }                 
                    this.dataRow["modi_meta"] = "Y";

                    //string value = this.DataManage.SelectNodeValue(this._parentId);
                    //string operType = XmlOperate.GetNodePropertyValue(value, "oper_type");
                    ////更改oper_type
                    //if (operType != "A")
                    //    this.DataManage.UpdateValue(this._parentId,
                    //        XmlOperate.UpdateNodeProperty(value, "oper_type", "E"));
                    ////更改modi_meta
                    //this.DataManage.UpdateValue(this._parentId,
                    //  XmlOperate.UpdateNodeProperty(this.DataManage.SelectNodeValue(this._parentId), "modi_meta", "Y"));

                    this.ShowMsg(message + SysLog.GetMessage(9100), false);
                }
                this.dataRow.EndEdit();
                if (isEdit)
                {
                    this.dataServies.Commit();
                }
            }
            catch (Exception ex)
            {
                this.dataServies.RollBack();
                this.ShowMsg(SysLog.GetMessage(7109) + ex.Message, true);
                SysLog.Write(7109, ex, this._proIdAndThreadId);
            }
            //ShowMsg("传入的提示信息（TextMessage）属性为空，请检查！");
        }

        //多语言获取信息值
        private string GetValue(string num)
        {
            return GlobalResource.AccessResource.GetInstance().GetValue(num);
        }

        /// <summary>
        /// 唯一性检查
        /// 0.不进行检查；1.检查失败，返回；2.检查通过，但不保存。3.检查通过，保存
        /// </summary>
        /// <param name="ctlName">控件的Name值</param>
        /// <param name="isAutoSave">是否自动保存</param>
        /// <param name="ctlText">控件对应的值</param>
        /// <param name="parentId">父节点的ID</param>
        /// <param name="message">相关状态记录的信息</param>
        /// <returns></returns>
        private int ReadXmlData(string ctlName, bool isAutoSave, string ctlText, int parentId,ref string message)
        {
            //XmlElement xe = null;
            int checkup = 1;
            //if (XmlTemplateCache.xmlRoot != null)
            //{
            //    xe = XmlTemplateCache.xmlRoot;
            //}

            if (this._xmlRoot != null)
            {
                //为控件模板填充数据
                foreach (XmlElement xeChild in this._xmlRoot.ChildNodes.Cast<XmlElement>().ToList())
                {
                    if (xeChild.HasAttribute("Name"))
                    {
                        string isUnique = string.Empty;
                        string isSubmit = string.Empty;

                        if (isAutoSave)
                        {
                            if (xeChild.GetAttribute("Name") == ctlName)
                            {
                                checkup = this.UniqueCheckup(xeChild, ctlName, ctlText, ref message);
                                //检查失败
                                if (checkup == 1)
                                {
                                    return 1;
                                }

                                return checkup;
                            }
                        }
                        else
                        {
                            if (xeChild.GetAttribute("Name") == ctlName)
                            {
                                checkup = this.UniqueCheckup(xeChild, ctlName, ctlText, ref message);
                                if (checkup == 1)
                                {
                                    return 1;
                                }
                            }

                            #region 注释
                            //if (xe_Child.GetAttribute("Name") == "index_metadata1")
                            //{
                            //    checkup = UniqueCheckup(xe_Child, "index_metadata1");
                            //    //检查失败
                            //    if (checkup == 1)
                            //    {
                            //        return 1;
                            //    }
                            //    //检查通过，但不保存
                            //    if (checkup == 2)
                            //    {
                            //        return 2;
                            //    }
                            //}

                            //if (xe_Child.GetAttribute("Name") == "index_metadata2")
                            //{
                            //    checkup = UniqueCheckup(xe_Child, "index_metadata2");
                            //    if (checkup == 1)
                            //    {
                            //        return 1;
                            //    }
                            //    //检查通过，但不保存
                            //    if (checkup == 2)
                            //    {
                            //        return 2;
                            //    }
                            //}

                            //if (xe_Child.GetAttribute("Name") == "index_metadata3")
                            //{
                            //    checkup = UniqueCheckup(xe_Child, "index_metadata3");
                            //    if (checkup == 1)
                            //    {
                            //        return 1;
                            //    }
                            //    //检查通过，但不保存
                            //    if (checkup == 2)
                            //    {
                            //        return 2;
                            //    }
                            //}

                            //if (xe_Child.GetAttribute("Name") == "ext_metadata")
                            //{
                            //    checkup = UniqueCheckup(xe_Child, "ext_metadata");
                            //    if (checkup == 1)
                            //    {
                            //        return 1;
                            //    }
                            //    //检查通过，但不保存
                            //    if (checkup == 2)
                            //    {
                            //        return 2;
                            //    }
                            //}
                            #endregion
                        }
                    }
                }
            }

            return checkup;
        }

        /// <summary>
        /// 唯一性检查
        /// 0：不进行检查，返回；1.检查失败，返回；2.检查通过。3.检查不通过，保存
        /// </summary>
        /// <param name="xe">XmlElement对象，去属性值</param>
        /// <param name="index_Metadata">index_Metadata属性</param>
        /// <param name="childText">子节点的名称对应的值</param>
        /// <param name="parentId">父节点的ID</param>
        /// <param name="message">相关状态记录的信息</param>
        /// <returns></returns>
        private int UniqueCheckup(XmlElement xe, string indexMetadata, string childText ,ref string message)
        {
            //DataManage dm = XmlTemplateCache.UniqueDataManage;

            string isUnique = string.Empty;
            string isSubmit = string.Empty;

            if (xe.GetAttribute("Name") == indexMetadata)
            {
                if (xe.HasAttribute("IsUnique"))
                {
                    isUnique = xe.GetAttribute("IsUnique");

                    if (isUnique == "False")
                    {
                        //不进行唯一性检查
                        return 0;
                    }

                    if (isUnique == "True")
                    {
                        bool isTure = this.UniqueCheckup(this.dataServies.GetDataTable(this.tableType), indexMetadata, childText);//唯一性检查

                        if (isTure) //唯一性检查失败
                        {
                            if (xe.HasAttribute("IsSubmit"))
                            {
                                isSubmit = xe.GetAttribute("IsSubmit");

                                if (isSubmit == "True")
                                {
                                    if (xe.HasAttribute("TextMessage"))
                                    {
                                        if (string.IsNullOrEmpty(xe.GetAttribute("TextMessage")))
                                        {
                                            message = xe.GetAttribute("Caption").Trim() + GlobalResource.AccessResource.GetInstance().GetValue("144") + " ";
                                            return 3;
                                        }
                                        message = xe.GetAttribute("TextMessage") + " ";
                                        return 3;
                                    }
                                    message = xe.GetAttribute("Caption").Trim() + GlobalResource.AccessResource.GetInstance().GetValue("144") + " ";
                                    return 3;
                                }
                                if (xe.HasAttribute("TextMessage"))
                                {
                                    if (string.IsNullOrEmpty(xe.GetAttribute("TextMessage")))
                                    {
                                        message = xe.GetAttribute("Caption").Trim() + GlobalResource.AccessResource.GetInstance().GetValue("144") + " ";
                                        return 1;
                                    }
                                    message = xe.GetAttribute("TextMessage") + " ";
                                    return 1;
                                }
                                message = xe.GetAttribute("Caption").Trim() + GlobalResource.AccessResource.GetInstance().GetValue("144") + " ";
                                return 1;
                            }
                            if (xe.HasAttribute("TextMessage"))
                            {
                                if (string.IsNullOrEmpty(xe.GetAttribute("TextMessage")))
                                {
                                    message = xe.GetAttribute("Caption").Trim() + GlobalResource.AccessResource.GetInstance().GetValue("144") + " ";
                                    return 1;
                                }
                                message = xe.GetAttribute("TextMessage") + " ";
                                return 1;
                            }
                            message = xe.GetAttribute("Caption").Trim() + GlobalResource.AccessResource.GetInstance().GetValue("144") + " ";
                            return 1;

                            #region 注释

                            //    //如果唯一性检查成功，那么将会走下边看是否提交
                            //    if (xe.HasAttribute("isSubmit"))
                            //    {
                            //        isSubmit = xe.GetAttribute("isSubmit");


                            //        if (isSubmit == "True")
                            //        {
                            //            //"唯一性检查通过,保存！";
                            //            return 3;
                            //        }
                            //        else
                            //        {
                            //            //唯一性检查通过但不保存！";
                            //            if (xe.HasAttribute("TextMessage"))
                            //            {
                            //                if (string.IsNullOrEmpty(xe.GetAttribute("TextMessage")))
                            //                {
                            //                    TextValidated(xe.GetAttribute("Caption") + GlobalResource.AccessResource.GetInstance().GetValue("144"));
                            //                    return 2;
                            //                }
                            //                else
                            //                {
                            //                    TextValidated(xe.GetAttribute("TextMessage"));
                            //                    return 2;
                            //                }
                            //            }
                            //            else
                            //            {
                            //                TextValidated(xe.GetAttribute("Caption") + GlobalResource.AccessResource.GetInstance().GetValue("144"));
                            //                return 2;
                            //            }
                            //        }
                            //    }
                            //    else
                            //    {
                            //        //如果没有是否提交（isSubmit）属性,那么默认不保存，相当于False
                            //        if (xe.HasAttribute("TextMessage"))
                            //        {
                            //            if (string.IsNullOrEmpty(xe.GetAttribute("TextMessage")))
                            //            {
                            //                TextValidated(xe.GetAttribute("Caption") + GlobalResource.AccessResource.GetInstance().GetValue("144"));
                            //                return 2;
                            //            }
                            //            else
                            //            {
                            //                TextValidated(xe.GetAttribute("TextMessage"));
                            //                return 2;
                            //            }
                            //        }
                            //        else
                            //        {
                            //            TextValidated(xe.GetAttribute("Caption") + GlobalResource.AccessResource.GetInstance().GetValue("144"));
                            //            return 2;
                            //        }
                            //    }
                            #endregion
                        }
                        return 2;
                    }
                }
            }

            return 0;
        }

        /// <summary>
        /// 唯一性检查,如果不唯一返回true，唯一返回false
        /// </summary>
        /// <param name="dm">docsinfo对应的内存表中的数据</param>
        /// <param name="childName">子节点的名称（如：index_metadata1，index_metadata2，index_metadata3，ext_metadata）</param>
        /// <param name="childText">子节点的名称对应的值</param>
        /// <returns></returns>
        private bool UniqueCheckup(DataTable dt, string childName, string childText)
        {
            //检查唯一性
            DataRow[] drs = dt.Select(""+ childName + "= \'" + childText + "\'");
            //List<string> list = new List<string>();

            if (drs.Length <= 0)
            {
                return false;
            }
            else
            {
                return true;
            }

            //for (int i = 0; i < drs.Length; i++)
            //{
            //    int _parentID = Int32.Parse(drs[i]["ParentID"].ToString());
            //    if (_parentID != parentId)
            //    {
            //        list.Add(drs[i]["Value"] as string);//这里内存表中有一个ID
            //    }
            //}

            //if (list.Count <= 0)
            //{
            //    return false;
            //}

            //for (int i = 0; i < list.Count; i++)
            //{
            //    if (childText == list[i])
            //    {
            //        return true;
            //    }
            //}

            #region 注释
            //for (int i = 0; i < list.Count; i++)
            //{
            //    for (int j = 0; j < list.Count; j++)
            //    {
            //        if (i != j)
            //        {
            //            if (list[i] == list[j])
            //            {
            //                return true;
            //            }
            //        }
            //    }
            //}
            #endregion

            //return false;
        }

        public void Click_NO(object sender, EventArgs e)
        {
        }

        public void Click_Cancel(object sender, EventArgs e)
        {
        }

        public void TextChanged(object sender, EventArgs e)
        {
            this.CurrentControl = (Control)sender;
            //isOutValidate = true;
        }

        public void LostFocus(object sender, EventArgs e)
        {
            Control control = (Control)sender;

            if (this.CurrentControl == null) this.CurrentControl = control;

            try
            {
                if (this.CurrentControl == control) this.ValidateInput(control);
            }
            catch (Exception ex)
            {
                this.ShowMsg(SysLog.GetMessage(7110)+ex.Message, true);
                SysLog.Write(7110, ex, this._proIdAndThreadId);
            }
        }

        /// <summary>
        /// 根据判断Tag中的验证属性，验证输入值
        /// 注：先执行内部验证，成功后判断是否自动保存，
        /// 是则执行外部验证并进行保存，
        /// 否则等待按钮点击之后执行外部校验，并保存
        /// </summary>
        /// <param name="control">控件对象</param>
        private bool ValidateInput(Control control)
        {
            //获取控件文本
            string strText = control.Text.Trim();
            //取得面板子控件Tag中存取的XML属性值
            Dictionary<string, string> dic = control.Tag as Dictionary<string, string>;
            //定义在Dictionary中根据指定的键搜索的值存取到的变量
            string value = string.Empty;

            //判断Tag中是否存在文本验证规则的属性
            if (dic.TryGetValue(
                SupportEnumType.EnumControlTag.Validate.ToString(),
                out value))
            {
                //判断文本是否匹配验证规则
                if (!Regex.IsMatch(strText, value))
                {
                    //取得Tag中存在的验证失败的提示信息
                    dic.TryGetValue(
                        SupportEnumType.EnumControlTag.ErrorMessage.ToString(),
                        out value);

                    if (value == null)
                    {
                        this.ShowMsg(SysLog.ReplaceParam(
                            SysLog.GetMessage(7140),
                             new[] { control.AccessibleName, strText }),
                             true);
                        SysLog.Write(7140, this._proIdAndThreadId, new[] { control.AccessibleName, strText });
                    }
                    else
                    {
                        this.ShowMsg(value, true);
                        SysLog.Write(7400, this._proIdAndThreadId, value);
                    }

                    control.BackColor = System.Drawing.Color.Red;
                    control.Focus();
                    //AllowButton(false);
                    return false;
                }
                control.BackColor = System.Drawing.Color.White;
                //AllowButton(true);

                this.ShowMsg(SysLog.GetMessage(9103), false);
                SysLog.Write(9103, this._proIdAndThreadId);
            }

            //检测是否自动保存
            if (this.CheckAutoSave(control, dic))
            {
                if (!this.ValidateGroupXml(control, dic)) return false;
                this.Click_OK(control, null);
            }

            return true;
        }

        /// <summary>
        /// 外部系统验证组XML
        /// </summary>
        /// <param name="control">控件对象</param>
        /// <param name="dic">面板子控件Tag中存取的XML属性值</param>
        private bool ValidateGroupXml(Control control, Dictionary<string, string> dic)
        {
            //控件组外部系统验证
            Control[] groupControl = null;
            string validataXml = this.CheckGroupValidate(control, dic, ref groupControl);
            if (validataXml != string.Empty)
            {
                if (!this.CheckOutXmlValidate(control, validataXml))
                {
                    this.SetControlGroupColor(groupControl, false);
                    return false;
                }
                this.SetControlGroupColor(groupControl, true);
            }
            else
            {
                control.BackColor = System.Drawing.Color.White;
            }

            return true;
        }

        /// <summary>
        /// 检测组验证
        /// </summary>
        /// <param name="control">控件对象</param>
        /// <param name="dic">面板子控件Tag中存取的XML属性值</param>
        /// <param name="groupControl">与当前相关的控件组</param>
        private string CheckGroupValidate(Control control, Dictionary<string, string> dic, ref Control[] groupControl)
        {
            //定义待发送的进行验证的控件与值组成的XML字符串
            string validataXml = "";

            //定义在Dictionary中根据指定的键搜索的值存取到的变量
            string value = string.Empty;

            if (!dic.TryGetValue(
                SupportEnumType.EnumControlTag.ValidateGroup.ToString(),
                out value))
            {
                validataXml = "<" + control.Name + ">" +
                    control.Text.Trim() +
                    "</" + control.Name + ">";
            }
            else
            {
                //验证组中其他的输入
                if (!this.ValidataGroup(control, value, out groupControl))
                    return string.Empty;

                //全部都验证成功 则保存输入信息
                groupControl.ToList().ForEach(a =>
                {
                    validataXml += "<" + a.Name + ">" +
                        a.Text.Trim() +
                        "</" + a.Name + ">";
                });
            }

            try
            {
                //使用uniq_metadata节点值作为外系统唯一识别号
                string uniqNodeName = (this.tableType ==EnumType.TableType.BusinessTable) ?
                    "biz_metadata1" :
                    "uniq_metadata";

                //DataRow[] nodeRows = this.DataManage.SelectNodeRows(this._parentId,
                //    uniqNodeName,
                //    SupportEnumType.EnumNodeType.Control);

                validataXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                    "<CheckData>" +
                    "<" + uniqNodeName + ">" +
                   dataRow[uniqNodeName] +
                    "</" + uniqNodeName + ">" +
                    validataXml +
                    "</CheckData>";
            }
            catch (Exception ex)
            {
                this.ShowMsg(SysLog.GetMessage(7111) + ex.Message, true);
                SysLog.Write(7111, ex, this._proIdAndThreadId);
                validataXml = string.Empty;
            }

            return validataXml;
        }

        /// <summary>
        /// 检测外部系统XML验证
        /// </summary>
        /// <param name="control">控件对象</param>
        /// <param name="validataXml">待发送的进行验证的控件与值组成的XML字符串</param>
        private bool CheckOutXmlValidate(Control control, string validataXml)
        {
            try
            {
                if (this.XmlValidated != null)
                {
                    this.ShowMsg(SysLog.GetMessage(9101), false);
                    SysLog.Write(9101, this._proIdAndThreadId);
                    
                    if (!this.XmlValidated(validataXml))
                    {
                        //不能操作按钮进行保存
                        this.ShowMsg(SysLog.GetMessage(7141), true);
                        SysLog.Write(7141, this._proIdAndThreadId);

                        control.Focus();
                        //AllowButton(false);

                        return false;
                    }
                    //可以操作按钮进行保存
                    this.ShowMsg(SysLog.GetMessage(9102), false);
                    SysLog.Write(9102, this._proIdAndThreadId);

                    //AllowButton(true);
                    this.CurrentControl = null;
                }
                else
                {
                    //AllowButton(true);
                    this.CurrentControl = null;
                }
            }
            catch(Exception ex)
            {
                this.ShowMsg(SysLog.GetMessage(7111)+ex.Message, true);
                SysLog.Write(7110, ex, this._proIdAndThreadId);
            }

            return true;
        }

        /// <summary>
        ///  验证组中其他的控件输入
        /// </summary>
        /// <param name="control">控件对象</param>
        /// <param name="groupName">组名</param>
        /// <param name="groupControl">验证控件组</param>
        private bool ValidataGroup(Control control, string groupName, out Control[] groupControl)
        {
            //定义在Dictionary中根据指定的键搜索的值存取到的变量
            string value = string.Empty;
            Dictionary<string, string> dic = null;
            bool isGroup = false;

            //取得同组的控件列表
            List<Control> listGroupControl = this._controlArray.Where(a =>
            {
                dic = a.Tag as Dictionary<string, string>;

                isGroup = dic.TryGetValue(
                   SupportEnumType.EnumControlTag.ValidateGroup.ToString(),
                   out value);

                if (isGroup &&
                    (value == groupName ||
                    value.Split(',').Any(b => b == groupName)))
                    return true;

                return false;
            }).ToList();
            groupControl = listGroupControl.ToArray();

            //判断组中其他的控件是否已输入值，任意一个没有都代表验证组失败
            return !listGroupControl.Any(a => a.Text.Trim() == string.Empty);
        }

        /// <summary>
        /// 显示错误提示消息
        /// </summary>
        /// <param name="value">消息</param>
        /// <param name="isError">是否错误消息</param>
        private void ShowMsg(string value, bool isError)
        {
            //判断是否执行外部委托的方法
            if (this.TextValidated != null)
            {
                //调用外部绑定事件
                this.TextValidated(value);
            }
            else
            {
                //直接提示信息
                if (isError)
                {
                    MessageBox.Show(value,
                        SysLog.GetMessage(7400),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    SysLog.Write(7400, this._proIdAndThreadId,
                        value);
                }
                else
                {
                    MessageBox.Show(value,
                        SysLog.GetMessage(9900),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    SysLog.Write(9900, this._proIdAndThreadId,
                        value);
                }
            }
        }

        /// <summary>
        /// 将按钮的Enable设置为指定的bool值
        /// </summary>
        /// <param name="isEnable">是否启用</param>
        private void AllowButton(bool isEnable)
        {
            //初始将按钮加载到数组
            if (this.EnableControl == null)
            {
                this.EnableControl = new List<Control>();

                this._controlArray.ToList().ForEach(a =>
                {
                    if (a.GetType() == typeof(Button))
                    {
                        a.Enabled = isEnable;
                        this.EnableControl.Add(a);
                    }
                });
            }
            else
            {
                this.EnableControl.ForEach(a => { a.Enabled = isEnable; });
            }
        }

        /// <summary>
        /// 判断是否自动保存
        /// </summary>
        /// <param name="control">控件对象</param>
        /// <param name="dic">面板子控件Tag中存取的XML属性值</param>
        private bool CheckAutoSave(Control control, Dictionary<string, string> dic)
        {
            //定义在Dictionary中根据指定的键搜索的值存取到的变量
            string value = string.Empty;
            //面板的Tag
            Dictionary<string, string> dicBasePanel =
                control.Parent.Parent.Tag as Dictionary<string, string>;

            if (!dicBasePanel.TryGetValue(
               SupportEnumType.EnumControlTag.AutoSave.ToString(),
               out value))
                return false;
            return bool.Parse(value);
        }

        /// <summary>
        /// 点击按钮执行外部验证
        /// </summary>
        private bool ValidateGroupXmlByClick()
        {
            string value = string.Empty;
            List<string> groupName = new List<string>();
            Dictionary<string, string> dic = null;

            foreach (var itemControl in this._controlArray)
            {
                dic = itemControl.Tag as Dictionary<string, string>;
                if (!dic.TryGetValue(SupportEnumType.EnumControlTag.ValidateGroup.ToString(),
                  out value) ||
                  value == string.Empty ||
                  groupName.Contains(value) ||
                  groupName.Any(a => value.Split(',').Any(b => b == a)))
                    continue;
                groupName.Add(value);

                if (!this.ValidateGroupXml(itemControl, dic))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 绑定默认内部事件(此事件不需要配置)
        /// </summary>
        private void BindDefaultEvent()
        {
            foreach (var itemControl in this._controlArray)
            {
                itemControl.TextChanged += this.TextChanged;
            }
        }

        /// <summary>
        /// 设置控件组的颜色
        /// </summary>
        /// <param name="groupControl">控件组</param>
        /// <param name="isSuccess">是否验证成功</param>
        private void SetControlGroupColor(Control[] groupControl, bool isSuccess)
        {
            if (groupControl == null) return;
            foreach (var itemControl in groupControl)
            {
                if (isSuccess)
                    itemControl.BackColor = System.Drawing.Color.White;
                else
                    itemControl.BackColor = System.Drawing.Color.Red;
            }
        }

        /// <summary>
        /// 点击按钮执行内部验证
        /// </summary>
        private bool ValidateAllControlByClick()
        {
            foreach (var itemControl in this._controlArray)
            {
                if (!this.ValidateInput(itemControl)) return false;
            }

            return true;
        }
    }
}
