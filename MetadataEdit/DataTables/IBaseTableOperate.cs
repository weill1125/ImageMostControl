using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Com.Boc.Icms.MetadataEdit.DataTables
{
    interface IBaseTableOperate<T>
    {
        DataRow AddRow(T t);
        void Delete(string filterExpression);
        T[] Find(params string[] parameters);
        void UpdateCellValue(string filterExpression, string column, object value);
    }
}
