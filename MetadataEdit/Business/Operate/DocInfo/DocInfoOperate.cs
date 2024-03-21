using Com.Boc.Icms.MetadataEdit.Business.BusinessData;
using Com.Boc.Icms.MetadataEdit.Business.CustomizeEvent;
using Com.Boc.Icms.MetadataEdit.Business.Operate.DocInfo;
using Com.Boc.Icms.MetadataEdit.Support.ProviderEffect;
using Com.Boc.Icms.MetadataEdit.Support.SupportControl.TemplateControl;
using Com.Boc.Icms.MetadataEdit.Base.XML;
using Com.Boc.Icms.MetadataEdit.Business.Operate;
using Com.Boc.Icms.MetadataEdit.Support.Template;

namespace Com.Boc.Icms.MetadataEdit.Business.Operate.DocInfo
{
    /// <summary>
    /// 文档操作类
    /// </summary>
    partial class DocInfoOperate : XmlNodeOperate, IDocInfocOperate
    {
        private Property property;
        private IXmlDataImport ixmlfe;
        private CustomEvent customEvent;
        private IXmlTemplateCache ixmlct;
        private Controls controls;
        private IControlManage ixmlcm;
        private CustomEventFunc eventFunc;
        private ContainerControl control;

        ///<summary>
        /// 在验证控件文本时发生
        /// </summary>
        public event ShowMessage TextValidated;

        /// <summary>
        /// 在验证控件文本组成的XML时发生
        /// </summary>
        public event ValidateXml XmlValidated;

        public DocInfoOperate(ContainerControl control, DataManage dataManage, Property property,
            IXmlDataImport ixmlfe, IXmlTemplateCache ixmlct)
            : base(dataManage)
        {
            controls = new Controls();
            customEvent = new CustomEvent();

            this.property = property;
            this.ixmlfe = ixmlfe;
            this.ixmlct = ixmlct;
            this.control = control;
           // this.ixmlcm = new XmlControlManage(control, customEvent, controls);
        }

        /// <summary>
        /// 初始化加载文档类型docs_info的XML
        /// 
        /// 描述：docs_info包含很多doc_info，解析并保存到数据结构中。
        /// </summary>
        /// <param name="xml">xml字符串</param>
        /// <param name="type">业务工作类型</param>
        public void InitXml(string xml, EnumType.EnumWorkType type)
        {
            try
            {
                ixmlfe.ParentID = dataManage.GetIDByIndex(ixmlfe.ParentID, 0);
                ixmlfe.SaveXml(xml);

                //判断文档是否需要添加GUID节点
                DataRow[] rows = dataManage.GetRowsByType(ixmlfe.ParentID,
                    EnumType.EnumNodeType.Node);
                rows.ToList().ForEach(a => AddGuidNode((int)a["ID"]));
            }
            catch
            {
                throw new Exception("初始化加载文档XML出错！");
            }
        }

        /// <summary>
        /// 添加单个文档
        /// </summary>
        /// <param name="data_type">数据类型</param>
        public void AddDoc(string data_type)
        {
            try
            {
                //取得docs_info节点编号
                int docs_infoID = CheckNode(property.BusinessID, "docs_info");

                string xml = "<doc_info archieved=\"Y\"" +
                    " data_type=\"" + data_type + "\"" +
                    " security=\"0\"" +
                    " expire_date=\"" + Now + "\"" +
                    " modi_time=\"" + Now + "\"" +
                    " new_version=\"Y\"" +
                    " full_index=\"Y\"" +
                    " modi_meta=\"Y\"" +
                    " oper_type=\"A\"></doc_info>";

                int docID = base.Add(docs_infoID,
                    "doc_info",
                    xml,
                    EnumType.EnumNodeType.Node);

                //新增文档默认添加一个pkuid节点
                AddGuidNode(docID);
            }
            catch
            {
                throw new Exception("添加文档出错！");
            }
        }

        /// <summary>
        /// 删除单个文档
        /// </summary>
        /// <param name="doc_index">文档索引</param>
        public void DeleteDoc(int doc_index)
        {
            try
            {
                //取得docs_info节点编号
                int docs_infoID = GetSpecialNode(property.BusinessID);

                //取得doc_info节点编号
                int docID = dataManage.GetIDByIndex(docs_infoID, doc_index);

                DataRow dr = dataManage.SelectNodeRow(docID);
                int parentId = (int)dr["ParentID"];
                int index = (int)dr["Index"];

                dataManage.DeleteByParentID(docID);
                ReseizeByIndex(parentId, index);
            }
            catch
            {
                throw new Exception("删除文档出错！");
            }
        }

        /// <summary>
        /// 移动文档
        /// </summary>
        /// <param name="doc_index">文档索引</param>
        /// <param name="old_doc_index">初始文档索引</param>
        public void MoveDoc(int doc_index, int old_doc_index)
        {
            try
            {
                //取得docs_info节点编号
                int docs_infoID = GetSpecialNode(property.BusinessID);

                //取得初始doc_info节点编号
                int old_docID = dataManage.GetIDByIndex(docs_infoID, old_doc_index);

                //取得doc_info节点编号
                int docID = dataManage.GetIDByIndex(docs_infoID, doc_index);

                base.MoveNode(old_docID, docs_infoID, doc_index);

                //更新初始文档与目标文档下面所有图像的doc_index属性
                base.SynNodeProperty(old_docID, "doc_index", doc_index.ToString());

                base.SynNodeProperty(docID, "doc_index", old_doc_index.ToString());

                if (property.WorkType == EnumType.EnumWorkType.InitScan)
                {
                    UpdateNodePropertyAndTime(old_docID, "oper_type", "A");

                    UpdateNodePropertyAndTime(docID, "oper_type", "A");
                }
            }
            catch
            {
                throw new Exception("移动文档位置出错！");
            }
        }

        /// <summary>
        /// 切换节点对应的XML控件模板
        /// 
        /// 描述：根据指定的模板名进行模板检索，并生成控件。根据指定的文档索引查找控件值，并填充控件。
        /// </summary>
        /// <param name="doc_index">文档索引(如何小于0的数都代表Business_info根节点)</param>
        /// <param name="templateName">模板名</param>
        public void ChangeXmlNode(int doc_index, string templateName)
        {
            //模板为空则移除所有控件
            if (templateName == string.Empty)
            {
                control.Controls.Clear();
                return;
            }

            try
            {
                //控件的父节点编号
                int nodeID = 0;

                //从模板列表中检索模板
                string xml = ixmlct.FindTemplate(templateName);

                //从缓存中取得字段值，并填充模板的控件值
                if (doc_index < 0)
                {
                    nodeID = property.BusinessID;
                }
                else
                {
                    //取得docs_info节点编号
                    int docs_infoID = GetSpecialNode(property.BusinessID);

                    nodeID = dataManage.GetIDByIndex(docs_infoID,
                        doc_index,
                        EnumType.EnumNodeType.Node);
                }

                ixmlfe.ParentID = nodeID;

                string templateXml = ixmlct.FillTemplate(xml, nodeID, dataManage);

                //加载模板控件
                InitTemplateControl(templateXml, nodeID);
            }
            catch
            {
                throw new Exception("切换节点对应的XML控件模板出错！");
            }
        }

        /// <summary>
        /// 根据指定的数据类型检索文档的PKUUID列表
        /// </summary>
        /// <param name="type">数据类型</param>
        /// <returns></returns>
        public string[] GetGuidByType(string type)
        {
            try
            {
                //取得docs_info
                int docsID = GetSpecialNode(property.BusinessID);

                string sql = "[ParentID] = " + docsID +
                    " and [Type] = " + (int)EnumType.EnumNodeType.Node +
                    " and [Value] like '*data_type=" + "\"" + type + "\"" + "*'";
                List<string> pkuuidList = new List<string>();

                //取得doc_info列表
                DataRow[] rows = dataManage.GData.Select(sql, "[Index] Asc");
                rows.ToList().ForEach(a =>
                {
                    //查找doc_info的子节点pkuuid的值
                    DataRow[] nodeRows = dataManage.SelectNodeRows((int)a[0],
                        "pkuuid",
                        EnumType.EnumNodeType.Control);

                    //添加pkuuid值到列表
                    if (nodeRows.Length > 0)
                        pkuuidList.Add(nodeRows[0]["Value"].ToString());
                });

                return pkuuidList.ToArray();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 根据指定的文档索引检索字段名为field的值
        /// </summary>
        /// <param name="doc_index">文档索引</param>
        /// <param name="field">字段名</param>
        /// <returns></returns>
        public string GetValueByField(int doc_index, string field)
        {
            try
            {
                //取得docs_info节点编号
                int docs_infoID = GetSpecialNode(property.BusinessID);

                //取得doc_info节点编号
                int docID = dataManage.GetIDByIndex(docs_infoID, doc_index);

                return dataManage.SelectNodeValue(docID, field, EnumType.EnumNodeType.Control);
            }
            catch
            {
                return string.Empty;
            }
        }

        #region 私有方法
        /// <summary>
        /// 初始化对象，解析XML控件模板，加载控件
        /// </summary>
        /// <param name="templateXml">XML格式字符串</param>
        /// <param name="nodeID">控件父节点编号</param>
        private void InitTemplateControl(string templateXml, int nodeID)
        {
            //重新构造CustomEvent事件类，避免执行重复绑定
            customEvent = new CustomEvent();

            //构建并显示控件
            ixmlcm = new ControlManage(control, customEvent, controls);
            ixmlcm.Init(templateXml);

            //绑定事件
            eventFunc = new CustomEventFunc(dataManage, ixmlcm.ControlCollection, nodeID);
            BindEvent();
        }

        /// <summary>
        /// 绑定事件
        /// </summary>
        private void BindEvent()
        {
            //外部事件
            eventFunc.TextValidated += this.TextValidated;
            eventFunc.XmlValidated += this.XmlValidated;

            //内部事件
            customEvent.ClickOK += eventFunc.Click_OK;
            customEvent.LostFocus += eventFunc.LostFocus;
        }

        /// <summary>
        /// 给指定编号的节点添加一个GUID子节点
        /// 
        /// 注：如果已经存在GUID子节点，则不再添加
        /// </summary>
        /// <param name="nodeID">节点编号</param>
        private void AddGuidNode(int nodeID)
        {
            DataRow[] rows = dataManage.SelectNodeRows(nodeID,
                "pkuuid",
                EnumType.EnumNodeType.Control);
            if (rows.Length > 0) return;

            base.Add(nodeID,
                "pkuuid",
                System.Guid.NewGuid().ToString().Replace("-",""),
                EnumType.EnumNodeType.Control,
                0);
        }

        /// <summary>
        /// 将DataRow数组中Index索引列在index处复位(所有大于等于index的记录都前移一位)
        /// 
        /// 描述：复位所有大于docID索引的doc_info节点的page子节点
        /// 
        /// 注：由于page节点里面包含doc_index，故doc_info删除之后，
        ///     所有的同级的大于当前删除的doc_info的编号的节点要向前移动一位，
        ///     即他们的索引将减1，故他们的page子节点里面的page_index值也要相应的减一位
        /// </summary>
        /// <param name="nodeID">父节点编号</param>
        /// <param name="index">占位索引</param>
        public void ReseizeByIndex(int nodeID, int index)
        {
            DataRow[] rows = this.dataManage.GData.Select("[parentID] = " + nodeID);

            //图像节点的XML
            string value = string.Empty;
            int currentIndex = 0;
            int pagesID = 0;
            DataRow[] nodeRows = null;

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

                    //同步doc_index属性
                    nodeRows = dataManage.SelectNodeRows((int)row[0],
                       "pages",
                       EnumType.EnumNodeType.Node);

                    if (nodeRows.Length > 0)
                    {
                        pagesID = (int)nodeRows[0][0];

                        //更新page节点中的page_index属性
                        nodeRows = dataManage.GetRowsByType(pagesID,
                            EnumType.EnumNodeType.Node);

                        foreach (var pageRow in nodeRows)
                        {
                            pageRow["Value"] = XMLOperate.UpdateNodeProperty(
                                pageRow["Value"].ToString(),
                               "doc_index",
                               currentIndex.ToString());
                        }
                    }
                }
                row.EndEdit();
            }

            this.dataManage.GData.AcceptChanges();
        }
        #endregion
    }
}
