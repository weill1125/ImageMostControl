using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Com.Boc.Icms.MetadataEdit.DataTables
{
    sealed class DocTable : BaseDateTable<Com.Boc.Icms.MetadataEdit.Models.Doc>
    {
        public DocTable()
            : base()
        {
            this.TableName = Com.Boc.Icms.MetadataEdit.Business.BusinessData.EnumType.TableType.DocTable.ToString();

            this.PrimaryKey = new[] { this.Columns[0] };
        }
    }
}
