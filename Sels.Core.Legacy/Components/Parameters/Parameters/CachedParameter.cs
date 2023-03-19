using Sels.Core.Extensions;
using System.Collections.Generic;

namespace Sels.Core.Components.Parameters.Parameters
{
    /// <summary>
    /// Caches generated values by using argument within the same scope
    /// </summary>
    public abstract class CachedParameter : Parameter<Dictionary<string , string>>
    {
        public CachedParameter(string name) : base(name)
        {

        }

        public override Dictionary<string, string> BeginNewScope()
        {
            return new Dictionary<string, string>();
        }

        protected override string GenerateValue(Dictionary<string, string> scope, string argument)
        {
            return scope.TryGetOrSet(argument, () => GenerateValue(scope));
        }

        protected override string GenerateValue(Dictionary<string, string> scope)
        {
            return GenerateValue();
        }

        // Abstraction
        /// <summary>
        /// Generates a new value without scope or argument
        /// </summary>
        /// <returns></returns>
        protected abstract string GenerateValue();
    }
}
