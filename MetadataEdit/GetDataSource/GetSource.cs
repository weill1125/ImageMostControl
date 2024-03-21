using AntDesign;
using Com.Boc.Icms.LogDLL;
using Com.Boc.Icms.MetadataEdit.Business.BusinessData;
using Com.Boc.Icms.MetadataEdit.DataTables;
using Com.Boc.Icms.MetadataEdit.Services;
using Com.Boc.Icms.MetadataEdit.Support.GlobalCache;
using System.Data;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;


namespace Com.Boc.Icms.MetadataEdit.GetDataSource
{
	public delegate Task<ConfirmResult> ShowMsgBox(string content, string title, ConfirmButtons confirmButtons, ConfirmIcon confirmIcon);

	public class GetSource
	{
		public ShowMsgBox ShowMsgBox = null;

		public string templateXml;

		public Dictionary<EnumType.TableType,DataRow> dataRow = new Dictionary<EnumType.TableType,DataRow>();

		public DataServies dataServies;

		public EnumType.TableType tableType;

		public XmlNode xmlnode;

		private readonly int _parentId = 0;

		private readonly string _proIdAndThreadId = System.Diagnostics.Process.GetCurrentProcess().Id + System.Threading.Thread.CurrentThread.ManagedThreadId.ToString();
		
		public event ShowMessage TextValidated = null;

		private  XmlElement[] _controlArray = null;

		public DataRow data;

		public event UpdateDirectory BizMetadata1Changed = null;
		//改变树节点的前景颜色
		public event UpdateDirectory ChangeTreeNodeForeColor = null;

		public event ValidateXml XmlValidated = null;

        public string backgroundcolor;

		public string Key;

        public async Task Click_OK(XmlNode _xmlnode)
		{
			xmlnode = _xmlnode;
			data = dataServies.GetRowByKey(tableType, Key); 
			//判断控件是否修改元数据
			bool isEdit = false;
			int checkup = 0;
			Dictionary<string, string> dic = new Dictionary<string, string>();

			//判断是否弹出提示框并且将变红的Doc节点置黑
			//bool isHint = false;
			string hintMessage = string.Empty;
			string message = "";

			_controlArray = new XmlElement[_xmlnode.ChildNodes.Count];
			int i = 0;
			foreach (XmlElement item in _xmlnode)
			{
				_controlArray[i] = item;
				i++;

				if (item.Attributes["Type"].Value == "Button")
				{
					continue;
				}
				checkup = this.ReadXmlData(item.Attributes["Name"].Value, false, item.Attributes["Text"].Value, this._parentId, ref message);
				if (checkup == 1)
				{
					//检查失败，返回
					this.ShowMsg(message, false);
					return;
				}
			}

			//检测是否自动保存
			if (!this.CheckAutoSave(_xmlnode))
			{
				if (!await this.ValidateAllControlByClick(_xmlnode)) return;
				if (!await this.ValidateGroupXmlByClick(_xmlnode)) return;
			}

			//置黑
		    //todo
			if (this.ChangeTreeNodeForeColor != null)
			{
				this.ChangeTreeNodeForeColor(null);
			}

			try
			{
				
				data.BeginEdit();
				this._controlArray.Where(a => a.Attributes["Type"].Value != "Button")
					.ToList().ForEach(a =>
					{
						if (this.data.Table.Columns.Contains(a.Attributes["Name"].Value)
							&& !a.Attributes["Text"].Value.Trim().Equals(this.data[a.Attributes["Name"].Value]))
						{
							isEdit = true;

							data.BeginEdit();
							//数据结构中存在相同的控件节点记录，则执行修改
							string oldbizname = this.data[a.Attributes["Name"].Value].ToString();
							this.data[a.Attributes["Name"].Value] = a.Attributes["Text"].Value.Trim();
							data.EndEdit();
							//如果为交易号信息，更新左边文件夹
							if (a.Attributes["Name"].Value.Equals("biz_metadata1"))
							{
								DocTable doc = (DocTable)this.dataServies.GetDataTable(EnumType.TableType.DocTable);
								DataRow[] dataRows = doc.Select("[" + a.Attributes["Name"].Value + "]='" + oldbizname + "'");
								for (int i = 0; i < dataRows.Length; i++)
									dataRows[i][a.Attributes["Name"].Value] = a.Attributes["Text"].Value.Trim();

								if (this.BizMetadata1Changed != null)
								{
									this.BizMetadata1Changed(a.Attributes["Text"].Value.Trim());
								}

							}
						}					

					});

				if (isEdit)
				{
					//数据结构中存在相同的控件节点记录，则执行修改

					if (this.data.Table.Columns.Contains("oper_type") && !"A".Equals(this.data["oper_type"]))
					{
						this.data["oper_type"] = "E";
					}
					this.data["modi_meta"] = "Y";

					this.ShowMsg(message + SysLog.GetMessage(9100), false);
				}
				this.data.EndEdit();
				if (isEdit)
				{
					this.dataServies.Commit();
				}
			}
			catch (Exception ex)
			{
				this.dataServies.RollBack();
				this.ShowMsg(SysLog.GetMessage(7109) + ex.Message, true);
				SysLog.Write(7109, ex, this._proIdAndThreadId);
			}

		}


		public void LostFocus(XmlElement item)
		{
			try
			{
				data = dataServies.GetRowByKey(tableType, Key);
				_controlArray = new XmlElement[item.ParentNode.ChildNodes.Count];
				int i = 0;
				foreach (XmlElement xmlitem in item.ParentNode)
				{
					_controlArray[i] = xmlitem;
					i++;
				}
					if (item != null)
					this.ValidateInput(item);

			}
			catch (Exception ex)
			{
				this.ShowMsg(SysLog.GetMessage(7110) + ex.Message, true);
				SysLog.Write(7110, ex, this._proIdAndThreadId);
			}
		}

		/// <summary>
		/// 唯一性检查
		/// 0.不进行检查；1.检查失败，返回；2.检查通过，但不保存。3.检查通过，保存
		/// </summary>
		/// <param name="ctlName">控件的Name值</param>
		/// <param name="isAutoSave">是否自动保存</param>
		/// <param name="ctlText">控件对应的值</param>
		/// <param name="parentId">父节点的ID</param>
		/// <param name="message">相关状态记录的信息</param>
		/// <returns></returns>
		private int ReadXmlData(string ctlName, bool isAutoSave, string ctlText, int parentId, ref string message)
		{		
			int checkup = 1;

			if (this.xmlnode != null)
			{
				//为控件模板填充数据
				foreach (XmlElement xeChild in xmlnode)
				{
					if (xeChild.HasAttribute("Name"))
					{
						string isUnique = string.Empty;
						string isSubmit = string.Empty;

						if (isAutoSave)
						{
							if (xeChild.GetAttribute("Name") == ctlName)
							{
								checkup = this.UniqueCheckup(xeChild, ctlName, ctlText, ref message);
								//检查失败
								if (checkup == 1)
								{
									return 1;
								}

								return checkup;
							}
						}
						else
						{
							if (xeChild.GetAttribute("Name") == ctlName)
							{
								checkup = this.UniqueCheckup(xeChild, ctlName, ctlText, ref message);
								if (checkup == 1)
								{
									return 1;
								}
							}

							
						}
					}
				}
			}

			return checkup;
		}
		/// <summary>
		/// 唯一性检查
		/// 0：不进行检查，返回；1.检查失败，返回；2.检查通过。3.检查不通过，保存
		/// </summary>
		/// <param name="xe">XmlElement对象，去属性值</param>
		/// <param name="index_Metadata">index_Metadata属性</param>
		/// <param name="childText">子节点的名称对应的值</param>
		/// <param name="parentId">父节点的ID</param>
		/// <param name="message">相关状态记录的信息</param>
		/// <returns></returns>
		private int UniqueCheckup(XmlElement xe, string indexMetadata, string childText, ref string message)
		{
			//DataManage dm = XmlTemplateCache.UniqueDataManage;

			string isUnique = string.Empty;
			string isSubmit = string.Empty;

			if (xe.GetAttribute("Name") == indexMetadata)
			{
				if (xe.HasAttribute("IsUnique"))
				{
					isUnique = xe.GetAttribute("IsUnique");

					if (isUnique == "False")
					{
						//不进行唯一性检查
						return 0;
					}

					if (isUnique == "True")
					{
						bool isTure = this.UniqueCheckup(this.dataServies.GetDataTable(this.tableType), indexMetadata, childText);//唯一性检查

						if (isTure) //唯一性检查失败
						{
							if (xe.HasAttribute("IsSubmit"))
							{
								isSubmit = xe.GetAttribute("IsSubmit");

								if (isSubmit == "True")
								{
									if (xe.HasAttribute("TextMessage"))
									{
										if (string.IsNullOrEmpty(xe.GetAttribute("TextMessage")))
										{
											message = xe.GetAttribute("Caption").Trim() + GlobalResource.AccessResource.GetInstance().GetValue("144") + " ";
											return 3;
										}
										message = xe.GetAttribute("TextMessage") + " ";
										return 3;
									}
									message = xe.GetAttribute("Caption").Trim() + GlobalResource.AccessResource.GetInstance().GetValue("144") + " ";
									return 3;
								}
								if (xe.HasAttribute("TextMessage"))
								{
									if (string.IsNullOrEmpty(xe.GetAttribute("TextMessage")))
									{
										message = xe.GetAttribute("Caption").Trim() + GlobalResource.AccessResource.GetInstance().GetValue("144") + " ";
										return 1;
									}
									message = xe.GetAttribute("TextMessage") + " ";
									return 1;
								}
								message = xe.GetAttribute("Caption").Trim() + GlobalResource.AccessResource.GetInstance().GetValue("144") + " ";
								return 1;
							}
							if (xe.HasAttribute("TextMessage"))
							{
								if (string.IsNullOrEmpty(xe.GetAttribute("TextMessage")))
								{
									message = xe.GetAttribute("Caption").Trim() + GlobalResource.AccessResource.GetInstance().GetValue("144") + " ";
									return 1;
								}
								message = xe.GetAttribute("TextMessage") + " ";
								return 1;
							}
							message = xe.GetAttribute("Caption").Trim() + GlobalResource.AccessResource.GetInstance().GetValue("144") + " ";
							return 1;
						}
						return 2;
					}
				}
			}

			return 0;
		}


		/// <summary>
		/// 唯一性检查,如果不唯一返回true，唯一返回false
		/// </summary>
		/// <param name="dm">docsinfo对应的内存表中的数据</param>
		/// <param name="childName">子节点的名称（如：index_metadata1，index_metadata2，index_metadata3，ext_metadata）</param>
		/// <param name="childText">子节点的名称对应的值</param>
		/// <returns></returns>
		private bool UniqueCheckup(DataTable dt, string childName, string childText)
		{
			//检查唯一性
			DataRow[] drs = dt.Select("" + childName + "= \'" + childText + "\'");
			//List<string> list = new List<string>();

			if (drs.Length <= 0)
			{
				return false;
			}
			else
			{
				return true;
			}

		}


		/// <summary>
		/// 判断是否自动保存
		/// </summary>
		/// <param name="control">控件对象</param>
		/// <param name="dic">面板子控件Tag中存取的XML属性值</param>
		private bool CheckAutoSave(XmlNode _xmlnode)
		{
			//定义在Dictionary中根据指定的键搜索的值存取到的变量
			string value = string.Empty;
						
			XmlElement xml = _xmlnode as XmlElement;

			if (xml.HasAttribute("AutoSave"))
			{
				value = xml.Attributes["AutoSave"].Value;
				return bool.Parse(value);
			}
			else
				return false;
			
		}
	

		/// <summary>
		/// 点击按钮执行内部验证
		/// </summary>
		private async Task<bool> ValidateAllControlByClick(XmlNode _xmlnode)
		{
			foreach (XmlElement item in _xmlnode)
			{
				if (!await this.ValidateInput(item)) return false;
			}

			return true;
		}


		/// <summary>
		/// 根据判断Tag中的验证属性，验证输入值
		/// 注：先执行内部验证，成功后判断是否自动保存，
		/// 是则执行外部验证并进行保存，
		/// 否则等待按钮点击之后执行外部校验，并保存
		/// </summary>
		/// <param name="control">控件对象</param>
		private async Task<bool> ValidateInput(XmlElement item)
		{
			//获取控件文本
			string strText = item.Attributes["Text"].Value;
			//取得面板子控件Tag中存取的XML属性值
			Dictionary<string, string> dic = new  Dictionary<string, string>();
			if(item.HasAttribute("ValidateGroup"))
				dic.Add("ValidateGroup", item.Attributes["ValidateGroup"].Value);
			if (item.HasAttribute("Validate"))
				dic.Add("Validate", item.Attributes["Validate"].Value);
			//定义在Dictionary中根据指定的键搜索的值存取到的变量
			string value = string.Empty;

			//判断Tag中是否存在文本验证规则的属性
			if (dic.TryGetValue(
				SupportEnumType.EnumControlTag.Validate.ToString(),
				out value))
			{
				//判断文本是否匹配验证规则
				if (!Regex.IsMatch(strText, value))
				{
					//取得Tag中存在的验证失败的提示信息
					dic.TryGetValue(
						SupportEnumType.EnumControlTag.ErrorMessage.ToString(),
						out value);

					if (value == null)
					{
						this.ShowMsg(SysLog.ReplaceParam(
							SysLog.GetMessage(7140),
							 new[] { item.Attributes["Caption"].Value, strText }),
							 true);
						SysLog.Write(7140, this._proIdAndThreadId, new[] { item.Attributes["Caption"].Value, strText });
					}
					else
					{
						this.ShowMsg(value, true);
						SysLog.Write(7400, this._proIdAndThreadId, value);
					}


                    backgroundcolor = $"background-color:red;";
					if (!item.HasAttribute("Color"))
					{
						item.SetAttribute("Color", backgroundcolor);
					}
					else
					{
						item.Attributes["Color"].Value = backgroundcolor;
					}

					return false;
				}
				//todo
				backgroundcolor = $"background-color:#fff;";
				if (!item.HasAttribute("Color"))
				{
					item.SetAttribute("Color", backgroundcolor);
				}
				else
				{
					item.Attributes["Color"].Value = backgroundcolor;
				}
				
				
                this.ShowMsg(SysLog.GetMessage(9103), false);
				SysLog.Write(9103, this._proIdAndThreadId);
			}

			//检测是否自动保存
			if (this.CheckAutoSave(item.ParentNode))
			{
				if (!await this.ValidateGroupXml(item, dic)) return false;
				this.Click_OK(item.ParentNode);
			}

			return true;
		}



		/// <summary>
		/// 点击按钮执行外部验证
		/// </summary>
		private async Task<bool> ValidateGroupXmlByClick(XmlNode _xmlnode)
		{
			string value = string.Empty;
			List<string> groupName = new List<string>();
			Dictionary<string, string> dic = null;

			foreach (XmlElement item in _xmlnode)
			{
				dic = new  Dictionary<string, string>();
				if (item.HasAttribute("ValidateGroup"))
					dic.Add("ValidateGroup", item.Attributes["ValidateGroup"].Value);
				if (item.HasAttribute("Validate"))
					dic.Add("Validate", item.Attributes["Validate"].Value);
				

				if (!dic.TryGetValue(SupportEnumType.EnumControlTag.ValidateGroup.ToString(),
				  out value) ||
				  value == string.Empty ||
				  groupName.Contains(value) ||
				  groupName.Any(a => value.Split(',').Any(b => b == a)))
					continue;
				groupName.Add(value);

				if (!await this.ValidateGroupXml(item, dic))
				{
					return false;
				}
			}

			return true;
		}


		/// <summary>
		/// 外部系统验证组XML
		/// </summary>
		/// <param name="control">控件对象</param>
		/// <param name="dic">面板子控件Tag中存取的XML属性值</param>
		private async Task<bool> ValidateGroupXml(XmlElement item, Dictionary<string, string> dic)
		{
			//控件组外部系统验证
			XmlElement[] groupControl = null;
			string validataXml = this.CheckGroupValidate(item, dic, ref groupControl);
			if (validataXml != string.Empty)
			{
				if (! await this.CheckOutXmlValidate(item, validataXml))
				{
					
					this.SetControlGroupColor(groupControl, false);
					return false;
				}
				this.SetControlGroupColor(groupControl, true);
			}
			else
			{            
                backgroundcolor = $"background-color:#fff;";
				if (!item.HasAttribute("Color"))
				{
					item.SetAttribute("Color", backgroundcolor);
				}
				else
				{
					item.Attributes["Color"].Value = backgroundcolor;
				}
			}
			return true;
		}

		/// <summary>
		/// 检测组验证
		/// </summary>
		/// <param name="control">控件对象</param>
		/// <param name="dic">面板子控件Tag中存取的XML属性值</param>
		/// <param name="groupControl">与当前相关的控件组</param>
		private string CheckGroupValidate(XmlElement control, Dictionary<string, string> dic, ref XmlElement[] groupControl)
		{
			//定义待发送的进行验证的控件与值组成的XML字符串
			string validataXml = "";

			//定义在Dictionary中根据指定的键搜索的值存取到的变量
			string value = string.Empty;

			if (!dic.TryGetValue(
				SupportEnumType.EnumControlTag.ValidateGroup.ToString(),
				out value))
			{
				validataXml = "<" + control.Attributes["Name"].Value + ">" +
					control.Attributes["Text"].Value.Trim() +
					"</" + control.Attributes["Name"].Value + ">";
			}
			else
			{
				//验证组中其他的输入
				if (!this.ValidataGroup(control, value, out groupControl))
					return string.Empty;

				//全部都验证成功 则保存输入信息
				groupControl.ToList().ForEach(a =>
				{
					validataXml += "<" + a.Attributes["Name"].Value + ">" +
						a.Attributes["Text"].Value.Trim() +
						"</" + a.Attributes["Name"].Value + ">";
				});
			}

			try
			{
				//使用uniq_metadata节点值作为外系统唯一识别号
				string uniqNodeName = (this.tableType == EnumType.TableType.BusinessTable) ?
					"biz_metadata1" :
					"uniq_metadata";

				//DataRow[] nodeRows = this.DataManage.SelectNodeRows(this._parentId,
				//    uniqNodeName,
				//    SupportEnumType.EnumNodeType.Control);

				validataXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
					"<CheckData>" +
					"<" + uniqNodeName + ">" +
				   data[uniqNodeName] +
					"</" + uniqNodeName + ">" +
					validataXml +
					"</CheckData>";
			}
			catch (Exception ex)
			{
				this.ShowMsg(SysLog.GetMessage(7111) + ex.Message, true);
				SysLog.Write(7111, ex, this._proIdAndThreadId);
				validataXml = string.Empty;
			}

			return validataXml;
		}

		/// <summary>
		/// 检测外部系统XML验证
		/// </summary>
		/// <param name="control">控件对象</param>
		/// <param name="validataXml">待发送的进行验证的控件与值组成的XML字符串</param>
		private async Task<bool> CheckOutXmlValidate(XmlElement control, string validataXml)
		{
			try
			{
				if (this.XmlValidated != null)
				{
					this.ShowMsg(SysLog.GetMessage(9101), false);
					SysLog.Write(9101, this._proIdAndThreadId);

					if (! await this.XmlValidated(validataXml))
					{
						//不能操作按钮进行保存
						this.ShowMsg(SysLog.GetMessage(7141), true);
						SysLog.Write(7141, this._proIdAndThreadId);

						//todo
						//control.Focus();
						//AllowButton(false);

						return false;
					}
					//可以操作按钮进行保存
					this.ShowMsg(SysLog.GetMessage(9102), false);
					SysLog.Write(9102, this._proIdAndThreadId);

					//AllowButton(true);
					//todo
					//this.CurrentControl = null;
				}
				else
				{
					//AllowButton(true);
					//todo
					//this.CurrentControl = null;
				}
			}
			catch (Exception ex)
			{
				this.ShowMsg(SysLog.GetMessage(7111) + ex.Message, true);
				SysLog.Write(7110, ex, this._proIdAndThreadId);
			}

			return true;
		}

		/// <summary>
		///  验证组中其他的控件输入
		/// </summary>
		/// <param name="control">控件对象</param>
		/// <param name="groupName">组名</param>
		/// <param name="groupControl">验证控件组</param>
		private bool ValidataGroup(XmlElement control, string groupName, out XmlElement[] groupControl)
		{
			//定义在Dictionary中根据指定的键搜索的值存取到的变量
			string value = string.Empty;
			Dictionary<string, string> dic = null;
			bool isGroup = false;

			//取得同组的控件列表
			List<XmlElement> listGroupControl = this._controlArray.Where(a =>
			{
				dic = new Dictionary<string, string>();
				if (a.HasAttribute("ValidateGroup"))
					dic.Add("ValidateGroup", a.Attributes["ValidateGroup"].Value);
				if (a.HasAttribute("Validate"))
					dic.Add("Validate", a.Attributes["Validate"].Value);
			
				isGroup = dic.TryGetValue(
				   SupportEnumType.EnumControlTag.ValidateGroup.ToString(),
				   out value);

				if (isGroup &&
					(value == groupName ||
					value.Split(',').Any(b => b == groupName)))
					return true;

				return false;
			}).ToList();
			groupControl = listGroupControl.ToArray();

			//判断组中其他的控件是否已输入值，任意一个没有都代表验证组失败
			return !listGroupControl.Any(a => a.Attributes["Text"].Value.Trim() == string.Empty);
		}


		/// <summary>
		/// 显示错误提示消息
		/// </summary>
		/// <param name="value">消息</param>
		/// <param name="isError">是否错误消息</param>
		private void ShowMsg(string value, bool isError)
		{
			//判断是否执行外部委托的方法
			if (this.TextValidated != null)
			{
				//调用外部绑定事件
				this.TextValidated(value);
			}
			else
			{
				//直接提示信息
				if (isError)
				{					
					ShowMsgBox?.Invoke(value,
						SysLog.GetMessage(7400),
						ConfirmButtons.OK,
						ConfirmIcon.Error);
					SysLog.Write(7400, this._proIdAndThreadId,
						value);
				}
				else
				{

					ShowMsgBox?.Invoke(value,
						SysLog.GetMessage(9900),
						ConfirmButtons.OK,
						ConfirmIcon.Info);
					SysLog.Write(9900, this._proIdAndThreadId,
						value);
				}
			}
		}


        /// <summary>
        /// 设置控件组的颜色
        /// </summary>
        /// <param name="groupControl">控件组</param>
        /// <param name="isSuccess">是否验证成功</param>
        private void SetControlGroupColor(XmlElement[] groupControl, bool isSuccess)
        {
            if (groupControl == null) return;
            foreach (XmlElement itemControl in groupControl)
            {
                if (isSuccess)
				{
					backgroundcolor = $"background-color:#fff;";
				}
				else
				{
					backgroundcolor = $"background-color:red;";
				}
				if (!itemControl.HasAttribute("Color"))
				{
					itemControl.SetAttribute("Color", backgroundcolor);
				}
				else
				{
					itemControl.Attributes["Color"].Value = backgroundcolor;
				}

			}
        }

    }
}
