using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Com.Boc.Icms.MetadataEdit.DataTables
{
    sealed class VersionTable : BaseDateTable<Com.Boc.Icms.MetadataEdit.Models.Version>
    {
        public VersionTable()
            : base()
        {
            this.TableName = Com.Boc.Icms.MetadataEdit.Business.BusinessData.EnumType.TableType.VersionTable.ToString();
        }
    }
}
