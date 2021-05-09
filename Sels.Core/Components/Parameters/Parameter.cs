using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Sels.Core.Components.Parameters
{
    public abstract class Parameter
    {
        // Fields
        private List<string> _dependencies = new List<string>();
        private Func<string, string, string> _parameterValueResolver;

        // Properties
        public string Name { get; }
        public ReadOnlyCollection<string> Dependencies => new ReadOnlyCollection<string>(_dependencies);

        public Parameter(string name)
        {
            Name = name.ValidateVariable(nameof(name));
        }

        #region GenerateNewValue
        /// <summary>
        /// Generates a new value using the supplied argument without starting a scope
        /// </summary>
        public string GenerateNewValue(string argument = null)
        {
            var scopeContext = BeginScope();
            return GenerateNewValue(scopeContext, argument);
        }

        /// <summary>
        /// Generates a new value using the supplied argument and scope
        /// </summary>
        public string GenerateNewValue(object scopeContext, string argument = null)
        {
            return argument.IsNullOrEmpty() ? GenerateValue(scopeContext) : GenerateValue(scopeContext, argument);
        }
        #endregion

        #region AddDependency
        /// <summary>
        /// Adds a dependency to another parameter by name.
        /// </summary>
        public Parameter AddDependency(string name)
        {
            name.ValidateVariable(nameof(name));
            name.ValidateVariable(x => !_dependencies.Contains(name), () => $"Dependency to {name} already added");

            _dependencies.Add(name);

            return this;
        }

        /// <summary>
        /// Adds a dependency to another parameter.
        /// </summary>
        public Parameter AddDependency(Parameter parameter)
        {
            parameter.ValidateVariable(nameof(parameter));
            AddDependency(parameter.Name);

            return this;
        }
        #endregion

        internal Parameter SetParameterResolver(Func<string, string, string> parameterValueResolver)
        {
            parameterValueResolver.ValidateVariable(nameof(parameterValueResolver));

            _parameterValueResolver = parameterValueResolver;

            return this;
        }

        /// <summary>
        /// Gets the value of another parameter
        /// </summary>
        protected string GetParameterValue(string name, string argument = null)
        {
            name.ValidateVariable(nameof(name));
            _parameterValueResolver.ValidateVariable(x => x.HasValue(), () => new NotSupportedException($"Parameter value resolver is not set"));

            return _parameterValueResolver(name, argument);
        }

        // Abstractions
        /// <summary>
        /// Creates a new context that can be used to create a new scope
        /// </summary>
        public abstract object BeginScope();
        /// <summary>
        /// Generates new value without argument
        /// </summary>
        protected abstract string GenerateValue(object scope);
        /// <summary>
        /// Generate bew value with argument
        /// </summary>
        protected abstract string GenerateValue(object scope, string argument);
    }

    public abstract class Parameter<TContext> : Parameter
    {
        public Parameter(string name) : base(name)
        {

        }

        public override object BeginScope()
        {
            return BeginNewScope();
        }

        protected override string GenerateValue(object scope)
        {
            scope.ValidateVariable(nameof(scope));
            scope.ValidateIfType<TContext>(nameof(scope));
            var typedScope = (TContext) scope;

            return GenerateValue(typedScope);
        }

        protected override string GenerateValue(object scope, string argument)
        {
            scope.ValidateVariable(nameof(scope));
            scope.ValidateIfType<TContext>(nameof(scope));
            var typedScope = (TContext)scope;

            return GenerateValue(typedScope, argument);
        }


        /// <summary>
        /// Creates a new context that can be used to create a new scope
        /// </summary>
        public abstract TContext BeginNewScope();
        /// <summary>
        /// Generates new value without argument
        /// </summary>
        protected abstract string GenerateValue(TContext scope);
        /// <summary>
        /// Generate bew value with argument
        /// </summary>
        protected abstract string GenerateValue(TContext scope, string argument);
    }
}
