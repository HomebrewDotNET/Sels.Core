using Sels.Core.Attributes;
using Sels.Core.Entity;
using Sels.Core.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Sels.Core.Helper.Expressions;
using LinqExpression = System.Linq.Expressions.Expression;

namespace Sels.Core
{
    /// <summary>
    /// Static class that contains helper methods for various tasks.
    /// </summary>
    public static partial class CoreHelper
    {
        /// <summary>
        /// Contains static helper methods for working with expressions.
        /// </summary>
        public static class Expressions
        {
            /// <summary>
            /// Generates a function that combines the hash codes of all public instance properties of type <typeparamref name="T"/> into a single hash code. Properties can be skipped by providing their names in <paramref name="skipPropertyNames"/>.
            /// </summary>
            /// <typeparam name="T">The type of the object to generate the hash code for</typeparam>
            /// <param name="includeEquivalent">If set to true, the delegate will also check if properties that implement <see cref="IDeduplicatable"/> are equivalent by calling <see cref="IDeduplicatable.IsEquivalentTo(IDeduplicatable)"/>.</param>
            /// <param name="skipPropertyNames">The names of the properties to skip when generating the hash code</param>
            /// <returns>A function that generates a hash code for an object of type <typeparamref name="T"/></returns>
            public static Func<T, int> GenerateHashCodeCombiner<T>(bool includeEquivalent = false, params string[] skipPropertyNames)
            {
                skipPropertyNames = Guard.IsNotNull(skipPropertyNames);
                var func = GenerateHashCodeCombiner(typeof(T), includeEquivalent, skipPropertyNames);
                return (T input) => func(input);
            }
            /// <summary>
            /// Generates a function that combines the hash codes of all public instance properties of <paramref name="type"/> into a single hash code. Properties can be skipped by providing their names in <paramref name="skipPropertyNames"/>.
            /// </summary>
            /// <param name="type">The type of the object to generate the hash code for</param>
            /// <param name="includeEquivalent">If set to true, the delegate will also check if properties that implement <see cref="IDeduplicatable"/> are equivalent by calling <see cref="IDeduplicatable.IsEquivalentTo(IDeduplicatable)"/>.</param>
            /// <param name="skipPropertyNames">The names of the properties to skip when generating the hash code</param>
            /// <returns>A function that generates a hash code for an object of type <paramref name="type"/></returns>
            public static Func<object, int> GenerateHashCodeCombiner(Type type, bool includeEquivalent = false, params string[] skipPropertyNames)
            {
                type = Guard.IsNotNull(type);
                skipPropertyNames = Guard.IsNotNull(skipPropertyNames);
                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => x.CanRead && !skipPropertyNames.Contains(x.Name) && x.GetAttributeOrDefault<IgnoredAttribute>() == null).ToArray();
                var input = LinqExpression.Parameter(typeof(object), CoreConstants.Expressions.InputParameterName);
                var castInput = LinqExpression.Convert(input, type);
                var variable = LinqExpression.Variable(type, CoreConstants.Expressions.CastInputVariableName);
                var assign = LinqExpression.Assign(variable, castInput);
                var hashCodeExpressions = new List<LinqExpression>();
                var createHashCode = LinqExpression.New(typeof(HashCode));
                var hashCodeVariable = LinqExpression.Variable(typeof(HashCode), CoreConstants.Expressions.HashCodeVariableName);
                var hashCodeAssign = LinqExpression.Assign(hashCodeVariable, createHashCode);
                var addHashCodeGenericMethod = Helper.Expressions.Method.GetMethod<HashCode>(x => x.Add(default(int))).GetGenericMethodDefinition();
                var toHashCodeMethod = Helper.Expressions.Method.GetMethod<HashCode>(x => x.ToHashCode());

                foreach (var property in properties)
                {
                    var propertyAccess = LinqExpression.Property(variable, property); 
                    var propertyIsDefault = LinqExpression.Equal(propertyAccess, LinqExpression.Default(property.PropertyType));

                    var addHashCodeMethod = addHashCodeGenericMethod.MakeGenericMethod(property.PropertyType);
                    LinqExpression addHashCodeCall = LinqExpression.Call(hashCodeVariable, addHashCodeMethod, propertyAccess);
                    if (includeEquivalent && property.PropertyType.IsAssignableTo(typeof(IDeduplicatable)))
                    {
                        var getBucketMethod = Method.GetMethod<IDeduplicatable>(x => x.GetBucket());
                        var getBucketCall = LinqExpression.Call(propertyAccess, getBucketMethod);
                        var addBucketHashCodeMethod = addHashCodeGenericMethod.MakeGenericMethod(typeof(int));
                        var addBucketHashCodeCall = LinqExpression.Call(hashCodeVariable, addBucketHashCodeMethod, getBucketCall);

                        var ifNotDefault = LinqExpression.IfThenElse(propertyIsDefault, addHashCodeCall, addBucketHashCodeCall);
                        addHashCodeCall = ifNotDefault;
                    }

                    hashCodeExpressions.Add(addHashCodeCall);
                }

                var toHashCodeCall = LinqExpression.Call(hashCodeVariable, toHashCodeMethod);
                return LinqExpression.Lambda<Func<object, int>>(LinqExpression.Block(new[] { hashCodeVariable, variable }, new[] { hashCodeAssign, assign }.Concat(hashCodeExpressions).Append(toHashCodeCall)), input).Compile();
            }
        }
    }
}
