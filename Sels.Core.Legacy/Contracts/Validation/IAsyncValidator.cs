using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sels.Core.Validation
{
    /// <summary>
    /// Validator that can validate objects of type <typeparamref name="TEntity"/> and returns validation errors of type <typeparamref name="TError"/>.
    /// </summary>
    /// <typeparam name="TEntity">Type of objects to validate</typeparam>
    /// <typeparam name="TError">Type of validation errors to returns</typeparam>
    public interface IAsyncValidator<in TEntity, TError> : IAsyncValidator<TEntity, TError, object>
    {
    }

    /// <summary>
    /// Validator that can validate objects of type <typeparamref name="TEntity"/> and returns validation errors of type <typeparamref name="TError"/>.
    /// </summary>
    /// <typeparam name="TEntity">Type of objects to validate</typeparam>
    /// <typeparam name="TError">Type of validation errors to returns</typeparam>
    /// <typeparam name="TContext">Type of optional context to modify the behaviour of this validator</typeparam>
    public interface IAsyncValidator<in TEntity, TError, in TContext>
    {
        /// <summary>
        /// Validates <paramref name="entity"/> and returns all validation errors.
        /// </summary>
        /// <param name="entity">Entity to validate</param>
        /// <param name="context">Optional context to modify the behaviour of the validator</param>
        /// <returns>All validation errors for <paramref name="entity"/></returns>
        Task<TError[]> ValidateAsync(TEntity entity, TContext context = default);
        /// <summary>
        /// Validates <paramref name="entities"/> and returns all validation errors.
        /// </summary>
        /// <param name="entities">Entities to validate</param>
        /// <param name="context">Optional context to modify the behaviour of the validator</param>
        /// <returns>All validation errors for <paramref name="entities"/></returns>
        Task<TError[]> ValidateAsync(IEnumerable<TEntity> entities, TContext context = default);
    }
}
