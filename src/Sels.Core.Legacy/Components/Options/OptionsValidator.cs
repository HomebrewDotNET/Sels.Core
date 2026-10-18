using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Options;
using Sels.Core.Extensions;
using Sels.Core.Validation;

namespace Sels.Core.Options
{
    /// <summary>Adapts a Sels validator to the Microsoft options validation contract.</summary>
    /// <typeparam name="T">The options type to validate.</typeparam>
    public class OptionsValidator<T> : IValidateOptions<T>
        where T : class
    {
        // Fields
        private readonly IValidator<T, string, OptionValidationContext> _optionsValidator;

        /// <inheritdoc cref="OptionsValidator{T}"/>
        /// <param name="optionsValidator">The validator that will be used to validate instances of <typeparamref name="T"/></param>
        public OptionsValidator(IValidator<T, string, OptionValidationContext> optionsValidator)
        {
            _optionsValidator = optionsValidator.ValidateArgument(nameof(optionsValidator));
        }

        /// <inheritdoc/>
        public ValidateOptionsResult Validate(string name, T options)
        {
            name = name ?? string.Empty;
            options = options.ValidateArgument(nameof(options));

            var context = new OptionValidationContext(name);
            var errors = _optionsValidator.Validate(options, context);

            if (errors.HasValue())
            {
                return ValidateOptionsResult.Fail(errors);
            }
            else
            {
                return ValidateOptionsResult.Success;
            }
        }
    }
}
