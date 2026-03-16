using Microsoft.AspNetCore.Components;
using Sels.Core.Models.Sorting;
using static Sels.Core.Delegates.Async;

namespace Sels.Core.Web.Blazor.Pages.Table
{
    /// <summary>
    /// Context object for a model table component.
    /// </summary>
    /// <typeparam name="TModel">Type of the objects to display</typeparam>
    public class LazyTableContext<TModel>
    {
        // Fields
        private List<(string ColumnId, RenderFragment Header, bool CanSort, string? CssClass, int Order)> _headers = new List<(string HeaderId, RenderFragment Header, bool CanSort, string? CssClass, int Order)>();
        private List<(RenderFragment<TModel> Column, string? CssClass, int Order)> _columns = new List<(RenderFragment<TModel> Column, string? CssClass, int Order)>();
        private List<(RenderFragment Row, string? CssClass, int Order)> _rowComponents = new List<(RenderFragment Row, string? CssClass, int Order)>();

        // Properties
        /// <summary>
        /// Array of all the headers for the table.
        /// </summary>
        public (string ColumnId, RenderFragment Header, bool CanSort, string? CssClass, int Order)[] Headers => _headers.ToArray();
        /// <summary>
        /// Array of all the render components that will be used to render a value from <typeparamref name="TModel"/> in a column.
        /// </summary>
        public (RenderFragment<TModel> Column, string? CssClass, int Order)[] Columns => _columns.ToArray();
        /// <summary>
        /// Array of all render components that will be used to render additional components in the row above the table.
        /// </summary>
        public (RenderFragment Row, string? CssClass, int Order)[] RowComponents => _rowComponents.ToArray();
        /// <summary>
        /// The current state of the table.
        /// </summary>
        public LazyTableState<TModel> State { get; }
        /// <summary>
        /// Event that gets raised when the table state changes. Does not trigger when selected models change.
        /// </summary>
        public AsyncAction<LazyTableState<TModel>> OnStateChanged { get; set; }
        /// <summary>
        /// Event that gets raised when the selected models change.
        /// </summary>
        public AsyncAction<LazyTableState<TModel>> OnSelectedChanged { get; set; }
        /// <summary>
        /// Event that gets raised when the razor page needs to be refreshed.
        /// </summary>
        public AsyncAction OnRenderRequested { get; set; }
        /// <inheritdoc cref="LazyTableContext{TModel}"/>
        public LazyTableContext()
        {
            State = new LazyTableState<TModel>(this);
        }

        /// <summary>
        /// Add a new header to the current table.
        /// </summary>
        /// <param name="columnId">The id of the column that will be provided as the sorted column</param>
        /// <param name="header">The value that will be displayed as the header</param>
        /// <param name="order">Used to determine the order. Lower means rendered first</param>
        /// <param name="canSort">If this column can be sorted by</param>
        /// <param name="cssClass">Optional css class for the th tag</param>
        public Task AddHeader(string columnId, RenderFragment header, int order, bool canSort = true, string? cssClass = null)
        {
            columnId.ValidateArgumentNotNullOrWhitespace(nameof(columnId));
            header.ValidateArgument(nameof(header));

            _headers.Add((columnId, header, canSort, cssClass, order));

            if(OnRenderRequested != null) return OnRenderRequested();
            return Task.CompletedTask;
        }
        /// <summary>
        /// Adds a component that will render a value from <typeparamref name="TModel"/> in a column.
        /// </summary>
        /// <param name="order">Used to determine the order. Lower means rendered first</param>
        /// <param name="component">The render component</param>
        /// <param name="cssClass">Optional css class for the td tag</param>
        public Task AddColumnComponent(RenderFragment<TModel> component, int order, string? cssClass = null)
        {
            component.ValidateArgument(nameof(component));

            _columns.Add((component, cssClass, order));

            if (OnRenderRequested != null) return OnRenderRequested();
            return Task.CompletedTask;
        }
        /// <summary>
        /// Adds a components that will be rendered in the row above the table.
        /// </summary>
        /// <param name="component">The component to render</param>
        /// <param name="order">Used to determine the order. Lower means rendered first</param>
        /// <param name="cssClass">Optional css class for the td tag</param>
        public Task AddRowComponent(RenderFragment component, int order, string? cssClass = null)
        {
            component.ValidateArgument(nameof(component));

            _rowComponents.Add((component, cssClass, order));

            if (OnRenderRequested != null) return OnRenderRequested();
            return Task.CompletedTask;
        }
        /// <summary>
        /// Removes a column component.
        /// </summary>
        /// <param name="component">The reference of the column component to remove</param>
        public Task RemoveColumnComponent(RenderFragment<TModel> component)
        {
            component.ValidateArgument(nameof(component));

            var columnComponent = _columns.FirstOrDefault(x => x.Column.Equals(component));
            if (columnComponent != default) _columns.Remove(columnComponent);

            if (OnRenderRequested != null) return OnRenderRequested();
            return Task.CompletedTask;
        }
        /// <summary>
        /// Removes a header component.
        /// </summary>
        /// <param name="component">The reference of the header component to remove</param>
        public Task RemoveHeaderComponent(RenderFragment component)
        {
            component.ValidateArgument(nameof(component));

            var headerComponent = _headers.FirstOrDefault(x => x.Header.Equals(component));
            if (headerComponent != default) _headers.Remove(headerComponent);

            if (OnRenderRequested != null) return OnRenderRequested();
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Contains the state of a model table.
    /// </summary>
    /// <typeparam name="TModel">Type of the objects to display</typeparam>
    public class LazyTableState<TModel>
    {
        // Fields
        private readonly LazyTableContext<TModel> _parent;
        private readonly List<TModel> _selected = new List<TModel>();

        internal LazyTableState(LazyTableContext<TModel> parent)
        {
            _parent = parent.ValidateArgument(nameof(parent));
        }

        /// <summary>
        /// All the models displayed in the table.
        /// </summary>
        public TModel[] Models { get; private set; } = Array.Empty<TModel>();
        /// <summary>
        /// The currently selected models
        /// </summary>
        public TModel[] Selected => _selected.ToArray();
        /// <summary>
        /// The current page.
        /// </summary>
        public int Page { get; private set; } = 1;
        /// <summary>
        /// How many pages there are.
        /// </summary>
        public int TotalPages { get; private set; } = 1;
        /// <summary>
        /// Max amount of items to display per page.
        /// </summary>
        public int PageSize { get; private set; } = 20;
        /// <summary>
        /// Contains the column id of the column that is selected as the sort column.
        /// </summary>
        public string? SortColumn { get; private set; }
        /// <summary>
        /// The sort order for <see cref="SortColumn"/> if one is selected.
        /// </summary>
        public SortOrder SortOrder { get; private set; }

        /// <summary>
        /// Sets the current models to display.
        /// </summary>
        /// <param name="models">Enumerator returning the models to display</param>
        public Task SetModels(IEnumerable<TModel>? models)
        {
            if(models != null) Models = models.Where(x => x != null).ToArray();

            return SetSelected(_selected.Where(x => Models.Contains(x)));
        }
        /// <summary>
        /// Sets <see cref="Page"/>.
        /// </summary>
        /// <param name="page">What page the table is currently on</param>
        public async Task SetPage(int page)
        {
            if (Page == page) return;
            Page = page.ValidateArgumentLargerOrEqual(nameof(page), 1);
            if(_parent.OnStateChanged != null) await _parent.OnStateChanged?.Invoke(this);
        }
        /// <summary>
        /// Sets <see cref="PageSize"/>.
        /// </summary>
        /// <param name="size">How many items to display per page</param>
        public async Task SetPageSize(int size)
        {
            if (PageSize == size) return;
            PageSize = size.ValidateArgumentLargerOrEqual(nameof(size), 1);
            if (_parent.OnStateChanged != null) await _parent.OnStateChanged?.Invoke(this);
        }
        /// <summary>
        /// Sets <see cref="TotalPages"/>.
        /// </summary>
        /// <param name="pageAmount">How many pages there are to display</param>
        public async Task SetTotalPages(int pageAmount)
        {
            TotalPages = pageAmount.ValidateArgumentLargerOrEqual(nameof(pageAmount), 0);
            if(Page > TotalPages) Page = TotalPages > 0 ? TotalPages : 1;
            if (_parent.OnRenderRequested != null) await _parent.OnRenderRequested?.Invoke();
        }
        /// <summary>
        /// Sets the currently selected models.
        /// </summary>
        /// <param name="models">Enumerator returning the models to set as selected</param>
        public async Task SetSelected(IEnumerable<TModel>? models)
        {
            _selected.Clear();
            if (models != null) {
                _selected.AddRange(models.Where(x => x != null));
            }
            if (_parent.OnSelectedChanged != null) await _parent.OnSelectedChanged?.Invoke(this);
        }
        /// <summary>
        /// Sets the current column to order by and in which direction.
        /// </summary>
        /// <param name="columnId">The id of the column to order by</param>
        /// <param name="sortOrder">In which order to sort the column</param>
        public async Task SetSortColumn(string columnId, SortOrder sortOrder = default)
        {
            SortColumn = columnId.ValidateArgumentNotNullOrWhitespace(nameof(columnId));
            SortOrder = sortOrder;
            if (_parent.OnStateChanged != null) await _parent.OnStateChanged?.Invoke(this);
        }
        /// <summary>
        /// Checks if <paramref name="model"/> is selected in the table.
        /// </summary>
        /// <param name="model">The model to check</param>
        /// <returns>True if <paramref name="model"/> is selected in the table, otherwise false</returns>
        public bool IsSelected(TModel model)
        {
            return _selected.Contains(model);
        }
        internal async Task AddSelected(TModel model)
        {
            _selected.Add(model);
            if (_parent.OnSelectedChanged != null) await _parent.OnSelectedChanged?.Invoke(this);
        }
        internal async Task RemoveSelected(TModel model)
        {
            _selected.Remove(model);
            if (_parent.OnSelectedChanged != null) await _parent.OnSelectedChanged?.Invoke(this);
        }
    }
}
