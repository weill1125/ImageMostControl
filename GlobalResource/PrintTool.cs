using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.Boc.Icms.GlobalResource
{
	
	public class PrintTool
    {
		
		private List<string> _printers = null;
		private string _proIdAndThreadId = string.Empty;
        /// <summary>
        /// 加载打印机
        /// </summary>
        public async Task<List<string>> PrinterItemsAsync()
        {
            await Task.Run(() =>
            {
				_printers = new List<string>();
				try
                {
                    Process process = new Process();
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.FileName = "lpstat";
                    process.StartInfo.Arguments = " -p";
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.Start();

                    StreamReader Outputresults = process.StandardOutput;


                    while (!Outputresults.EndOfStream)
                    {
                        string[] items = Outputresults.ReadLine().Split(new char[] { ' ' });

                        if (!_printers.Contains(items[1]))
                            _printers.Add(items[1]);
                        Console.WriteLine(items[1]);
                    }
                    Outputresults.Close();
                    process.WaitForExit(3);
                    //if (!_printers.Contains("打印机"))
                    //    _printers.Add("打印机");
                    //Task.Delay(1500);
                }
				catch (Exception ex)
                {
					
					throw new Exception( AccessResource.GetInstance().GetValue("GetPrinterFail"));
                   
                }
            });

            return _printers;
        }


        /// <summary>
        /// 执行打印命令
        /// </summary>
        /// <returns></returns>
        public async Task PrintFiles(string arguments,string filepaths)
        {
			
            await Task.Run(() =>
            {
				try
				{
                    Console.WriteLine("执行打印");
                    Process process = new Process();
                    process.StartInfo.UseShellExecute = true;
                    process.StartInfo.FileName = "lp";
                    process.StartInfo.Arguments = arguments + filepaths;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.RedirectStandardOutput = false;
                    process.Start();
                    process.WaitForExit(3);

                    //Console.WriteLine("正在打印");
                    //Task.Delay(500);
                }
				catch (Exception ex)
				{
					
					throw new Exception(AccessResource.GetInstance().GetValue("PrintingFail"));
				}
			}); 
           
           
        }

    }
}
