namespace Com.Boc.Icms.MetadataEdit.Support.GlobalCache
{
    public class SupportEnumType
    {
        /// <summary>
        /// 放入控件Tag中的XML属性
        /// 描述：标记可在自定义的事件中处理的XML属性
        /// </summary>
        public enum EnumControlTag : int
        {
            /// <summary>
            /// 验证
            /// </summary>
            Validate,

            /// <summary>
            /// 验证组
            /// </summary>
            ValidateGroup,

            /// <summary>
            /// 错误消息
            /// </summary>
            ErrorMessage,

            /// <summary>
            /// 自动保存
            /// </summary>
            AutoSave
        }

        /// <summary>
        /// 节点类型
        /// </summary>
        public enum EnumNodeType : int
        {
            /// <summary>
            /// 节点
            /// </summary>
            Node = 0,

            /// <summary>
            /// 控件
            /// </summary>
            Control = 1
        }
    }
}
