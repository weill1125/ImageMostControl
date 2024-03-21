using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Com.Boc.Icms.MetadataEdit.Base.Component
{
    /// <summary>
    /// 控件抽象类
    /// 如果要添加新的控件，请继承此方法，并将控件对象添加到ControlCollection
    /// </summary>
    public abstract class BaseTemplateControl : IEnumerable
    {
        protected List<Control> ControlCollection = new List<Control>();

        public virtual Button Button 
        { 
            get 
            { 
                return new Button(); 
            } 
        }
        
        public virtual Label Label 
        { 
            get 
            { 
                return new Label(); 
            } 
        }

        public virtual TextBox TextBox 
        { 
            get 
            { 
                return new TextBox(); 
            } 
        }

        public virtual CheckBox CheckBox 
        { 
            get 
            { 
                return new CheckBox(); 
            } 
        }

        public virtual PictureBox PictrueBox 
        { 
            get 
            { 
                return new PictureBox(); 
            } 
        }

        public virtual DataGridView DataGridView 
        { 
            get 
            { 
                return new DataGridView(); 
            } 
        }

        public virtual RichTextBox RichTextBox 
        { 
            get 
            { 
                return new RichTextBox(); 
            } 
        }

        public virtual CheckedListBox CheckedListBox 
        { 
            get 
            { 
                return new CheckedListBox(); 
            } 
        }

        public virtual ComboBox ComboBox 
        { 
            get 
            { 
                return new ComboBox(); 
            } 
        }

        public virtual RadioButton RadioButton 
        { 
            get 
            { 
                return new RadioButton(); 
            } 
        }

        /// <summary>
        /// 搜索指定类型名的控件对象
        /// </summary>
        /// <param name="type">类型名</param>
        /// <returns></returns>
        public virtual Control Find(string type)
        {
            type = type.ToUpperInvariant();
            Control control = this.GetFindControl(this.ControlCollection, type);

            //如果在当前集合中没有找到控件对象，则到默认的控件库中查找
            if (control == null)
            {
                List<Control> listControl = new List<Control>(){ this.Button, this.Label, this.TextBox, this.CheckBox, this.PictrueBox, this.DataGridView, this.RichTextBox, this.DataGridView, this.RichTextBox, this.CheckedListBox, this.ComboBox, this.RadioButton};

                control = this.GetFindControl(listControl, type);
            }

            return control;
        }

        /// <summary>
        /// 取得在集合中找到的控件对象
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private Control GetFindControl(List<Control> listControl, string type)
        {
            Control control = null;
            this.ControlCollection.ForEach(a =>
              {
                  if (a.GetType().Name.ToUpperInvariant() == type)
                  {
                      //控件深复制
                      control = (Control)System.Activator.CreateInstance(a.GetType());
                  }
              });

            return control;
        }

        public IEnumerator GetEnumerator()
        {
            return this.ControlCollection.GetEnumerator();
        }
    }
}
