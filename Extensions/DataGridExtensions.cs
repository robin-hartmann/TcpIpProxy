using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Extensions
{
    public static class DataGridExtensions
    {
        public static DataGridCell ExtractDataGridCell(this DataGrid grid, DataGridCellInfo cellInfo)
        {
            var cellContent = cellInfo.Column.GetCellContent(cellInfo.Item);

            if (cellContent != null)
            {
                return cellContent.Parent as DataGridCell;
            }

            return null;
        }
    }
}
