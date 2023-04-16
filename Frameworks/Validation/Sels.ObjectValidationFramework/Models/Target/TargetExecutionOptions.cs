using System;
using Sels.ObjectValidationFramework.Rules;

namespace Sels.ObjectValidationFramework.Target
{
    /// <summary>
    /// Modifies the behaviour how and when validation rules are executed.
    /// </summary>
    [Flags]
    public enum TargetExecutionOptions
    {
        /// <summary>
        /// No settings selected.
        /// </summary>
        None = 0,
        /// <summary>
        /// If a context of the required type is required for the validation rules created for the current target 
        /// If set to false and context is not of the required type then <see cref="IValidationRuleContext{TEntity, TContext}.HasContext"/> will be set to false.
        /// When set to true the rules will only be executed when the supplied context is of the required type.
        /// </summary>
        WithSuppliedContext = 1,
        /// <summary>
        /// When a rule returns an error the validation rules defined after won't be executed. By default all rules are executed. 
        /// Can be handy when the first rule checks for nulls but the rules afterwards don't. This avoids having to add extra null checks after the first rule or <see cref="NullReferenceException"/>'s.
        /// </summary>
        ExitOnInvalid = 2
    }
}
