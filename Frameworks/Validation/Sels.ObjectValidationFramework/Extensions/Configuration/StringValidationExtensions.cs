using Sels.Core.Extensions;
using Sels.ObjectValidationFramework.Contracts.Rules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

// Adjusted so extensions are available when using the ValidationProfile
namespace Sels.ObjectValidationFramework.Templates.Profile
{
    /// <summary>
    /// Contains generic configuration extensions for strings.
    /// </summary>
    public static class StringValidationExtensions
    {
        /// <summary>
        /// Value is only valid when it is not null or empty.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, string> CannotBeNullOrEmpty<TEntity, TError, TInfo, TContext>(this IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, string> configurator, Func<IValidationRuleContext<TEntity, TInfo, TContext, string>, TError> errorConstructor)
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));

            return configurator.InvalidIf(info => string.IsNullOrEmpty(info.Value), errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it is not null or empty.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="includeParents">If the hierarchy of parents should be included in the display name</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TInfo, TContext, string> CannotBeNullOrEmpty<TEntity, TInfo, TContext>(this IValidationRuleConfigurator<TEntity, string, TInfo, TContext, string> configurator, bool includeParents = true)
        {
            configurator.ValidateArgument(nameof(configurator));

            return configurator.InvalidIf(info => string.IsNullOrEmpty(info.Value), info => $"{info.GetFullDisplayNameDynamically(includeParents)} cannot be null or empty");
        }

        /// <summary>
        /// Value is only valid when it is not null, empty or whitespace.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, string> CannotBeNullOrWhitespace<TEntity, TError, TInfo, TContext>(this IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, string> configurator, Func<IValidationRuleContext<TEntity, TInfo, TContext, string>, TError> errorConstructor)
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));

            return configurator.InvalidIf(info => string.IsNullOrWhiteSpace(info.Value), errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it is not null, empty or whitespace.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="includeParents">If the hierarchy of parents should be included in the display name</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TInfo, TContext, string> CannotBeNullOrWhitespace<TEntity, TInfo, TContext>(this IValidationRuleConfigurator<TEntity, string, TInfo, TContext, string> configurator, bool includeParents = true)
        {
            configurator.ValidateArgument(nameof(configurator));

            return configurator.InvalidIf(info => string.IsNullOrEmpty(info.Value), info => $"{info.GetFullDisplayNameDynamically(includeParents)} cannot be null, empty or whitespace");
        }

        #region Regex
        /// <summary>
        /// Value is only valid when it matches <paramref name="regex"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="regex">Regex to check against</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, string> MustMatchRegex<TEntity, TError, TInfo, TContext>(this IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, string> configurator, string regex, Func<IValidationRuleContext<TEntity, TInfo, TContext, string>, TError> errorConstructor)
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));
            regex.ValidateArgumentNotNullOrWhitespace(nameof(regex));

            return configurator.ValidIf(info => info.Value != null && Regex.IsMatch(info.Value, regex), errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it matches <paramref name="regex"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="regex">Regex to check against</param>
        /// <param name="includeParents">If the hierarchy of parents should be included in the display name</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TInfo, TContext, string> MustMatchRegex<TEntity, TInfo, TContext>(this IValidationRuleConfigurator<TEntity, string, TInfo, TContext, string> configurator, string regex, bool includeParents = true)
        {
            configurator.ValidateArgument(nameof(configurator));
            regex.ValidateArgumentNotNullOrWhitespace(nameof(regex));

            return configurator.ValidIf(info => info.Value != null && Regex.IsMatch(info.Value, regex), info => $"{info.GetFullDisplayNameDynamically(includeParents)} must match regex <{regex}> Was <{info.Value}>");
        }

        /// <summary>
        /// Value is only valid when it matches <paramref name="regex"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="regex">Regex to check against</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, string> MustMatchRegex<TEntity, TError, TInfo, TContext>(this IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, string> configurator, Regex regex, Func<IValidationRuleContext<TEntity, TInfo, TContext, string>, TError> errorConstructor)
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));
            regex.ValidateArgument(nameof(regex));

            return configurator.ValidIf(info => regex.IsMatch(info.Value), errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it matches <paramref name="regex"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="regex">Regex to check against</param>
        /// <param name="includeParents">If the hierarchy of parents should be included in the display name</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TInfo, TContext, string> MustMatchRegex<TEntity, TInfo, TContext>(this IValidationRuleConfigurator<TEntity, string, TInfo, TContext, string> configurator, Regex regex, bool includeParents = true)
        {
            configurator.ValidateArgument(nameof(configurator));
            regex.ValidateArgument(nameof(regex));

            return configurator.ValidIf(info => regex.IsMatch(info.Value), info => $"{info.GetFullDisplayNameDynamically(includeParents)} must match regex <{regex}> Was <{info.Value}>");
        }

        /// <summary>
        /// Value is only valid when it doesn't match <paramref name="regex"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="regex">Regex to check against</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, string> CannotMatchRegex<TEntity, TError, TInfo, TContext>(this IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, string> configurator, string regex, Func<IValidationRuleContext<TEntity, TInfo, TContext, string>, TError> errorConstructor)
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));
            regex.ValidateArgumentNotNullOrWhitespace(nameof(regex));

            return configurator.InvalidIf(info => Regex.IsMatch(info.Value, regex), errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it doesn't match <paramref name="regex"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="regex">Regex to check against</param>
        /// <param name="includeParents">If the hierarchy of parents should be included in the display name</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TInfo, TContext, string> CannotMatchRegex<TEntity, TInfo, TContext>(this IValidationRuleConfigurator<TEntity, string, TInfo, TContext, string> configurator, string regex, bool includeParents = true)
        {
            configurator.ValidateArgument(nameof(configurator));
            regex.ValidateArgumentNotNullOrWhitespace(nameof(regex));

            return configurator.InvalidIf(info => Regex.IsMatch(info.Value, regex), info => $"{info.GetFullDisplayNameDynamically(includeParents)} cannot match regex <{regex}> Was <{info.Value}>");
        }

        /// <summary>
        /// Value is only valid when it doesn't match <paramref name="regex"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="regex">Regex to check against</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, string> CannotMatchRegex<TEntity, TError, TInfo, TContext>(this IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, string> configurator, Regex regex, Func<IValidationRuleContext<TEntity, TInfo, TContext, string>, TError> errorConstructor)
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));
            regex.ValidateArgument(nameof(regex));

            return configurator.InvalidIf(info => regex.IsMatch(info.Value), errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it doesn't match <paramref name="regex"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="regex">Regex to check against</param>
        /// <param name="includeParents">If the hierarchy of parents should be included in the display name</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TInfo, TContext, string> CannotMatchRegex<TEntity, TInfo, TContext>(this IValidationRuleConfigurator<TEntity, string, TInfo, TContext, string> configurator, Regex regex, bool includeParents = true)
        {
            configurator.ValidateArgument(nameof(configurator));
            regex.ValidateArgument(nameof(regex));

            return configurator.InvalidIf(info => regex.IsMatch(info.Value), info => $"{info.GetFullDisplayNameDynamically(includeParents)} cannot match regex <{regex}> Was <{info.Value}>");
        }
        #endregion

        #region Length
        /// <summary>
        /// Value is only valid when it's length is smaller or equal to <paramref name="length"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="length">Max allowed length</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, string> HasMaxLengthOf<TEntity, TError, TInfo, TContext>(this IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, string> configurator, int length, Func<IValidationRuleContext<TEntity, TInfo, TContext, string>, TError> errorConstructor)
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));
            length.ValidateArgumentLargerOrEqual(nameof(length), 0);

            return configurator.ValidIf(info => info.Value.Length <= length, errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it's length is smaller or equal to <paramref name="length"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="length">Max allowed length</param>
        /// <param name="includeParents">If the hierarchy of parents should be included in the display name</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TInfo, TContext, string> HasMaxLengthOf<TEntity, TInfo, TContext>(this IValidationRuleConfigurator<TEntity, string, TInfo, TContext, string> configurator, int length, bool includeParents = true)
        {
            configurator.ValidateArgument(nameof(configurator));
            length.ValidateArgumentLargerOrEqual(nameof(length), 0);

            return configurator.ValidIf(info => info.Value.Length <= length, info => $"{info.GetFullDisplayNameDynamically(includeParents)} length cannot be higher than {length}. Was <{info.Value}>");
        }

        /// <summary>
        /// Value is only valid when it's length is larger or equal to <paramref name="length"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="length">Min allowed length</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, string> HasMinLengthOf<TEntity, TError, TInfo, TContext>(this IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, string> configurator, int length, Func<IValidationRuleContext<TEntity, TInfo, TContext, string>, TError> errorConstructor)
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));
            length.ValidateArgumentLargerOrEqual(nameof(length), 0);

            return configurator.ValidIf(info => info.Value.Length >= length, errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it's length is larger or equal to <paramref name="length"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="length">Min allowed length</param>
        /// <param name="includeParents">If the hierarchy of parents should be included in the display name</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TInfo, TContext, string> HasMinLengthOf<TEntity, TInfo, TContext>(this IValidationRuleConfigurator<TEntity, string, TInfo, TContext, string> configurator, int length, bool includeParents = true)
        {
            configurator.ValidateArgument(nameof(configurator));
            length.ValidateArgumentLargerOrEqual(nameof(length), 0);

            return configurator.ValidIf(info => info.Value.Length >= length, info => $"{info.GetFullDisplayNameDynamically(includeParents)} length cannot be lower than {length}. Was <{info.Value}>");
        }

        /// <summary>
        /// Value is only valid when it's length is larger or equal to <paramref name="minLength"/> and smaller or equal to <paramref name="maxLength"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="minLength">Min allowed length</param>
        /// <param name="maxLength">Max allowed length</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, string> HasLengthInRangeOf<TEntity, TError, TInfo, TContext>(this IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, string> configurator, int minLength, int maxLength, Func<IValidationRuleContext<TEntity, TInfo, TContext, string>, TError> errorConstructor)
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));
            minLength.ValidateArgumentLargerOrEqual(nameof(minLength), 0);
            maxLength.ValidateArgumentLarger(nameof(maxLength), minLength);

            return configurator.ValidIf(info => info.Value.Length >= minLength && info.Value.Length <= maxLength, errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it's length is larger or equal to <paramref name="minLength"/> and smaller or equal to <paramref name="maxLength"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="minLength">Min allowed length</param>
        /// <param name="maxLength">Max allowed length</param>
        /// <param name="includeParents">If the hierarchy of parents should be included in the display name</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TInfo, TContext, string> HasLengthInRangeOf<TEntity, TInfo, TContext>(this IValidationRuleConfigurator<TEntity, string, TInfo, TContext, string> configurator, int minLength, int maxLength, bool includeParents = true)
        {
            configurator.ValidateArgument(nameof(configurator));
            minLength.ValidateArgumentLargerOrEqual(nameof(minLength), 0);
            maxLength.ValidateArgumentLarger(nameof(maxLength), minLength);

            return configurator.ValidIf(info => info.Value.Length >= minLength && info.Value.Length <= maxLength, info => $"{info.GetFullDisplayNameDynamically(includeParents)} length cannot be lower than {minLength} and higher than {maxLength}. Was <{info.Value}>");
        }

        /// <summary>
        /// Value is only valid when it's length is larger than <paramref name="minLength"/> and smaller than <paramref name="maxLength"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="minLength">Min allowed length</param>
        /// <param name="maxLength">Max allowed length</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, string> HasLengthBetween<TEntity, TError, TInfo, TContext>(this IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, string> configurator, int minLength, int maxLength, Func<IValidationRuleContext<TEntity, TInfo, TContext, string>, TError> errorConstructor)
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));
            minLength.ValidateArgumentLargerOrEqual(nameof(minLength), 0);
            maxLength.ValidateArgumentLarger(nameof(maxLength), minLength);

            return configurator.ValidIf(info => info.Value.Length > minLength && info.Value.Length < maxLength, errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it's length is larger than <paramref name="minLength"/> and smaller than <paramref name="maxLength"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="minLength">Min allowed length</param>
        /// <param name="maxLength">Max allowed length</param>
        /// <param name="includeParents">If the hierarchy of parents should be included in the display name</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TInfo, TContext, string> HasLengthBetween<TEntity, TInfo, TContext>(this IValidationRuleConfigurator<TEntity, string, TInfo, TContext, string> configurator, int minLength, int maxLength, bool includeParents = true)
        {
            configurator.ValidateArgument(nameof(configurator));
            minLength.ValidateArgumentLargerOrEqual(nameof(minLength), 0);
            maxLength.ValidateArgumentLarger(nameof(maxLength), minLength);

            return configurator.ValidIf(info => info.Value.Length > minLength && info.Value.Length < maxLength, info => $"{info.GetFullDisplayNameDynamically(includeParents)} length cannot be lower than {minLength} and higher than {maxLength}. Was <{info.Value}>");
        }
        #endregion

        #region FileSystem
        /// <summary>
        /// Value is only valid when it is a valid directory path.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, string> MustBeValidPath<TEntity, TError, TInfo, TContext>(this IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, string> configurator, Func<IValidationRuleContext<TEntity, TInfo, TContext, string>, TError> errorConstructor)
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));

            var invalidPathChars = Path.GetInvalidPathChars();
            return configurator.InvalidIf(info => info.Value.Contains(invalidPathChars), errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it is a valid directory path.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="includeParents">If the hierarchy of parents should be included in the display name</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TInfo, TContext, string> MustBeValidPath<TEntity, TInfo, TContext>(this IValidationRuleConfigurator<TEntity, string, TInfo, TContext, string> configurator, bool includeParents = true)
        {
            configurator.ValidateArgument(nameof(configurator));

            var invalidPathChars = Path.GetInvalidPathChars();
            var invalidPathCharString = invalidPathChars.JoinString(", ");
            return configurator.InvalidIf(info => info.Value.Contains(invalidPathChars), info => $"{info.GetFullDisplayNameDynamically(includeParents)} must be a valid directory path. Was <{info.Value}>. Cannot contain <{invalidPathCharString}>");
        }

        /// <summary>
        /// Value is only valid when it is a valid file name.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, string> MustBeValidFileName<TEntity, TError, TInfo, TContext>(this IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, string> configurator, Func<IValidationRuleContext<TEntity, TInfo, TContext, string>, TError> errorConstructor)
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));

            var invalidFileNameChars = Path.GetInvalidFileNameChars();
            return configurator.InvalidIf(info => info.Value.Contains(invalidFileNameChars), errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it is a valid file name.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="includeParents">If the hierarchy of parents should be included in the display name</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TInfo, TContext, string> MustBeValidFileName<TEntity, TInfo, TContext>(this IValidationRuleConfigurator<TEntity, string, TInfo, TContext, string> configurator, bool includeParents = true)
        {
            configurator.ValidateArgument(nameof(configurator));

            var invalidFileNameChars = Path.GetInvalidFileNameChars();
            var invalidFileNameCharString = invalidFileNameChars.JoinString(", ");
            return configurator.InvalidIf(info => info.Value.Contains(invalidFileNameChars), info => $"{info.GetFullDisplayNameDynamically(includeParents)} must be a valid file name. Was <{info.Value}>. Cannot contain <{invalidFileNameCharString}>");
        }

        /// <summary>
        /// Value is only valid when it points to an existing directory.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, string> MustBeExistingPath<TEntity, TError, TInfo, TContext>(this IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, string> configurator, Func<IValidationRuleContext<TEntity, TInfo, TContext, string>, TError> errorConstructor)
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));

            return configurator.ValidIf(info => Directory.Exists(info.Value), errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it points to an existing directory.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="includeParents">If the hierarchy of parents should be included in the display name</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TInfo, TContext, string> MustBeExistingPath<TEntity, TInfo, TContext>(this IValidationRuleConfigurator<TEntity, string, TInfo, TContext, string> configurator, bool includeParents = true)
        {
            configurator.ValidateArgument(nameof(configurator));

            return configurator.ValidIf(info => Directory.Exists(info.Value), info => $"{info.GetFullDisplayNameDynamically(includeParents)} must point to an existing directory. Was <{info.Value}>.");
        }

        /// <summary>
        /// Value is only valid when it points to an existing file.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, string> MustBeExistingFileName<TEntity, TError, TInfo, TContext>(this IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, string> configurator, Func<IValidationRuleContext<TEntity, TInfo, TContext, string>, TError> errorConstructor)
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));

            return configurator.ValidIf(info => File.Exists(info.Value), errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it points to an existing file.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="includeParents">If the hierarchy of parents should be included in the display name</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TInfo, TContext, string> MustBeExistingFileName<TEntity, TInfo, TContext>(this IValidationRuleConfigurator<TEntity, string, TInfo, TContext, string> configurator, bool includeParents = true)
        {
            configurator.ValidateArgument(nameof(configurator));

            return configurator.ValidIf(info => File.Exists(info.Value), info => $"{info.GetFullDisplayNameDynamically(includeParents)} must point to an existing directory. Was <{info.Value}>.");
        }
        #endregion

        #region Type
        /// <summary>
        /// Value is only valid when it is a valid type name.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, string> MustBeValidTypeName<TEntity, TError, TInfo, TContext>(this IValidationRuleConfigurator<TEntity, TError, TInfo, TContext, string> configurator, Func<IValidationRuleContext<TEntity, TInfo, TContext, string>, TError> errorConstructor)
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));

            return configurator.InvalidIf(info => Type.GetType(info.Value, false) == null, errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it is a valid type name.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TContext">Optional context that can be supplied to a validation profile</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="includeParents">If the hierarchy of parents should be included in the display name</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TInfo, TContext, string> MustBeValidTypeName<TEntity, TInfo, TContext>(this IValidationRuleConfigurator<TEntity, string, TInfo, TContext, string> configurator, bool includeParents = true)
        {
            configurator.ValidateArgument(nameof(configurator));

            return configurator.InvalidIf(info => Type.GetType(info.Value, false) == null, info => $"{info.GetFullDisplayNameDynamically(includeParents)} must be a valid type name. Was <{info.Value}>");
        }
        #endregion              
    }
}
