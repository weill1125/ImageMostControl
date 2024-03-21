using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Com.Boc.Icms.MetadataEdit.DataTables
{
    public class BusinessTable : BaseDateTable<Com.Boc.Icms.MetadataEdit.Models.Business>
    {
        public BusinessTable()
            : base()
        {

            this.TableName = Com.Boc.Icms.MetadataEdit.Business.BusinessData.EnumType.TableType.BusinessTable.ToString();

            this.PrimaryKey = new[] { this.Columns[0] };
        }
    }
}
