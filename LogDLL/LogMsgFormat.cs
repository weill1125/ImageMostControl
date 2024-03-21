using System;
using System.Collections.Generic;
using System.Text;


namespace Com.Boc.Icms.LogDLL
{
    public class LogMsgFormat
    {
        /// <summary>
        /// 返回方法参数值（Dictionary）
        /// </summary>
        /// <typeparam name="T1">字典键的类型</typeparam>
        /// <typeparam name="T2">字典值的类型</typeparam>
        /// <param name="dictionary">字典类参数</param>
        /// <returns>String</returns>
        public static string LogValue_Dictionary<T1, T2>(Dictionary<T1, T2> dictionary)
        {
            try
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("{");
                foreach (T1 key in dictionary.Keys)
                {
                    if (key != null && dictionary[key] != null)
                    {
                        builder.Append(key + ":" + dictionary[key] + ",");
                    }
                }

                return builder.ToString().TrimEnd(',') + "}";
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /*/// <summary>
        /// 返回方法参数值（Dictionary）
        /// </summary>
        /// <typeparam name="T1">字典键的类型</typeparam>
        /// <typeparam name="T2">字典值的类型</typeparam>
        /// <param name="dictionary">字典类参数</param>
        /// <returns>String</returns>
        public static string LogValue_Dictionary<T1, T2>(Dictionary<T1, List<T2>> dictionary)
        {
            try
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("[");
                foreach (T1 key in dictionary.Keys)
                {
                    if (key != null && dictionary[key] != null)
                    {
                        if (dictionary[key].Count > 0)
                        {
                            string str = LogValue_List(dictionary[key]);
                            builder.Append(key + ":" + "{" + str + "}");
                        }
                    }
                }

                builder.Append("]");

                return builder.ToString();
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }*/

        /*/// <summary>
        /// 返回方法参数值（List）
        /// </summary>
        /// <typeparam name="T">List值的类型</typeparam>
        /// <param name="list">List类型</param>
        /// <returns>String</returns>
        public static string LogValue_List<T>(List<T> list)
        {
            try
            {
                StringBuilder builder = new StringBuilder();
                if (list is List<string>)
                {
                    string str = String.Join(",", (list as List<string>).ToArray());
                    builder.Append("{" + str + "}");
                }
                else if (list is List<TreeNode>)
                {
                    string str = "";
                    foreach (TreeNode node in list as List<TreeNode>)
                    {
                        str += node.Name + ",";
                    }
                    if (str.Length > 0)
                    {
                        str = str.Substring(0, str.Length - 1);
                        builder.Append("{" + str + "}");
                    }
                }
                
                return builder.ToString();
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }*/

        /// <summary>
        /// 返回方法参数值（Array）
        /// </summary>
        /// <typeparam name="T">List值的类型</typeparam>
        /// <param name="list">List类型</param>
        /// <returns>String</returns>
        public static string LogValue_Array<T>(T[] arraylist)
        {
            try
            {
                StringBuilder builder = new StringBuilder();
                string str = String.Join(",", arraylist as string[]);
                builder.Append("{" + str + "}");
                return builder.ToString();
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}
