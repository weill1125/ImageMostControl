using System;
using System.Linq;
using System.Xml;
using Com.Boc.Icms.MetadataEdit.Support.GlobalCache;

using Com.Boc.Icms.LogDLL;
using Com.Boc.Icms.MetadataEdit.Base.Xml;
using System.Data;

namespace Com.Boc.Icms.MetadataEdit.Support.Template
{
    /// <summary>
    /// XML控件模板缓存类
    /// 可根据制定的模板名找到执行的模板
    /// </summary>
    public class XmlTemplateCache : XmlOperate, IXmlTemplateCache
    {
        public XmlElement XmlRoot = null;
        private readonly string _proIdAndThreadId = string.Empty;

        public XmlTemplateCache()
            : base()
        {
            this._proIdAndThreadId = System.Diagnostics.Process.GetCurrentProcess().Id + System.Threading.Thread.CurrentThread.ManagedThreadId.ToString();
        }

        /// <summary>
        /// 加载XML控件模板
        /// </summary>
        /// <param name="xml">xml字符串</param>
        public new bool LoadXml(string xml)
        {
            try
            {
                base.LoadXml(xml);
                return true;
            }
            catch (Exception ex)
            {
                SysLog.Write(7108, ex, this._proIdAndThreadId);
                return false;
            }
        }

        /// <summary>
        /// 查找XML业务控件模板中对应的节点模板
        /// </summary>
        /// <param name="name">模板名</param>
        /// <returns></returns>
        public string FindTemplate(string name)
        {
            try
            {
                string xpath = "descendant::Template[@Name='" + name + "']";
                
                return this.Root.SelectSingleNode(xpath).OuterXml;
               
            }
            catch (Exception ex)
            {
                SysLog.Write(7139, ex.ToString(), this._proIdAndThreadId);
                return string.Empty;
            }
        }

        //将XML字符串放到全局静态变量中以便判断时使用
        //public static XmlElement xmlRoot;
        //public static DataManage UniqueDataManage;

        /// <summary>
        /// 填充控件的值
        /// </summary>
        /// <param name="xml">xml字符串</param>
        /// <param name="nodeId">控件节点在缓存中对应的父节点编号</param>
        /// <param name="dataManage">数据管理类</param>
        public string FillTemplate(string xml, DataRow dataRow)
        {
            //控件填充值
            string text = string.Empty;
            string currentTemplate = string.Empty;
            string rootClone = this.Root.Clone().OuterXml;

            try
            {
                //控件模板xml的克隆

                //获得xml中每一项的模板
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);
                XmlElement xmlExement = doc.DocumentElement;
                this.XmlRoot = xmlExement;
                //UniqueDataManage = dataManage;


                //加载控件模板XML
                this.LoadXml(xml);

                //为控件模板填充数据
                
                this.Root.ChildNodes
                    .Cast<XmlElement>().ToList()
                    .ForEach(a =>
                    {
                        if (a.HasAttribute("Name"))
                        {
                            //dataRow 列不区分大小写，故直接用属性取值

                            string rowName = a.Attributes["Name"].Value;

                            if(dataRow.Table.Columns.Contains(rowName))
                            {
                                text = (string)dataRow[rowName];

                                //if (text == string.Empty) return;

                                if (a.HasAttribute("Text"))
                                {
                                    a.Attributes["Text"].Value = text;
                                }
                                else
                                {
                                    a.SetAttribute("Text", text);
                                }
                            }
                            
                        }
                    });

                currentTemplate = this.Root.OuterXml;

                //恢复控件模板xml
                
                return currentTemplate;
            }
            catch (Exception ex)
            {
                SysLog.Write(7144, ex, this._proIdAndThreadId);
                return string.Empty;
            }
            finally
            {
                this.LoadXml(rootClone);
            }
        }
    }
}
