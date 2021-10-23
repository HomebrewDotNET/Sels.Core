using Sels.Core.Components.Filtering.ObjectFilter;
using Sels.Core.Wpf.Components.Property;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Wpf.Templates.Crud
{
    public class InstantSearchCrudList<TFilterObject, TFilter, TObject> : SearchableCrudList<TFilterObject, TFilter, TObject> where TFilter : ObjectFilter<TFilterObject, TObject> where TFilterObject : BasePropertyChangedNotifier, new()
    {
        public InstantSearchCrudList(TFilter filter) : base(filter)
        {

        }

        protected override void ClearSearch()
        {
            base.ClearSearch();

            // Subscribe to property changes on filter object and trigger search when they change
            FilterObject.SubscribeToAllPropertiesChanged((wasChanged, property, propertyValue) => { if (wasChanged) SearchCommand.Execute(null); });
        }
    }
}
