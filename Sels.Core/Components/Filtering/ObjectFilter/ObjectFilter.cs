using Sels.Core.Extensions;
using Sels.Core.Extensions;
using Sels.Core.Extensions;
using Sels.Core.Extensions;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Reflection;
using Sels.Core.Extensions.Reflection;
using Sels.Core.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Sels.Core.Components.Filtering.ObjectFilter
{
    public class ObjectFilter<TFilterObject, TObject>
    {
        // Fields
        private readonly Dictionary<PropertyInfo, PropertyInfo> _propertyMappings = new Dictionary<PropertyInfo, PropertyInfo>();
        private readonly Dictionary<Type, Delegate> _filters = new Dictionary<Type, Delegate>();
        private readonly Dictionary<(PropertyInfo FilterProperty, PropertyInfo ObjectProperty), Delegate> _explicitFilters = new Dictionary<(PropertyInfo, PropertyInfo), Delegate>();

        public ObjectFilter()
        {
            CreateImplicitMaps();
            CreateDefaultFilters();
        }

        #region Setup
        /// <summary>
        /// Used to expliticly map 2 properties together. By default properties with the same name are implicitly mapped
        /// </summary>
        /// <param name="filterPropertyExpression">Property on filter object</param>
        /// <param name="objectPropertyExpression">Property on object</param>
        public void CreateExplicitMap(Expression<Action<TFilterObject>> filterPropertyExpression, Expression<Action<TObject>> objectPropertyExpression)
        {
            filterPropertyExpression.ValidateVariable(nameof(filterPropertyExpression));
            objectPropertyExpression.ValidateVariable(nameof(objectPropertyExpression));

            var filterProperty = filterPropertyExpression.ExtractProperty(nameof(filterPropertyExpression));
            var objectProperty = objectPropertyExpression.ExtractProperty(nameof(objectPropertyExpression));

            CreateExplicitMap(filterProperty, objectProperty);
        }

        /// <summary>
        /// Used when filtering properties of type T. If no explicit filters exist the ObjectFilters uses Equals
        /// </summary>
        /// <typeparam name="T">Property type</typeparam>
        /// <param name="filter">Filter delegate</param>
        public void CreateExplicitTypeFilter<T>(Func<T, T, bool> filter)
        {
            filter.ValidateVariable(nameof(filter));

            var propertyType = typeof(T);

            _filters.AddValue(propertyType, filter);
        }

        /// <summary>
        /// Create a explicit mapping for 2 properties if there property types aren't assignable from each other
        /// </summary>
        /// <typeparam name="TFilterValue">Filter property type</typeparam>
        /// <typeparam name="TObjectValue">Object property type</typeparam>
        /// <param name="filterPropertyExpression">Property on filter object</param>
        /// <param name="filter">Filter delegate</param>
        public void CreateExplicitFilter<TFilterValue, TObjectValue>(Expression<Func<TFilterObject, TFilterValue>> filterPropertyExpression, Expression<Func<TObject, TObjectValue>> objectPropertyExpression, Func<TFilterValue, TObjectValue, bool> filter)
        {
            filterPropertyExpression.ValidateVariable(nameof(filterPropertyExpression));
            filter.ValidateVariable(nameof(filter));

            var filterProperty = filterPropertyExpression.ExtractProperty(nameof(filterPropertyExpression));
            var objectProperty = objectPropertyExpression.ExtractProperty(nameof(objectPropertyExpression));

            _explicitFilters.AddValue((filterProperty, objectProperty), filter);
        }

        private void CreateExplicitMap(PropertyInfo filterProperty, PropertyInfo objectProperty)
        {
            if (!filterProperty.PropertyType.IsAssignableFrom(objectProperty.PropertyType))
            {
                throw new NotSupportedException($"Filter type must be assignable from object type");
            }

            _propertyMappings.AddValue(filterProperty, objectProperty);
        }
        #endregion

        #region Filter
        public IEnumerable<TObject> Filter(TFilterObject filterObject, IEnumerable<TObject> objects)
        {
            if (filterObject.HasValue())
            {
                var filteredObjects = new List<TObject>();
                // Give all mappings not present in explicit mappings
                var propertyMappings = _propertyMappings.Where(x => !_explicitFilters.Any(y => x.Key == y.Key.FilterProperty));

                foreach(var toBeFilteredObject in objects)
                {
                    var isFiltered = false;

                    foreach(var filter in _explicitFilters)
                    {
                        try
                        {
                            var filterValue = filter.Key.FilterProperty.GetValue(filterObject);
                            var objectValue = filter.Key.ObjectProperty.GetValue(toBeFilteredObject);

                            if (!filter.Value.Invoke<bool>(filterValue, objectValue))
                            {
                                isFiltered = true;
                                break;
                            }
                        }
                        catch(Exception ex) { }                      
                    }

                    if (!isFiltered)
                    {
                        foreach (var mapping in propertyMappings)
                        {
                            var filterValue = mapping.Key.GetValue(filterObject);
                            var objectValue = mapping.Value.GetValue(toBeFilteredObject);
                            var propertyType = mapping.Key.PropertyType;

                            try
                            {
                                var filters = _filters.Where(x => x.Key.IsAssignableFrom(propertyType));

                                if (filters.HasValue())
                                {
                                    foreach (var filter in filters)
                                    {
                                        if (!filter.Value.Invoke<bool>(filterValue, objectValue))
                                        {
                                            isFiltered = true;
                                        }
                                    }

                                }
                                else if (DefaultFilter(filterValue, objectValue))
                                {
                                    isFiltered = true;
                                    break;
                                }
                            }
                            catch (Exception ex) { }
                        }
                    }

                    if (!isFiltered)
                    {
                        filteredObjects.Add(toBeFilteredObject);
                    }
                }

                return filteredObjects;
            }
            else
            {
                return objects;
            }
        }
        #endregion

        private void CreateImplicitMaps()
        {
            var filterType = typeof(TFilterObject);
            var objectType = typeof(TObject);

            foreach(var property in filterType.GetProperties())
            {
                if(objectType.TryFindProperty(property.Name, out var foundProperty) && property.PropertyType.Equals(foundProperty.PropertyType))
                {
                    CreateExplicitMap(property, foundProperty);
                }
            }
        }

        #region Filters
        private void CreateDefaultFilters()
        {
            CreateExplicitTypeFilter<string>(StringFilter);
            CreateExplicitTypeFilter<DateTime>(DateTimeFilter);
            CreateExplicitTypeFilter<DateTime?>(NullableDateTimeFilter);
        }

        private bool StringFilter(string filterValue, string objectValue)
        {
            if (!filterValue.HasValue())
            {
                return true;
            }
            else if (objectValue.HasValue())
            {
                return objectValue.ToLower().Contains(filterValue.ToLower());
            }
            else
            {
                return DefaultFilter(filterValue, objectValue);
            }
        }

        private bool DateTimeFilter(DateTime filterValue, DateTime objectValue)
        {
            if (filterValue.IsDefault())
            {
                return true;
            }
            else
            {
                return filterValue.IsSameDay(objectValue);
            }
        }

        private bool NullableDateTimeFilter(DateTime? filterValue, DateTime? objectValue)
        {
            if (filterValue == null || objectValue == null)
            {
                return true;
            }
            else
            {
                return DateTimeFilter(filterValue.Value, objectValue.Value);
            }
        }

        private bool DefaultFilter(object filterValue, object objectValue)
        {
            if (filterValue.IsDefault() || objectValue.IsDefault())
            {
                return true;
            }
            else
            {
                return filterValue.Equals(objectValue);
            }
        }
        #endregion
    }
}
