using Sels.Core.Extensions;
using Sels.ObjectValidationFramework.Configurators;
using Sels.ObjectValidationFramework.Rules;
using System;
using System.IO;
using System.Net.Mail;
using System.Text.RegularExpressions;

// Adjusted so extensions are available when using the ValidationProfile
namespace Sels.ObjectValidationFramework.Profile
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
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, string> CannotBeNullOrEmpty<TEntity, TError, TBaseContext, TInfo, TTargetContext>(this IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, string> configurator, Func<IValidationRuleContext<TEntity, TInfo, TTargetContext, string>, TError> errorConstructor)
        where TTargetContext : TBaseContext
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
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>

        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, string> CannotBeNullOrEmpty<TEntity, TBaseContext, TInfo, TTargetContext>(this IValidationTargetConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, string> configurator)
        where TTargetContext : TBaseContext
        {
            configurator.ValidateArgument(nameof(configurator));

            return configurator.InvalidIf(info => string.IsNullOrEmpty(info.Value), info => $"Cannot be null or empty");
        }

        /// <summary>
        /// Value is only valid when it is not null, empty or whitespace.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, string> CannotBeNullOrWhitespace<TEntity, TError, TBaseContext, TInfo, TTargetContext>(this IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, string> configurator, Func<IValidationRuleContext<TEntity, TInfo, TTargetContext, string>, TError> errorConstructor)
        where TTargetContext : TBaseContext
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
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>

        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, string> CannotBeNullOrWhitespace<TEntity, TBaseContext, TInfo, TTargetContext>(this IValidationTargetConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, string> configurator)
        where TTargetContext : TBaseContext
        {
            configurator.ValidateArgument(nameof(configurator));

            return configurator.InvalidIf(info => string.IsNullOrEmpty(info.Value), info => $"Cannot be null, empty or whitespace");
        }

        #region Regex
        /// <summary>
        /// Value is only valid when it matches <paramref name="regex"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="regex">Regex to check against</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, string> MustMatchRegex<TEntity, TError, TBaseContext, TInfo, TTargetContext>(this IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, string> configurator, string regex, Func<IValidationRuleContext<TEntity, TInfo, TTargetContext, string>, TError> errorConstructor)
        where TTargetContext : TBaseContext
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
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="regex">Regex to check against</param>

        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, string> MustMatchRegex<TEntity, TBaseContext, TInfo, TTargetContext>(this IValidationTargetConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, string> configurator, string regex)
        where TTargetContext : TBaseContext
        {
            configurator.ValidateArgument(nameof(configurator));
            regex.ValidateArgumentNotNullOrWhitespace(nameof(regex));

            return configurator.ValidIf(info => info.Value != null && Regex.IsMatch(info.Value, regex), info => $"Must match regex <{regex}> Was <{info.Value}>");
        }

        /// <summary>
        /// Value is only valid when it matches the regex created by <paramref name="regexContructor"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="regexContructor">Delegate that returns the regex to use</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, string> MustMatchRegex<TEntity, TError, TBaseContext, TInfo, TTargetContext>(this IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, string> configurator, Func<IValidationRuleContext<TEntity, TInfo, TTargetContext, string>, string> regexContructor, Func<IValidationRuleContext<TEntity, TInfo, TTargetContext, string>, TError> errorConstructor)
        where TTargetContext : TBaseContext
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));
            regexContructor.ValidateArgument(nameof(regexContructor));

            return configurator.ValidIf(info => info.Value != null && Regex.IsMatch(info.Value, regexContructor(info)), errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it matches the regex created by <paramref name="regexContructor"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="regexContructor">Delegate that returns the regex to use</param>

        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, string> MustMatchRegex<TEntity, TBaseContext, TInfo, TTargetContext>(this IValidationTargetConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, string> configurator, Func<IValidationRuleContext<TEntity, TInfo, TTargetContext, string>, string> regexContructor)
        where TTargetContext : TBaseContext
        {
            configurator.ValidateArgument(nameof(configurator));
            regexContructor.ValidateArgument(nameof(regexContructor));

            return configurator.MustMatchRegex(regexContructor, info => $"Must match regex <{regexContructor(info)}> Was <{info.Value}>");
        }

        /// <summary>
        /// Value is only valid when it matches <paramref name="regex"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="regex">Regex to check against</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, string> MustMatchRegex<TEntity, TError, TBaseContext, TInfo, TTargetContext>(this IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, string> configurator, Regex regex, Func<IValidationRuleContext<TEntity, TInfo, TTargetContext, string>, TError> errorConstructor)
        where TTargetContext : TBaseContext
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
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="regex">Regex to check against</param>

        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, string> MustMatchRegex<TEntity, TBaseContext, TInfo, TTargetContext>(this IValidationTargetConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, string> configurator, Regex regex)
        where TTargetContext : TBaseContext
        {
            configurator.ValidateArgument(nameof(configurator));
            regex.ValidateArgument(nameof(regex));

            return configurator.ValidIf(info => regex.IsMatch(info.Value), info => $"Must match regex <{regex}> Was <{info.Value}>");
        }

        /// <summary>
        /// Value is only valid when it doesn't match <paramref name="regex"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="regex">Regex to check against</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, string> CannotMatchRegex<TEntity, TError, TBaseContext, TInfo, TTargetContext>(this IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, string> configurator, string regex, Func<IValidationRuleContext<TEntity, TInfo, TTargetContext, string>, TError> errorConstructor)
        where TTargetContext : TBaseContext
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
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="regex">Regex to check against</param>

        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, string> CannotMatchRegex<TEntity, TBaseContext, TInfo, TTargetContext>(this IValidationTargetConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, string> configurator, string regex)
        where TTargetContext : TBaseContext
        {
            configurator.ValidateArgument(nameof(configurator));
            regex.ValidateArgumentNotNullOrWhitespace(nameof(regex));

            return configurator.InvalidIf(info => Regex.IsMatch(info.Value, regex), info => $"Cannot match regex <{regex}> Was <{info.Value}>");
        }

        /// <summary>
        /// Value is only valid when it doesn't match <paramref name="regex"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="regex">Regex to check against</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, string> CannotMatchRegex<TEntity, TError, TBaseContext, TInfo, TTargetContext>(this IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, string> configurator, Regex regex, Func<IValidationRuleContext<TEntity, TInfo, TTargetContext, string>, TError> errorConstructor)
        where TTargetContext : TBaseContext
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
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="regex">Regex to check against</param>

        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, string> CannotMatchRegex<TEntity, TBaseContext, TInfo, TTargetContext>(this IValidationTargetConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, string> configurator, Regex regex)
        where TTargetContext : TBaseContext
        {
            configurator.ValidateArgument(nameof(configurator));
            regex.ValidateArgument(nameof(regex));

            return configurator.InvalidIf(info => regex.IsMatch(info.Value), info => $"Cannot match regex <{regex}> Was <{info.Value}>");
        }
        #endregion

        #region Length
        /// <summary>
        /// Value is only valid when it's length is smaller or equal to <paramref name="length"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="length">Max allowed length</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, string> HasMaxLengthOf<TEntity, TError, TBaseContext, TInfo, TTargetContext>(this IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, string> configurator, int length, Func<IValidationRuleContext<TEntity, TInfo, TTargetContext, string>, TError> errorConstructor)
        where TTargetContext : TBaseContext
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));
            length.ValidateArgumentLargerOrEqual(nameof(length), 0);

            return configurator.ValidIf(info => info.Value == null || info.Value.Length <= length, errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it's length is smaller or equal to <paramref name="length"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="length">Max allowed length</param>

        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, string> HasMaxLengthOf<TEntity, TBaseContext, TInfo, TTargetContext>(this IValidationTargetConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, string> configurator, int length)
        where TTargetContext : TBaseContext
        {
            configurator.ValidateArgument(nameof(configurator));
            length.ValidateArgumentLargerOrEqual(nameof(length), 0);

            return configurator.ValidIf(info => info.Value == null || info.Value.Length <= length, info => $"Length cannot be higher than {length}. Was <{info.Value}>");
        }

        /// <summary>
        /// Value is only valid when it's length is larger or equal to <paramref name="length"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="length">Min allowed length</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, string> HasMinLengthOf<TEntity, TError, TBaseContext, TInfo, TTargetContext>(this IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, string> configurator, int length, Func<IValidationRuleContext<TEntity, TInfo, TTargetContext, string>, TError> errorConstructor)
        where TTargetContext : TBaseContext
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));
            length.ValidateArgumentLargerOrEqual(nameof(length), 0);

            return configurator.ValidIf(info => info.Value != null && info.Value.Length >= length, errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it's length is larger or equal to <paramref name="length"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="length">Min allowed length</param>

        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, string> HasMinLengthOf<TEntity, TBaseContext, TInfo, TTargetContext>(this IValidationTargetConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, string> configurator, int length)
        where TTargetContext : TBaseContext
        {
            configurator.ValidateArgument(nameof(configurator));
            length.ValidateArgumentLargerOrEqual(nameof(length), 0);

            return configurator.ValidIf(info => info.Value != null && info.Value.Length >= length, info => $"Length cannot be lower than {length}. Was <{info.Value}>");
        }

        /// <summary>
        /// Value is only valid when it's length is larger or equal to <paramref name="minLength"/> and smaller or equal to <paramref name="maxLength"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="minLength">Min allowed length</param>
        /// <param name="maxLength">Max allowed length</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, string> HasLengthInRangeOf<TEntity, TError, TBaseContext, TInfo, TTargetContext>(this IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, string> configurator, int minLength, int maxLength, Func<IValidationRuleContext<TEntity, TInfo, TTargetContext, string>, TError> errorConstructor)
        where TTargetContext : TBaseContext
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));
            minLength.ValidateArgumentLargerOrEqual(nameof(minLength), 0);
            maxLength.ValidateArgumentLarger(nameof(maxLength), minLength);

            return configurator.ValidIf(info => info.Value != null && info.Value.Length >= minLength && info.Value.Length <= maxLength, errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it's length is larger or equal to <paramref name="minLength"/> and smaller or equal to <paramref name="maxLength"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="minLength">Min allowed length</param>
        /// <param name="maxLength">Max allowed length</param>

        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, string> HasLengthInRangeOf<TEntity, TBaseContext, TInfo, TTargetContext>(this IValidationTargetConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, string> configurator, int minLength, int maxLength)
        where TTargetContext : TBaseContext
        {
            configurator.ValidateArgument(nameof(configurator));
            minLength.ValidateArgumentLargerOrEqual(nameof(minLength), 0);
            maxLength.ValidateArgumentLarger(nameof(maxLength), minLength);

            return configurator.ValidIf(info => info.Value != null && info.Value.Length >= minLength && info.Value.Length <= maxLength, info => $"Length cannot be lower than {minLength} and higher than {maxLength}. Was <{info.Value}>");
        }

        /// <summary>
        /// Value is only valid when it's length is larger than <paramref name="minLength"/> and smaller than <paramref name="maxLength"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="minLength">Min allowed length</param>
        /// <param name="maxLength">Max allowed length</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, string> HasLengthBetween<TEntity, TError, TBaseContext, TInfo, TTargetContext>(this IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, string> configurator, int minLength, int maxLength, Func<IValidationRuleContext<TEntity, TInfo, TTargetContext, string>, TError> errorConstructor)
        where TTargetContext : TBaseContext
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));
            minLength.ValidateArgumentLargerOrEqual(nameof(minLength), 0);
            maxLength.ValidateArgumentLarger(nameof(maxLength), minLength);

            return configurator.ValidIf(info => info.Value != null && info.Value.Length > minLength && info.Value.Length < maxLength, errorConstructor);
        }

        /// <summary>
        /// Value is only valid when it's length is larger than <paramref name="minLength"/> and smaller than <paramref name="maxLength"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="minLength">Min allowed length</param>
        /// <param name="maxLength">Max allowed length</param>

        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, string> HasLengthBetween<TEntity, TBaseContext, TInfo, TTargetContext>(this IValidationTargetConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, string> configurator, int minLength, int maxLength)
        where TTargetContext : TBaseContext
        {
            configurator.ValidateArgument(nameof(configurator));
            minLength.ValidateArgumentLargerOrEqual(nameof(minLength), 0);
            maxLength.ValidateArgumentLarger(nameof(maxLength), minLength);

            return configurator.ValidIf(info => info.Value != null && info.Value.Length > minLength && info.Value.Length < maxLength, info => $"Length cannot be lower than {minLength} and higher than {maxLength}. Was <{info.Value}>");
        }
        #endregion

        #region FileSystem
        /// <summary>
        /// Value is only valid when it is a valid directory path.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, string> MustBeValidPath<TEntity, TError, TBaseContext, TInfo, TTargetContext>(this IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, string> configurator, Func<IValidationRuleContext<TEntity, TInfo, TTargetContext, string>, TError> errorConstructor)
        where TTargetContext : TBaseContext
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
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>

        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, string> MustBeValidPath<TEntity, TBaseContext, TInfo, TTargetContext>(this IValidationTargetConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, string> configurator)
        where TTargetContext : TBaseContext
        {
            configurator.ValidateArgument(nameof(configurator));

            var invalidPathChars = Path.GetInvalidPathChars();
            var invalidPathCharString = invalidPathChars.JoinString(", ");
            return configurator.InvalidIf(info => info.Value.Contains(invalidPathChars), info => $"Must be a valid directory path. Was <{info.Value}>. Cannot contain <{invalidPathCharString}>");
        }

        /// <summary>
        /// Value is only valid when it is a valid file name.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, string> MustBeValidFileName<TEntity, TError, TBaseContext, TInfo, TTargetContext>(this IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, string> configurator, Func<IValidationRuleContext<TEntity, TInfo, TTargetContext, string>, TError> errorConstructor)
        where TTargetContext : TBaseContext
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
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>

        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, string> MustBeValidFileName<TEntity, TBaseContext, TInfo, TTargetContext>(this IValidationTargetConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, string> configurator)
        where TTargetContext : TBaseContext
        {
            configurator.ValidateArgument(nameof(configurator));

            var invalidFileNameChars = Path.GetInvalidFileNameChars();
            var invalidFileNameCharString = invalidFileNameChars.JoinString(", ");
            return configurator.InvalidIf(info => info.Value.Contains(invalidFileNameChars), info => $"Must be a valid file name. Was <{info.Value}>. Cannot contain <{invalidFileNameCharString}>");
        }

        /// <summary>
        /// Value is only valid when it points to an existing directory.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, string> MustBeExistingPath<TEntity, TError, TBaseContext, TInfo, TTargetContext>(this IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, string> configurator, Func<IValidationRuleContext<TEntity, TInfo, TTargetContext, string>, TError> errorConstructor)
        where TTargetContext : TBaseContext
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
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>

        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, string> MustBeExistingPath<TEntity, TBaseContext, TInfo, TTargetContext>(this IValidationTargetConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, string> configurator)
        where TTargetContext : TBaseContext
        {
            configurator.ValidateArgument(nameof(configurator));

            return configurator.ValidIf(info => Directory.Exists(info.Value), info => $"Must point to an existing directory. Was <{info.Value}>.");
        }

        /// <summary>
        /// Value is only valid when it points to an existing file.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, string> MustBeExistingFileName<TEntity, TError, TBaseContext, TInfo, TTargetContext>(this IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, string> configurator, Func<IValidationRuleContext<TEntity, TInfo, TTargetContext, string>, TError> errorConstructor)
        where TTargetContext : TBaseContext
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
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>

        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, string> MustBeExistingFileName<TEntity, TBaseContext, TInfo, TTargetContext>(this IValidationTargetConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, string> configurator)
        where TTargetContext : TBaseContext
        {
            configurator.ValidateArgument(nameof(configurator));

            return configurator.ValidIf(info => File.Exists(info.Value), info => $"Must point to an existing directory. Was <{info.Value}>.");
        }
        #endregion

        #region Type
        /// <summary>
        /// Value is only valid when it is a valid type name.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, string> MustBeValidTypeName<TEntity, TError, TBaseContext, TInfo, TTargetContext>(this IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, string> configurator, Func<IValidationRuleContext<TEntity, TInfo, TTargetContext, string>, TError> errorConstructor)
        where TTargetContext : TBaseContext
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
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>

        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, string> MustBeValidTypeName<TEntity, TBaseContext, TInfo, TTargetContext>(this IValidationTargetConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, string> configurator)
        where TTargetContext : TBaseContext
        {
            configurator.ValidateArgument(nameof(configurator));

            return configurator.InvalidIf(info => Type.GetType(info.Value, false) == null, info => $"Must be a valid type name. Was <{info.Value}>");
        }
        #endregion

        #region Email
        /// <summary>
        /// Value is only valid when it is a valid mail address.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>

        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, string> IsValidEmail<TEntity, TBaseContext, TInfo, TTargetContext>(this IValidationTargetConfigurator<TEntity, string, TBaseContext, TInfo, TTargetContext, string> configurator)
        where TTargetContext : TBaseContext
        {
            configurator.ValidateArgument(nameof(configurator));

            return configurator.IsValidEmail(info => $"Cannot be null or empty");
        }

        /// <summary>
        /// Value is only valid when it is a valid mail address.
        /// </summary>
        /// <typeparam name="TEntity">Type of source object that the validation rule was created for</typeparam>
        /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
        /// <typeparam name="TInfo">Type of object that contains additional info that the validation rule can use depending on what is being validated</typeparam>
        /// <typeparam name="TBaseContext">Type of the validation context used by the current validator</typeparam>
        /// <typeparam name="TTargetContext">Type of the validation context used by the current validation target</typeparam>
        /// <param name="configurator">Configurator to configure validation</param>
        /// <param name="errorConstructor">Delegate that creates a validation error when <see cref="IValidationRuleContext{TEntity, TInfo, TContext, TValue}.Value"/> is not a valid value</param>
        /// <returns>Current configurator</returns>
        public static IValidationRuleConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, string> IsValidEmail<TEntity, TError, TBaseContext, TInfo, TTargetContext>(this IValidationTargetConfigurator<TEntity, TError, TBaseContext, TInfo, TTargetContext, string> configurator, Func<IValidationRuleContext<TEntity, TInfo, TTargetContext, string>, TError> errorConstructor)
        where TTargetContext : TBaseContext
        {
            configurator.ValidateArgument(nameof(configurator));
            errorConstructor.ValidateArgument(nameof(errorConstructor));

            return configurator.InvalidIf(info =>
            {
                if (!info.Value.HasValue()) return false;
                try
                {
                    var address = new MailAddress(info.Value).Address;
                    return address.Equals(info.Value.GetWithoutWhitespace());
                }
                catch (FormatException)
                {
                    return false;
                }
            }, errorConstructor);
        }
        #endregion
    }
}
