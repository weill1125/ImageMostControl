namespace Com.Boc.Icms.GlobalResource
{
    public class GlobalFinalProperties
    {
        /// <summary>
        /// 产品所属公司名称
        /// </summary>
        public static readonly string CompanyName = "Founder";

        /// <summary>
        /// 产品名称
        /// </summary>
        public static readonly string SystemName = "ICMS";

        /// <summary>
        /// 项目版本号
        /// </summary>
        public static readonly string VersionNo = "V01.0C";

        /// <summary>
        /// 全局注册路径
        /// </summary>
        public static string GlobalRegeditPath
        {
            get
            {
                return Path.Combine( "SOFTWARE" , CompanyName , SystemName , VersionNo );
            }
        }

        public static string GlobalConfigPath
        {
            get
            {
                //return Path.Combine("/home", CompanyName, SystemName, VersionNo);
                return GlobalInstallPath;
            }
        }

        /// <summary>
        /// 全局安装路径
        /// </summary>
        public static string GlobalInstallPath
        {
            get
            {
                try
                {
                    //todo
                    /*//从注册表中读取LOG日志配置文件路径与系统日志的源名称
                    Microsoft.Win32.RegistryKey key =
                        Microsoft.Win32.Registry.LocalMachine.OpenSubKey(GlobalRegeditPath);
                    return key.GetValue("Path").ToString();*/
                    return AppDomain.CurrentDomain.BaseDirectory;
                }
                catch
                {
                    return string.Empty;
                }
            }
        }
    }
}
