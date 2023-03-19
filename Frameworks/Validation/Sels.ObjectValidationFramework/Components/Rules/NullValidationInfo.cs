namespace Sels.ObjectValidationFramework.Rules
{
    /// <summary>
    /// Empty info object for <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}"/> that don't supply any additional information.
    /// </summary>
    public class NullValidationInfo
    {
        /// <summary>
        /// Singleton instance to avoid creating useless Null objects.
        /// </summary>
        internal static NullValidationInfo Instance { get; } = new NullValidationInfo();
    }
}
