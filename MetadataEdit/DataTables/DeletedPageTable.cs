using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Com.Boc.Icms.MetadataEdit.DataTables
{
    sealed class DeletedPageTable : BaseDateTable<Com.Boc.Icms.MetadataEdit.Models.DeletedPage>
    {
        public DeletedPageTable()
            : base()
        {

            this.TableName = Com.Boc.Icms.MetadataEdit.Business.BusinessData.EnumType.TableType.DeletedPageTable.ToString();



        }
    }
}
