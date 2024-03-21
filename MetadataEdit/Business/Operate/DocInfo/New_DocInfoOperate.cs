using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Com.Boc.Icms.MetadataEdit.Business.BusinessData;
using Com.Boc.Icms.MetadataEdit.Support.Template;
using Com.Boc.Icms.MetadataEdit.Services;
using Com.Boc.Icms.MetadataEdit.DataTables;
using Com.Boc.Icms.MetadataEdit.Models;
using Com.Boc.Icms.LogDLL;
using Com.Boc.Icms.LogDLL;
using Com.Boc.Icms.MetadataEdit.Business.BusinessData;
using Com.Boc.Icms.MetadataEdit.Business.Operate.DocInfo;
using Com.Boc.Icms.MetadataEdit.Business.Operate;
using Com.Boc.Icms.MetadataEdit.DataTables;
using Com.Boc.Icms.MetadataEdit.Models;
using Com.Boc.Icms.MetadataEdit.Services;
using Com.Boc.Icms.MetadataEdit.Support.Template;
using Com.Boc.Icms.MetadataEdit;
using Com.Boc.Icms.MetadataEdit.GetDataSource;

namespace Com.Boc.Icms.MetadataEdit.Business.Operate.DocInfo
{

    /// <summary>
    /// 文档操作类
    /// 描述：doc_index代表图像的位置之后，不能再根据文档索引来取文档，
    /// 而需要根据pkuuid来取。原修改原意之前的操作doc_info的代码保留不做修改。
    /// </summary>
    partial class NewDocInfoOperate : XmlNodeOperate, INewDocInfocOperate
    {
        private readonly Property _property;
        private readonly IXmlDataImport _ixmlfe;

        private readonly XmlTemplateCache _xmlct;





        /// <summary>
        /// 在验证控件文本时发生
        /// </summary>
        public event ShowMessage TextValidated;

        /// <summary>
        /// 在更改交易编号时发生
        /// </summary>
        public event UpdateDirectory BizMetadata1Changed;

        /// <summary>
        /// 改变TreeNode前景色
        /// </summary>
        public event UpdateDirectory ChangeTreeNodeForeColor;

        /// <summary>
        /// 在验证控件文本组成的XML时发生
        /// </summary>
        public event ValidateXml XmlValidated;
        private readonly string _proIdAndThreadId = string.Empty;
        public GetSource _getSource;


        public NewDocInfoOperate(DataServies dataServies, Property property,
            IXmlDataImport ixmlfe, XmlTemplateCache xmlct, GetSource getSource)
            : base(dataServies)
        {
			//this._controls = new Controls();

			_getSource = getSource;

			this._property = property;
            this._ixmlfe = ixmlfe;
            this._xmlct = xmlct;
            this._proIdAndThreadId = System.Diagnostics.Process.GetCurrentProcess().Id + System.Threading.Thread.CurrentThread.ManagedThreadId.ToString();
        }

        ///// <summary>
        ///// 初始化加载文档类型docs_info的XML
        ///// 描述：docs_info包含很多doc_info，解析并保存到数据结构中。
        ///// </summary>
        ///// <param name="xml">xml字符串</param>
        ///// <param name="isAddUniqMetadata">是否自动添加uniq_metadata节点</param>
        ///// <param name="isAddGuid">是否自动添加uniq_metadata节点值guid</param>
        ///// <returns>pkuuid列表</returns>
        //public string[] InitXml(string xml, bool isAddUniqMetadata, bool isAddGuid, int index)
        //{
        //    try
        //    {
        //        //this._ixmlfe.ParentID = this.DataManage.GetIDByIndex(this._ixmlfe.ParentID, index);
        //        if (!this._ixmlfe.SaveXml(xml)) throw new Exception();

        //        //判断文档是否需要添加GUID节点
        //        List<string> pkuuidList = new List<string>();
        //        List<DataRow> rows = this.dataServies.GetRowsByType(EnumType.TableType.DocTable);
        //        rows.ForEach(a =>
        //        {
        //            pkuuidList.Add(this.AddGuidNode((int)a["ID"]));
        //            this.AddUniqMetadataNode((int)a["ID"], isAddUniqMetadata, isAddGuid);
        //        });
        //        return pkuuidList.ToArray();
        //    }
        //    catch (Exception ex)
        //    {
        //       SysLog.Write(7108,ex, this._proIdAndThreadId);
        //        return null;
        //    }
        //}
        /// <summary>
        /// 初始化加载文档类型docs_info的XML
        /// 描述：docs_info包含很多doc_info，解析并保存到数据结构中。
        /// </summary>
        /// <param name="xml">xml字符串</param>
        /// <returns>是否操作成功</returns>
        public bool AddDocs(string xml, string businessId)
        {
            try
            {
                //this._ixmlfe.ParentID = this.DataManage.GetIDByIndex(this._ixmlfe.ParentID, index);
                this._ixmlfe.AddDocs(xml, businessId);

                ////判断文档是否需要添加GUID节点
                //DataRow[] rows = this.DataManage.GetRowsByType(this._ixmlfe.ParentID,
                //    SupportEnumType.EnumNodeType.Node);
                //if(rows.Length != pkuuidList.Count)
                //    throw new Exception("rows.length !=  pkuuidList.Count");
                //for(int i =0; i < rows.Length ;i++ )
                //{
                //    this.AddGuidNode((int)rows[i]["ID"], pkuuidList[i]);
                //    this.AddUniqMetadataNode((int)rows[i]["ID"], isAddUniqMetadata, isAddGuid);
                //}

                return true;
            }
            catch (Exception ex)
            {
                //SysLog.Write(7108, ex, this._proIdAndThreadId);
                return false;
            }
        }

        /// <summary>
        /// 添加单个文档
        /// </summary>
        /// <param name="xml">文档XML</param>
        public string AddDoc(string xml, string businessId)
        {
            try
            {
                ////取得docs_info节点编号
                //int docsInfoId = this.CheckNode(this._property.BusinessId, "docs_info");

                return this._ixmlfe.AddDoc(xml, businessId);

            }
            catch (Exception ex)
            {

                //SysLog.Write(7112, ex, this._proIdAndThreadId);
                return string.Empty;
            }
        }

        /// <summary>
        /// 删除单个文档
        /// 描述：删除整个DOC,将递归删除DOC节点下面的所有子节点
        /// 并且将DOC的oper_type属性设置为"D"
        /// </summary>
        /// <param name="pkuuid">文档唯一标识</param>
        public bool DeleteDoc(string pkuuid)
        {
            try
            {
                //根据pkuuid 查找行
                DataRow docRow = this.dataServies.GetRowByKey(EnumType.TableType.DocTable, pkuuid);


                if (this._property.WorkType == EnumType.EnumWorkType.InitScan)
                {
                    docRow.Delete();
                }
                else
                {
                    //判断重扫补扫情况，新增doc_info，再删除，则不能生成XML节点,
                    //即直接删除当前doc_info节点
                    string oper_type = (string)docRow["oper_type"];

                    //重扫补扫新增的doc_info为A，拖放之后,doc_info没有图像则为I
                    //这两种情况都可以直接删除节点
                    if (oper_type == "A" || oper_type == "I")
                    {
                        docRow.Delete();
                    }
                    else
                    {
                        //docRow["oper_type"] = "D";
                        this.UpdateNodePropertyAndTime(docRow, "oper_type", "D");
                    }
                }

                this.dataServies.Commit();
                return true;
            }
            catch (Exception ex)
            {
                this.dataServies.RollBack();
                //SysLog.Write(7113, ex, this._proIdAndThreadId);
                return false;
            }
        }


        /// <summary>
        /// 修改节点的属性值(同时修改节点中的modi_time时间值)
        /// </summary>
        /// <param name="docRow">节点编号</param>
        /// <param name="attribute">属性名</param>
        /// <param name="value">值</param>
        public virtual void UpdateNodePropertyAndTime(DataRow docRow, string attribute, string value)
        {
            docRow.BeginEdit();
            docRow[attribute] = value;
            //docRow["Modi_time"] = this.Now;  //时间在EndEdit时更新
            docRow.EndEdit();
        }



        /// <summary>
        /// 切换节点对应的XML控件模板
        /// 描述：根据指定的模板名进行模板检索，并生成控件。根据指定的文档索引查找控件值，并填充控件。
        /// </summary>
        /// <param name="nodeType">节点种类/doc/page</param>
        /// <param name="key">主键</param>
        /// <param name="templateName">模板</param>
        public void ChangeXmlNode(EnumType.TableType tableType, string key, string templateName)
        {
            //模板为空则移除所有控件
            if (templateName == string.Empty)
            {
                //this._control.Controls.Clear();
                return;
            }
            try
            {
                //从模板列表中检索模板
                string xml = this._xmlct.FindTemplate(templateName);
                if (xml == string.Empty) return;

                //判断是否是businesss节点还是doc_info节点，
                //只有doc_info节点包含有pkuuid 
                //EnumType.TableType tableType = EnumType.TableType.BusinessTable;
                //if (nodeType == EnumType.NodeType.Business)
                //{
                //    tableType = EnumType.TableType.BusinessTable;

                //}
                //else if (nodeType == EnumType.NodeType.Pkuuid)
                //{
                //    tableType = EnumType.TableType.DocTable;
                //}

                DataRow dataRow = this.dataServies.GetRowByKey(tableType, key);

                string templateXml = this._xmlct.FillTemplate(xml, dataRow);
                
                if (templateXml == string.Empty) return;

                 _getSource.templateXml = templateXml;

                _getSource.dataServies = this.dataServies;

                _getSource.tableType = tableType;
                _getSource.Key = key;


				//if (!_getSource.dataRow.ContainsKey(tableType))
    //            {
    //                _getSource.dataRow.Add(tableType, dataRow);
				//}

				
				/*//加载模板控件
                this.InitTemplateControl(templateXml, tableType, dataRow);*/
			}
			catch (Exception ex)
            {
                //SysLog.Write(7114, ex, this._proIdAndThreadId);
            }
        }

        ///// <summary>
        ///// 根据指定的数据类型检索文档的PKUUID列表
        ///// </summary>
        ///// <param name="type">数据类型</param>
        ///// <returns></returns>
        //public string[] GetGuidByType(string type)
        //{
        //    try
        //    {
        //        //取得docs_info
        //        int docsId = this.GetSpecialNode(this._property.BusinessId);

        //        string sql = "[ParentID] = " + docsId +
        //            " and [Type] = " + (int)SupportEnumType.EnumNodeType.Node +
        //            " and [Value] like '*data_type=" + "\"" + type + "\"" + "*'";
        //        List<string> pkuuidList = new List<string>();

        //        //取得doc_info列表
        //        DataRow[] rows = this.DataManage.GData.Select(sql, "[Index] Asc");
        //        rows.ToList().ForEach(a =>
        //        {
        //            //查找doc_info的子节点pkuuid的值
        //            DataRow[] nodeRows = this.DataManage.SelectNodeRows((int)a[0],
        //                "pkuuid",
        //                SupportEnumType.EnumNodeType.Control);

        //            //添加pkuuid值到列表
        //            if (nodeRows.Length > 0)
        //                pkuuidList.Add(nodeRows[0]["Value"].ToString());
        //        });

        //        return pkuuidList.ToArray();
        //    }
        //    catch (Exception ex)
        //    {
        //        SysLog.Write(7115, ex, this._proIdAndThreadId);
        //        return null;
        //    }
        //}

        /// <summary>
        /// 根据指定的文档索引检索字段名为field的值
        /// </summary>
        /// <param name="pkuuid">文档唯一标识</param>
        /// <param name="field">字段名</param>
        /// <returns></returns>
        public string GetValueByField(string pkuuid, string field)
        {
            try
            {
                DataRow dataRow = this.dataServies.GetRowByKey(EnumType.TableType.DocTable, pkuuid);

                return (string)dataRow[field];
            }
            catch (Exception ex)
            {
                //SysLog.Write(7116, ex, this._proIdAndThreadId);
                return string.Empty;
            }
        }

        ///// <summary>
        ///// 根据指定的PKUID检索属性值
        ///// </summary>
        ///// <param name="pkuuid">文档唯一标识</param>
        ///// <param name="attribute">属性名</param>
        //public string GetDocAttribute(string pkuuid, string attribute)
        //{
        //    try
        //    {
        //        //取得docs_info节点编号
        //        int docsInfoId = this.GetSpecialNode(this._property.BusinessId);

        //        //取得doc_info节点编号
        //        int docId = this.SelectDocInfoByGuid(docsInfoId, pkuuid);

        //        if (docId == 0) return string.Empty;
        //        string nodeValue = this.DataManage.SelectNodeValue(docId);
        //        return GetNodePropertyValue(nodeValue, attribute);
        //    }
        //    catch (Exception ex)
        //    {
        //        SysLog.Write(7142, ex, this._proIdAndThreadId);
        //        return string.Empty;
        //    }
        //}

        /// <summary>
        /// 检索批次的字段名为field的值
        /// </summary>
        /// <param name="bussnessId">交易编号</param>
        /// <param name="field">字段名</param>
        public string GetValueByBusinessInfoField(string bussnessId, string field)
        {
            try
            {
                DataRow dataRow = this.dataServies.GetRowByKey(EnumType.TableType.BusinessTable, bussnessId);
                return (string)dataRow[field];
            }
            catch (Exception ex)
            {
                SysLog.Write(7142, ex, this._proIdAndThreadId);
                return string.Empty;
            }
        }

        /// <summary>
        /// 设置businessinfo的节点值
        /// </summary>
        /// <param name="bussnessId">交易编号</param>
        /// <param name="busInfo">要设置的值<节点名字，节点值></param>
        /// <returns>是否修改成功</returns>
        public bool SetValueByBusinessInfoField(string bussnessId, Dictionary<string, string> busInfo)
        {
            bool isUpdateSucceed = true;

            try
            {
                if (!string.IsNullOrEmpty(bussnessId))
                {
                    DataRow dataRow = this.dataServies.GetRowByKey(EnumType.TableType.BusinessTable, bussnessId);
                    dataRow.BeginEdit();
                    foreach (string field in busInfo.Keys)
                    {
                        dataRow[field] = busInfo[field];
                    }

                    dataRow.EndEdit();

                    if (busInfo.ContainsKey("biz_metadata1"))
                    {
                        DataRow[] docRows = this.dataServies.GetDataTable(EnumType.TableType.DocTable).Select("Biz_metadata1 = \'" + bussnessId + "\'");
                        foreach (DataRow docRow in docRows)
                        {
                            docRow.BeginEdit();

                            docRow["Biz_metadata1"] = busInfo["biz_metadata1"];
                            docRow.EndEdit();

                        }
                    }

                }
                else if (busInfo.ContainsKey("biz_metadata1"))//新建交易
                {

                    BusinessTable businessTable = (BusinessTable)this.dataServies.GetDataTable(BusinessData.EnumType.TableType.BusinessTable);
                    DataRow dataRow = businessTable.NewRow();
                    foreach (string key in busInfo.Keys)
                    {
                        dataRow[key] = busInfo[key];
                    }

                    businessTable.Rows.Add(dataRow);


                }

            }
            catch (Exception ex)
            {
                //SysLog.Write(9141, ex, this._proIdAndThreadId);
                isUpdateSucceed = false;
            }

            return isUpdateSucceed;
        }

        /*/// <summary>
        /// 检索控件名为field的模板控件标签
        /// </summary>
        /// <param name="field">控件名</param>
        public string GetTemplateControlCaption(string field)
        {
            try
            {
                foreach (var control in this._ixmlcm.ControlCollection)
                {
                    if (control.Name == field)
                        return control.AccessibleName;
                }

                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }*/

        /// <summary>
        /// 根据数据类型获取对应的pkuuid
        /// </summary>
        /// <param name="dataType">数据类型</param>
        /// <returns>pkuuid</returns>
        public string GetPkuuidByDatatype(string dataType)
        {
            try
            {
                Doc[] doc = ((DocTable)this.dataServies.GetDataTable(EnumType.TableType.DocTable)).Find("Data_type = \'" + dataType + "\'");
                if (doc.Count() == 0)
                {
                    //SysLog.Write(7151, this._proIdAndThreadId, dataType);
                    return "";
                }
                return doc[0].Pkuuid;
            }
            catch (Exception ex)
            {
                //SysLog.Write(7122, ex, this._proIdAndThreadId);
                return "";
            }

        }

        /// <summary>
        /// 删除交易
        /// </summary>
        /// <param name="businessid">交易编号</param>
        /// <returns>删除结果</returns>
        public bool DeleteBusinessInfo(string businessid)
        {
            try
            {
                //int businessNodeId = this.DataManage.GetIDByIndex(this.DataManage.GetIDByIndex(0, 0), businessIndex);

                this.dataServies.DelectRowByKey(EnumType.TableType.BusinessTable, businessid);

                this.dataServies.Commit();
                return true;
            }
            catch (Exception ex)
            {
                this.dataServies.RollBack();
                //SysLog.Write(7113, ex, this._proIdAndThreadId);
                return false;
            }
        }

        /*  delete by dy,删除此方法，新内存表没有当前交易一说，删除均走交易编号删除
        /// <summary>
        /// 删除交易
        /// </summary>
        /// <param name="businessIndex">交易节点序号</param>
        /// <returns>删除结果</returns>
        public bool DeleteCurBusinessInfo()
        {
            try
            {
                this.DataManage.DeleteByParentId(this._property.BusinessId);
                this.DataManage.OrderBy(this.DataManage.GetSpecialIdByType(0, 0));

                this.DataManage.Commit();

                return true;
            }
            catch (Exception ex)
            {
                this.DataManage.RollBack();
                SysLog.Write(7113, ex, this._proIdAndThreadId);
                return false;
            }
        }
        */

        /*#region 私有方法
        /// <summary>
        /// 初始化对象，解析XML控件模板，加载控件
        /// </summary>
        /// <param name="nodeType">节点种类/doc/page</param>
        /// <param name="key">主键</param>
        /// <param name="templateName">模板</param>
        private void InitTemplateControl(string templateXml, EnumType.TableType tableType, DataRow dataRow)
        {
            //重新构造CustomEvent事件类，避免执行重复绑定
            this._customEvent = new CustomEvent();

            //构建并显示控件
            this._ixmlcm = new ControlManage(this._control, this._customEvent, this._controls);
            this._ixmlcm.Init(templateXml);

            //绑定事件
            //this._eventFunc = new CustomEventFunc(this.dataServies, this._ixmlcm.ControlCollection, tableType, dataRow, this._xmlct.XmlRoot);
            this.BindEvent();
        }*/

        /*/// <summary>
        /// 绑定事件
        /// </summary>
        private void BindEvent()
        {
            //外部事件
            this._eventFunc.TextValidated += this.TextValidated;
            this._eventFunc.XmlValidated += this.XmlValidated;
            this._eventFunc.BizMetadata1Changed += this.BizMetadata1Changed;
            this._eventFunc.ChangeTreeNodeForeColor += this.ChangeTreeNodeForeColor;

            //内部事件
            this._customEvent.ClickOk += this._eventFunc.Click_OK;
            this._customEvent.LostFocus += this._eventFunc.LostFocus;
        }*/

        /*
        /// <summary>
        /// 给指定编号的节点添加一个GUID子节点
        /// 注：如果已经存在GUID子节点，则不再添加
        /// </summary>
        /// <param name="nodeId">节点编号</param>
        private string AddGuidNode(int nodeId)
        {
            DataRow[] rows = this.DataManage.SelectNodeRows(nodeId,
                "pkuuid",
                SupportEnumType.EnumNodeType.Control);
            string guid = Guid.NewGuid().ToString().Replace("-", "");

            if (rows.Length > 0)
            {
                //如果pkuuid已经存在，但是空值，则设置新的guid值
                if (rows[0]["Value"].Equals(string.Empty))
                    this.DataManage.UpdateValue(
                        int.Parse(rows[0]["ID"].ToString()),
                        guid);
                else
                    guid = rows[0]["Value"].ToString();
            }
            else
            {
                this.Add(nodeId,
                    "pkuuid",
                    guid,
                    SupportEnumType.EnumNodeType.Control,
                    0);
            }

            return guid;
        }
        

        /// <summary>
        /// 给指定编号的节点添加一个GUID子节点
        /// 注：如果已经存在GUID子节点，则不再添加
        /// </summary>
        /// <param name="nodeId">节点编号</param>
        /// <param name="pkuuid">传入的pkuuid</param>
        /// <returns></returns>
        private void AddGuidNode(int nodeId,string pkuuid)
        {

            DataRow[] rows = this.DataManage.SelectNodeRows(nodeId,
                "pkuuid",
                SupportEnumType.EnumNodeType.Control);

            if (rows.Length > 0)
            {
                //如果pkuuid已经存在，但是空值，则设置新的guid值
                if (rows[0]["Value"].Equals(string.Empty))
                    this.DataManage.UpdateValue(
                        int.Parse(rows[0]["ID"].ToString()),
                        pkuuid);
            }
            else
            {
                this.Add(nodeId,
                    "pkuuid",
                    pkuuid,
                    SupportEnumType.EnumNodeType.Control,
                    0);
            }

        }
        /// <summary>
        /// 给指定编号的节点添加一个uniq_metadata子节点
        /// </summary>
        /// <param name="nodeId">节点编号</param>
        /// <param name="isAddUniqMetadata">是否自动添加uniq_metadata节点</param>
        /// <param name="isAddGuid">是否自动添加uniq_metadata节点值guid</param>
        private void AddUniqMetadataNode(int nodeId, bool isAddUniqMetadata, bool isAddGuid)
        {
            string guid = Guid.NewGuid().ToString().Replace("-", "");
            DataRow[] rows = this.DataManage.SelectNodeRows(nodeId,
                "uniq_metadata",
                SupportEnumType.EnumNodeType.Control);

            if (rows.Length > 0)
            {
                if (!isAddGuid) return;

                DataRow dr = rows[0];
                if (!dr["Value"].Equals(string.Empty)) return;

                dr.BeginEdit();
                dr["Value"] = guid;
                try
                {
                    dr.EndEdit();
                }
                catch (Exception ex)
                {
                    SysLog.Write(9143, ex, this._proIdAndThreadId);
                    throw ex;
                }
            }
            else
            {
                if (!isAddUniqMetadata) return;
                if (!isAddGuid) guid = string.Empty;

                this.Add(nodeId,
                   "uniq_metadata",
                   guid,
                   SupportEnumType.EnumNodeType.Control);
            }
        }
        
        /// <summary>
        /// 根据pkuuid从docs_info里面查找doc_info
        /// </summary>
        /// <param name="docs_infoID">docs_info编号</param>
        /// <param name="pkuuid">pkuuid</param>
        private int SelectDocInfoByGuid(int docsInfoId, string pkuuid)
        {
            //枚举docs_info下面的所有的doc_info
            DataRow[] rows = this.DataManage.GetRowsByType(docsInfoId,
                SupportEnumType.EnumNodeType.Node);

            int nodeId = 0;
            foreach (var row in rows)
            {
                int id = (int)row["ID"];
                //根据pkuuid查找doc_info
                DataRow[] itemRows = this.DataManage.SelectNodeRows(
                   id,
                     "pkuuid",
                     SupportEnumType.EnumNodeType.Control);

                if (itemRows.Length > 0 &&
                    itemRows[0]["Value"].Equals(pkuuid))
                {
                    nodeId = id;
                    break;
                }
            }

            return nodeId;
        }
        
        /// <summary>
        /// 将DataRow数组中Index索引列在index处复位(所有大于等于index的记录都前移一位)
        /// 描述：复位所有大于docID索引的doc_info节点的page子节点
        /// 注：由于page节点里面包含doc_index，故doc_info删除之后，
        /// 所有的同级的大于当前删除的doc_info的编号的节点要向前移动一位，
        /// 即他们的索引将减1，故他们的page子节点里面的page_index值也要相应的减一位
        /// </summary>
        /// <param name="nodeId">父节点编号</param>
        /// <param name="index">占位索引</param>
        public void ReseizeByIndex(int nodeId, int index)
        {
            DataRow[] rows = this.DataManage.GData.Select("[parentID] = " + nodeId);

            //图像节点的XML
            string value = string.Empty;
            int currentIndex = 0;

            //排序
            //rows = rows.OrderBy(a => a["Index"]).ToArray();

            //复位
            foreach (var row in rows)
            {
                currentIndex = (int)row["Index"];

                row.BeginEdit();
                if (currentIndex >= index)
                {
                    row["Index"] = --currentIndex;
                }

                try
                {
                    row.EndEdit();
                }
                catch (Exception ex)
                {
                    SysLog.Write(9142, ex, this._proIdAndThreadId);
                    throw ex;
                }
            }
        }
        
        /// <summary>
        /// 清空父节点下面的所有后代子节点
        /// 注:父节点下面的所有子节点将删除，但父节点保留
        /// </summary>
        /// <param name="parentId"></param>
        private void ClearChilds(int parentId)
        {
            DataRow[] rowArray = this.DataManage.GData.Select("[ParentID] = " + parentId);
            foreach (var row in rowArray)
            {
                if (row["Child"].Equals("pkuuid")) continue;
                this.ClearChilds((int)row[0]);
                row.Delete();
            }
        }
        /// <summary>
        /// 删除整个doc_info节点
        /// </summary>
        /// <param name="docId">doc_info的编号</param>
        /// <param name="index">doc_info的索引</param>
        private void DeleteByDocId(int docsInfoId, int docId, int index)
        {
            this.DataManage.DeleteByParentId(docId);
            this.ReseizeByIndex(docsInfoId, index);
        }
        */
        /// <summary>
        /// 更新元数据 IndexMetadata 属性
        /// 描述：只是对Index_Metadata属性进行更新
        /// </summary>
        /// <param name="pkuuid">文档的唯一标识</param>
        /// <returns></returns>
        public void UpdateDocIndexMetadata(string pkuuid, Dictionary<string, string> dicIndexMetadata)
        {
            //取得docs_info节点编号
            //int docs_infoID = GetSpecialNode(property.BusinessID);
            DataRow dataRow = this.dataServies.GetRowByKey(EnumType.TableType.DocTable, pkuuid);

            if (dataRow == null)
            {
                return;
            }


            //取得doc_info节点编号
            //int docID = SelectDocInfoByGUID(docs_infoID, pkuuid);

            // DataRow[] drs = this.DataManage.Select("[ParentID] = " + docId);
            //string value = string.Empty;
            dataRow.BeginEdit();
            foreach (string key in dicIndexMetadata.Keys)
            {
                //dicIndexMetadata.TryGetValue(key, out value);
                dataRow[key] = dicIndexMetadata[key];

            }
            try
            {
                dataRow.EndEdit();

            }
            catch (Exception ex)
            {
                //SysLog.Write(7113, ex, this._proIdAndThreadId);
            }
        }

    }
}
