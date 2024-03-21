using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Com.Boc.Icms.LogDLL;

namespace Com.Boc.Icms.MetadataEdit.Base.Xml
{
    /// <summary>
    /// XML操作抽象类
    /// </summary>
    public abstract class XmlOperate
    {
        private XmlDocument _xmlDoc = null;

        private readonly string _proIdAndThreadId = string.Empty;

        /// <summary>
        /// XML文档
        /// </summary>
        public XmlDocument XmlDoc
        {
            get { return this._xmlDoc; }
            set { this._xmlDoc = value; }
        }
        /// <summary>
        /// 取得当前日期和时间的字符串(格式：yyyy-MM-ddTHH:mm:ssZ)
        /// </summary>
        /// <returns></returns>
        public virtual string Now
        {
            get { return DateTime.Now.ToString("s") + "Z"; }
        }

        public XmlOperate()
        {
            this._xmlDoc = new XmlDocument();
            this._proIdAndThreadId = System.Diagnostics.Process.GetCurrentProcess().Id + System.Threading.Thread.CurrentThread.ManagedThreadId.ToString();
        }

        public XmlOperate(string path)
            : this()
        {
            this.Load(path);
        }

        /// <summary>
        /// 从指定的URL加载XML文档
        /// </summary>
        /// <param name="path">XML路径</param>
        public void Load(string path)
        {
            try
            {
                this._xmlDoc.Load(path);
            }
            catch (Exception ex)
            {
                SysLog.Write(7106, ex, this._proIdAndThreadId);
                throw;
            }
        }

        /// <summary>
        /// 从指定的流XML文档
        /// </summary>
        /// <param name="stream">XML流</param>
        public void Load(Stream stream)
        {
            try
            {
                this._xmlDoc.Load(stream);
            }
            catch (Exception ex)
            {
                SysLog.Write(7107, ex, this._proIdAndThreadId);
                throw;
            }
        }

        /// <summary>
        /// 从指定的字符串加载XML文档
        /// </summary>
        /// <param name="xml">XML字符串</param>
        public void LoadXml(string xml)
        {
            try
            {
                this._xmlDoc.LoadXml(xml);
            }
            catch (Exception ex)
            {
                SysLog.Write(7108, ex, this._proIdAndThreadId);
                throw;
            }
        }

        /// <summary>
        /// XML根节点
        /// </summary>
        public XmlElement Root
        {
            get
            {
                if (this._xmlDoc != null)
                    return this._xmlDoc.DocumentElement;
                return null;
            }
        }

        /// <summary>
        /// 修改匹配的所有节点值为text
        /// </summary>
        /// <param name="xpath">xpath表达式</param>
        /// <param name="text">值</param>
        public virtual void UpdateNodesText(string xpath, string text)
        {
            this.Root.SelectNodes(xpath)
                .Cast<XmlNode>().ToList<XmlNode>().ForEach(a =>
                {
                    a.Value = text;
                });
        }

        /// <summary>
        /// 修改匹配的单个节点值为text
        /// </summary>
        /// <param name="xpath">xpath表达式</param>
        /// <param name="text">值</param>
        public virtual void UpdateNodeText(string xpath, string text)
        {
            XmlNode node = this.Root.SelectSingleNode(xpath);
            if (node != null) node.Value = text;
        }

        /// <summary>
        /// 添加命名空间（如;("bk", "urn:newbooks-schema")）
        /// </summary>
        /// <param name="prefix">命名空间前缀</param>
        /// <param name="uri">命名空间</param>
        public virtual void AddNamespace(string prefix, string uri)
        {
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(this._xmlDoc.NameTable);
            if (!nsmgr.HasNamespace(prefix)) nsmgr.AddNamespace(prefix, uri);
        }

        /// <summary>
        /// 取得指定节点的指定属性名的属性值
        /// </summary>
        /// <param name="element">节点对象</param>
        /// <param name="name">属性名</param>
        public string GetNodePropertyValue(XmlElement element, string name)
        {
            if (element.HasAttribute(name))
                return element.Attributes[name].Value;
            return string.Empty;
        }

        #region 静态方法
        /// <summary>
        /// 取得节点的属性值
        /// </summary>
        /// <param name="xml">XML字符串</param>
        /// <param name="attribute">属性名</param>
        /// <returns></returns>
        public static string GetNodePropertyValue(string xml, string attribute)
        {
            Match match = Regex.Match(xml, "((?:\\s+" + attribute + ")\\s*=\\s*)(\"([^\"]*)\")");
            if (match.Success)
                return match.Groups[3].Value;

            return string.Empty;
        }

        /// <summary>
        /// 修改节点的属性值(将替换所有匹配属性名的值)
        /// </summary>
        /// <param name="nodeID">节点编号</param>
        /// <param name="attribute">属性名</param>
        /// <param name="value">值</param>
        public static string UpdateNodeProperty(string xml, string attribute, string value)
        {
            return Regex.Replace(xml, "((?:\\s+" + attribute + ")\\s*=\\s*)(\"([^\"]*)\")", "$1" + "\"" + value + "\"");
        }

        /// <summary>
        /// 修改节点的属性值(只替换匹配属性名的指定索引位置的值)
        /// </summary>
        /// <param name="nodeID">节点编号</param>
        /// <param name="attribute">属性名</param>
        /// <param name="value">值</param>
        /// <param name="index">匹配索引</param>
        public static string UpdateNodeProperty(string xml, string attribute, string value, int index)
        {
            int i = 0;

            xml = Regex.Replace(xml,
                "((?:\\s+" + attribute + ")\\s*=\\s*)(\"([^\"]*)\")",
                delegate (Match m)
                {
                    if (i++ == index)
                        return "$1" + "\"" + value + "\"";

                    return m.Value;
                });

            return xml;
        }
        #endregion
    }
}
