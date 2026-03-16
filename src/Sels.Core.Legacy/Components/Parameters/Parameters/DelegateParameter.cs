using Sels.Core.Extensions;
using System;

namespace Sels.Core.Parameters.Parameters
{
    public class DelegateParameter : Parameter
    {
        // Fields
        private readonly Func<object> _beginScopeAction;
        private readonly Func<object, string, string> _generateValueAction;

        public DelegateParameter(string name, Func<object, string, string> generateValueAction, Func<object> beginScopeAction = null) : base(name)
        {
            _generateValueAction = generateValueAction.ValidateArgument(nameof(generateValueAction));
            _beginScopeAction = beginScopeAction;
        }

        public override object BeginScope()
        {
            return _beginScopeAction != null ? _beginScopeAction() : null;
        }

        protected override string GenerateValue(object scope)
        {
            return _generateValueAction(scope, string.Empty);
        }

        protected override string GenerateValue(object scope, string argument)
        {
            return _generateValueAction(scope, argument);
        }
    }
}
