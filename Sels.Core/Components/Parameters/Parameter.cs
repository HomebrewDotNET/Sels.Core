﻿using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Sels.Core.Components.Parameters
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
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
            Name = name.ValidateArgument(nameof(name));
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
            name.ValidateArgument(nameof(name));
            name.ValidateArgument(x => !_dependencies.Contains(name), $"Dependency to {name} already added");

            _dependencies.Add(name);

            return this;
        }

        /// <summary>
        /// Adds a dependency to another parameter.
        /// </summary>
        public Parameter AddDependency(Parameter parameter)
        {
            parameter.ValidateArgument(nameof(parameter));
            AddDependency(parameter.Name);

            return this;
        }
        #endregion

        internal Parameter SetParameterResolver(Func<string, string, string> parameterValueResolver)
        {
            parameterValueResolver.ValidateArgument(nameof(parameterValueResolver));

            _parameterValueResolver = parameterValueResolver;

            return this;
        }

        /// <summary>
        /// Gets the value of another parameter
        /// </summary>
        protected string GetParameterValue(string name, string argument = null)
        {
            name.ValidateArgument(nameof(name));
            _parameterValueResolver.ValidateArgument(x => x.HasValue(), x => new NotSupportedException($"Parameter value resolver is not set"));

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
            scope.ValidateArgument(nameof(scope));
            scope.ValidateArgumentAssignableTo(nameof(scope), typeof(TContext));
            var typedScope = (TContext) scope;

            return GenerateValue(typedScope);
        }

        protected override string GenerateValue(object scope, string argument)
        {
            scope.ValidateArgument(nameof(scope));
            scope.ValidateArgumentAssignableTo(nameof(scope), typeof(TContext));
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
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
