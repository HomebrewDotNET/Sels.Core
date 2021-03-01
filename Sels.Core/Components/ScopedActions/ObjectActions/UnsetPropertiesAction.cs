using Sels.Core.Extensions;
using Sels.Core.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Sels.Core.Components.ScopedActions.ObjectActions
{
    public class UnsetPropertiesAction<TObject> : ObjectAction<PropertyBundle<TObject>> where TObject : class
    {
        public UnsetPropertiesAction(TObject value, params Expression<Func<TObject, object>>[] expressions) : base(new PropertyBundle<TObject>(value, expressions), UnsetProperties, ResetProperties)
        {

        }


        private static void UnsetProperties(PropertyBundle<TObject> bundle)
        {
            bundle.ValidateVariable(nameof(bundle));

            foreach(var expression in bundle.Expressions)
            {
                // Extract value from property, store it and set default value on properties
                if(expression.TryExtractProperty(out var property))
                {
                    var propertyValue = property.GetValue(bundle.Source);

                    bundle.PropertyValues.AddValue(property, propertyValue);

                    property.SetDefault(bundle.Source);
                }
            }
        }

        private static void ResetProperties(PropertyBundle<TObject> bundle)
        {
            if (bundle.PropertyValues.HasValue())
            {
                // Restore previous property values
                foreach(var propertyValue in bundle.PropertyValues)
                {
                    propertyValue.Key.SetValue(bundle.Source, propertyValue.Value);
                }
            }
        }
    }

    public class PropertyBundle<TObject> where TObject : class
    {
        // Properties
        public TObject Source { get; }
        public Expression<Func<TObject, object>>[] Expressions { get; }
        public Dictionary<PropertyInfo, object> PropertyValues { get; }

        public PropertyBundle(TObject source, params Expression<Func<TObject, object>>[] expressions)
        {
            source.ValidateVariable(nameof(source));
            expressions.ValidateVariable(nameof(expressions));

            Source = source;
            Expressions = expressions;
            PropertyValues = new Dictionary<PropertyInfo, object>();
        }
    }
}
