using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace System
{
    /// <summary>
    /// Contains extension methods for working with 2 dimensional arrays.
    /// </summary>
    public static class GridExtensions
    {
        /// <summary>
        /// Gets an array from all the elements in row <paramref name="rowIndex"/> in grid <paramref name="grid"/>.
        /// </summary>
        /// <param name="grid">The grid to get the row from</param>
        /// <param name="rowIndex">The index of the row to get</param>
        /// <returns>The columns of row at index <paramref name="rowIndex"/></returns>
        public static string[] GetRow(this string[,] grid, int rowIndex = 0)
        {
            grid.ValidateArgument(nameof(grid));
            rowIndex.ValidateArgumentLargerOrEqual(nameof(rowIndex), 0);

            var columnLength = grid.GetLength(1);

            var columns = new string[columnLength];

            for(int i = 0; i < columnLength; i++)
            {
                columns[i] = grid[rowIndex, i];
            }

            return columns;
        }
    }
}
