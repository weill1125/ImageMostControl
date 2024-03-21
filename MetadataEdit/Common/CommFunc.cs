using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CCTW.Support;

namespace CCTW.Common
{
    /// <summary>
    /// 公共操作类
    /// </summary>
    public class CommFunc
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
            catch {
                throw new Exception("STYLE样式格式错误！");
            }
        }
    }
}
