using System;
using System.Linq;
using System.Windows.Forms;
using Com.Boc.Icms.LogDLL;

namespace Com.Boc.Icms.MetadataEdit.Support.ProviderEffect
{
    /// <summary>
    /// 公共操作类
    /// </summary>
    public class SupprotCommFunc
    {
        /// <summary>
        /// 设置控件样式
        /// </summary>
        /// <param name="control"></param>
        /// <param name="style"></param>
        public void SetControlStyle(Control control, string style)
        {
            try
            {
                style.Split(';').ToList().ForEach(a =>
                {
                    string[] styleProperty = a.Split(':');

                    switch (styleProperty[0].ToUpperInvariant())
                    {
                        case "WIDTH":
                            control.Width = int.Parse(styleProperty[1]);
                            break;
                        case "HEIGHT":
                            control.Height = int.Parse(styleProperty[1]);
                            break;
                        default:
                            break;
                    }
                });
            }
            catch (Exception ex)
            {
                SysLog.Write(7138,ex, System.Diagnostics.Process.GetCurrentProcess().Id + System.Threading.Thread.CurrentThread.ManagedThreadId.ToString());
            }
        }
    }
}
