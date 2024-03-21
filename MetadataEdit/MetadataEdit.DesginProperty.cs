using System.ComponentModel;
using Com.Boc.Icms.MetadataEdit.Business.BusinessData;

namespace Com.Boc.Icms.MetadataEdit
{
    /// <summary>
    /// 更在交易编号时调用
    /// 描述：当元数据更改交易编号时调用
    /// 此委托可以在外部进行进行更改文件夹名称
    /// </summary>
    /// <param name="msg">交易编号信息</param>
    public delegate void UpdateDirectory(string bizMetadata1);

    /// <summary>
    /// 显示提示信息的委托
    /// 描述：当控件输入值，只要执行验证都将产生一条消息
    /// 此委托可以在外部进行消息处理
    /// </summary>
    /// <param name="msg">错误消息</param>
    public delegate void ShowMessage(string msg);

    /// <summary>
    /// 验证XML信息的委托
    /// 描述：将XML与外部接口交互，验证输入的控件值是否有效；此XML包含父控件名、控件名、控件值。
    /// <!--格式:
    /// <父节点>
    ///     <控件名1>控件值1</控件名1>
    ///     <控件名2>控件值2</控件名2>
    /// </父节点>
    /// -->
    /// 注：此XML中的控件名与控件模板定义一致。
    /// </summary>
    /// <param name="xml">待验证的XML字符串</param>
    public delegate Task<bool> ValidateXml(string xml);

    /// <summary>
    /// 设计属性
    /// </summary>
    public partial class MetadataEdit
    {
        private string _businessControlXml = string.Empty;

        /// <summary>
        /// 设置业务XML控件模板文档
        /// </summary>
        [Category("公用控件模板组件预设"), Description("设置业务XML控件模板文档")]
        public string BusinessControlXml
        {
            get 
            { 
                return this._businessControlXml; 
            }

            set
            {
                this._businessControlXml = value;
                if (value != string.Empty) this.InitBusinessControlXml(value);
            }
        }

        private EnumType.EnumWorkType _workType = EnumType.EnumWorkType.InitScan;

        /// <summary>
        /// 设置业务工作类型
        /// </summary>
        [Category("公用控件模板组件预设"), Description("设置业务工作类型")]
        [Browsable(false)]
        public EnumType.EnumWorkType WorkType
        {
            get { return this._workType; }
            set { this._workType = value; }
        }

        private EnumType.EnumPageFlagMode _pageFlagMode = EnumType.EnumPageFlagMode.FrontAll;

        /// <summary>
        /// 业务当前的正反模式
        /// </summary>
        [Category("公用控件模板组件预设"), Description("设置业务正反模式")]
        [Browsable(false)]
        public EnumType.EnumPageFlagMode PageFlagMode
        {
            get { return this._pageFlagMode; }
            set { this._pageFlagMode = this._property.PageFlagMode = value; }
        }

        //private Control[] _externalControls = null;

        /*/// <summary>
        /// 设置外部控件对象数组,设置规则的Tag值,可在内部自定义事件中统一处理或特殊处理
        /// </summary>
        [Category("公用控件模板组件预设"),
        Description("设置外部控件对象数组,设置规则的Tag值,可在内部自定义事件中统一处理或特殊处理")]
        [Browsable(false)]
        public Control[] ExternalControls
        {
            get { return this._externalControls; }
            set { this._externalControls = value; }
        }*/

        /// <summary>
        /// 在验证控件文本时发生
        /// </summary>
        [Browsable(false)]
        public event ShowMessage TextValidated;
        
        /// <summary>
        /// 在更改交易编号时发生
        /// </summary>
        [Browsable(false)]
        public event UpdateDirectory BizMetadata1Changed;

        /// <summary>
        /// 在更改交易编号时发生
        /// </summary>
        [Browsable(false)]
        public event UpdateDirectory ChangeTreeNodeForeColor;

        /// <summary>
        /// 在验证控件文本组成的XML时发生
        /// </summary>
        [Browsable(false)]
        public event ValidateXml XmlValidated;
    }
}
