using Sels.Core.Components.Filtering.ObjectFilter;
using System;
using System.Collections.Generic;
using System.Text;
using Sels.Core.Extensions;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Threading.Tasks;
using Sels.Core.Extensions.Reflection;

namespace Sels.Core.Wpf.Templates.Crud
{
    public class SearchableCrudList<TFilterObject, TFilter, TObject> : CrudList<TObject> where TFilter : ObjectFilter<TFilterObject, TObject> where TFilterObject : new()
    {
        // Fields
        private readonly TFilter _filter;

        // Properties
        public bool CanSearch {
            get
            {
                return GetValue<bool>(nameof(CanSearch));
            }
            set
            {
                SetValue(nameof(CanSearch), value);
            }
        }
        public TFilterObject FilterObject {
            get
            {
                return GetValue<TFilterObject>(nameof(FilterObject));
            }
            set
            {
                SetValue(nameof(FilterObject), value);
            }
        }
        public ObservableCollection<TObject> FilteredObjects {
            get
            {
                return GetValue<ObservableCollection<TObject>>(nameof(FilteredObjects));
            }
            set
            {
                SetValue(nameof(FilteredObjects), value);
            }
        }

        // Commands
        public ICommand SearchCommand { get; }
        public ICommand ClearSearchCommand { get; }
        public ICommand ClearFilterPropertyCommand { get; }

        public SearchableCrudList(TFilter filter)
        {
            filter.ValidateVariable(nameof(filter));

            _filter = filter;
            CanSearch = true;

            SearchCommand = CreateCommand(Search);
            ClearSearchCommand = CreateCommand(ClearSearch);
            ClearFilterPropertyCommand = CreateCommand<string>(ClearFilterProperty);
        }

        // Virtuals
        protected virtual void Search()
        {
            var filteredObjects = _filter.Filter(FilterObject, Objects);
            FilteredObjects = new ObservableCollection<TObject>(filteredObjects);
        }
        protected virtual void ClearSearch()
        {
            FilteredObjects = Objects; 
            FilterObject = new TFilterObject();
        }
        protected virtual void ClearFilterProperty(string propertyName)
        {
            if(FilterObject.TryFindProperty(propertyName, out var property))
            {
                property.SetDefault(FilterObject);
            }
        }

        // Overrides
        protected async override Task InitializeControl()
        {
            await base.InitializeControl();

            ClearSearch();
        }
    }
}
