using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Web.Blazor.Pages.Table
{
    /// <summary>
    /// Contains options for a model table of what features to enable/disable.
    /// </summary>
    [Flags]
    public enum ModelTableOptions
    {
        /// <summary>
        /// Disables nothing.
        /// </summary>
        None = 0,
        /// <summary>
        /// Disables the checkbox column for selecting rows.
        /// </summary>
        DisableSelection = 1,
        /// <summary>
        /// Disables the pagination components.
        /// </summary>
        DisablePagination = 2,
        /// <summary>
        /// Disables the option to order the columns.
        /// </summary>
        DisableSorting = 4 
    }
}
