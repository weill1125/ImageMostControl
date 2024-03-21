using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Com.Boc.Icms.MetadataEdit.DataTables
{
    sealed class PageTable : BaseDateTable<Com.Boc.Icms.MetadataEdit.Models.Page>
    {
        public PageTable()
            : base()
        {
            this.TableName = Com.Boc.Icms.MetadataEdit.Business.BusinessData.EnumType.TableType.PageTable.ToString();

            this.PrimaryKey = new[] { this.Columns[0] };

        }
    }
}
