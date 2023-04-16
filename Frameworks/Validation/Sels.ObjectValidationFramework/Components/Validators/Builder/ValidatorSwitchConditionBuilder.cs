using Newtonsoft.Json.Linq;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.ObjectValidationFramework.Configurators;
using Sels.ObjectValidationFramework.Rules;
using Sels.ObjectValidationFramework.Target;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static Sels.Core.Delegates.Async;

namespace Sels.ObjectValidationFramework.Validators.Builder
{
    /// <inheritdoc cref="ISwitchRootConditionConfigurator{TEntity, TError, TBaseContext, TTargetContext, TValue}"/>.
    /// <typeparam name="TEntity">Type of object to create validation rules for</typeparam>
    /// <typeparam name="TBaseContext">The type of the context used by the parent validator</typeparam>
    /// <typeparam name="TTargetContext">Type of the validation context used by all rules</typeparam>
    /// <typeparam name="TError">Type of validation error that this rule returns</typeparam>
    /// <typeparam name="TValue">Type of the value to switch on</typeparam>
    internal class ValidatorSwitchConditionBuilder<TEntity, TError, TBaseContext, TTargetContext, TValue> : 
        ISwitchRootConditionConfigurator<TEntity, TError, TBaseContext, TTargetContext, TValue>,
        ISwitchCaseConditionConfigurator<TEntity, TError, TBaseContext, TTargetContext, TValue>,
        ISwitchFullConditionConfigurator<TEntity, TError, TBaseContext, TTargetContext, TValue>
    {
        // Fields
        private readonly Action _onBuilderExit;
        private readonly List<AsyncPredicate<TValue>> _allSwitchConditions = new List<AsyncPredicate<TValue>>();
        private readonly List<AsyncPredicate<TValue>> _currentSwitchConditions = new List<AsyncPredicate<TValue>>();
        private readonly Func<TEntity, TValue> _valueGetter;
        private readonly IValidationConfigurator<TEntity, TBaseContext, TError> _parent;
        private readonly IValidationConfigurator<TEntity, TTargetContext, TError> _targetBuilder;

        // Properties
        private ISwitchRootConditionConfigurator<TEntity, TError, TBaseContext, TTargetContext, TValue> CurrentRoot => this.Cast<ISwitchRootConditionConfigurator<TEntity, TError, TBaseContext, TTargetContext, TValue>>();

        /// <inheritdoc cref="ValidatorSwitchConditionBuilder{TEntity, TError, TBaseContext, TTargetContext, TValue}"/>
        /// <param name="parent">The builder used to create the current builder</param>
        /// <param name="targetBuilder">The builder that the validation rules will be added to</param>
        /// <param name="valueGetter">Delegate that gets the value to switch on</param>
        /// <param name="onBuilderExit">Delegate called when the current builder exists and syntax returns to the parent</param>
        public ValidatorSwitchConditionBuilder(IValidationConfigurator<TEntity, TBaseContext, TError> parent, IValidationConfigurator<TEntity, TTargetContext, TError> targetBuilder, Func<TEntity, TValue> valueGetter, Action onBuilderExit)
        {
            _parent = parent.ValidateArgument(nameof(parent));
            _targetBuilder = targetBuilder.ValidateArgument(nameof(targetBuilder));
            _valueGetter = valueGetter.ValidateArgument(nameof(valueGetter));
            _onBuilderExit = onBuilderExit.ValidateArgument(nameof(onBuilderExit));
        }

        #region Root
        ISwitchCaseConditionConfigurator<TEntity, TError, TBaseContext, TTargetContext, TValue> ISwitchRootConditionConfigurator<TEntity, TError, TBaseContext, TTargetContext, TValue>.Case(AsyncPredicate<TValue> condition)
        {
            condition.ValidateArgument(nameof(condition));
            _currentSwitchConditions.Add(condition);
            _allSwitchConditions.Add(condition);
            return this;
        }

        ISwitchCaseConditionConfigurator<TEntity, TError, TBaseContext, TTargetContext, TValue> ISwitchRootConditionConfigurator<TEntity, TError, TBaseContext, TTargetContext, TValue>.Case(Predicate<TValue> condition)
        {
            condition.ValidateArgument(nameof(condition));

            return CurrentRoot.Case(x => Task.FromResult(condition(x)));
        }

        ISwitchCaseConditionConfigurator<TEntity, TError, TBaseContext, TTargetContext, TValue> ISwitchRootConditionConfigurator<TEntity, TError, TBaseContext, TTargetContext, TValue>.Case(TValue match)
        {
            return CurrentRoot.Case(x => x.Equals(match));
        }
        #endregion

        #region Case
        ISwitchCaseConditionConfigurator<TEntity, TError, TBaseContext, TTargetContext, TValue> ISwitchCaseConditionConfigurator<TEntity, TError, TBaseContext, TTargetContext, TValue>.Case(AsyncPredicate<TValue> condition)
        {
            return CurrentRoot.Case(condition);
        }

        ISwitchCaseConditionConfigurator<TEntity, TError, TBaseContext, TTargetContext, TValue> ISwitchCaseConditionConfigurator<TEntity, TError, TBaseContext, TTargetContext, TValue>.Case(Predicate<TValue> condition)
        {
            return CurrentRoot.Case(condition);
        }

        ISwitchCaseConditionConfigurator<TEntity, TError, TBaseContext, TTargetContext, TValue> ISwitchCaseConditionConfigurator<TEntity, TError, TBaseContext, TTargetContext, TValue>.Case(TValue match)
        {
            return CurrentRoot.Case(match);
        }

        ISwitchFullConditionConfigurator<TEntity, TError, TBaseContext, TTargetContext, TValue> ISwitchCaseConditionConfigurator<TEntity, TError, TBaseContext, TTargetContext, TValue>.Then(Action<IValidationConfigurator<TEntity, TTargetContext, TError>> builder)
        {
            builder.ValidateArgument(nameof(builder));
            var cases = _currentSwitchConditions.ToArray();
            _targetBuilder.ValidateWhen(async x =>
            {
                
                var value = _valueGetter(x.Source);
                foreach(var switchCase in cases)
                {
                    if(await switchCase(value).ConfigureAwait(false))
                    {
                        return true;
                    }
                }
                return false;
            }, builder);

            _currentSwitchConditions.Clear();
            return this;
        }
        #endregion

        IValidationConfigurator<TEntity, TBaseContext, TError> ISwitchFullConditionConfigurator<TEntity, TError, TBaseContext, TTargetContext, TValue>.Default(Action<IValidationConfigurator<TEntity, TTargetContext, TError>> builder)
        {
            builder.ValidateArgument(nameof(builder));

            _targetBuilder.ValidateWhen(async x =>
            {
                var value = _valueGetter(x.Source);
                foreach (var switchCase in _allSwitchConditions)
                {
                    if (await switchCase(value).ConfigureAwait(false))
                    {
                        return false;
                    }
                }
                return true;
            }, builder);

            _onBuilderExit();
            return _parent;
        }

        #region Configurator
        IValidationTargetRootConfigurator<TEntity, TError, TBaseContext, NullValidationInfo, TEntity> IValidationConfigurator<TEntity, TBaseContext, TError>.ForSource(TargetExecutionOptions settings)
        {
            _onBuilderExit();
            return _parent.ForSource(settings);
        }

        IValidationTargetRootConfigurator<TEntity, TError, TBaseContext, NullValidationInfo, TValue1> IValidationConfigurator<TEntity, TBaseContext, TError>.ForSource<TValue1>(Func<TEntity, TValue1> valueSelector, TargetExecutionOptions settings)
        {
            _onBuilderExit();
            return _parent.ForSource(valueSelector, settings);
        }

        IValidationTargetRootConfigurator<TEntity, TError, TBaseContext, PropertyValidationInfo, TPropertyValue> IValidationConfigurator<TEntity, TBaseContext, TError>.ForProperty<TPropertyValue>(System.Linq.Expressions.Expression<Func<TEntity, TPropertyValue>> property, TargetExecutionOptions settings)
        {
            _onBuilderExit();
            return _parent.ForProperty(property, settings);
        }

        IValidationTargetRootConfigurator<TEntity, TError, TBaseContext, PropertyValidationInfo, TValue1> IValidationConfigurator<TEntity, TBaseContext, TError>.ForProperty<TPropertyValue, TValue1>(Expression<Func<TEntity, TPropertyValue>> property, Func<TPropertyValue, TValue1> valueSelector, TargetExecutionOptions settings)
        {
            _onBuilderExit();
            return _parent.ForProperty(property, valueSelector, settings);
        }

        IValidationTargetRootConfigurator<TEntity, TError, TBaseContext, CollectionPropertyValidationInfo, TElement> IValidationConfigurator<TEntity, TBaseContext, TError>.ForElements<TElement>(System.Linq.Expressions.Expression<Func<TEntity, IEnumerable<TElement>>> property, TargetExecutionOptions settings)
        {
            _onBuilderExit();
            return _parent.ForElements(property, settings);
        }

        IValidationTargetRootConfigurator<TEntity, TError, TBaseContext, CollectionPropertyValidationInfo, TValue1> IValidationConfigurator<TEntity, TBaseContext, TError>.ForElements<TElement, TValue1>(Expression<Func<TEntity, IEnumerable<TElement>>> property, Func<TElement, TValue1> valueSelector, TargetExecutionOptions settings)
        {
            _onBuilderExit();
            return _parent.ForElements(property, valueSelector, settings);
        }

        IValidationConfigurator<TEntity, TBaseContext, TError> IValidationConfigurator<TEntity, TBaseContext, TError>.ValidateWhen(Predicate<IValidationRuleContext<TEntity, TBaseContext>> condition, Action<IValidationConfigurator<TEntity, TBaseContext, TError>> configurator)
        {
            _onBuilderExit();
            return _parent.ValidateWhen(condition, configurator);
        }

        IValidationConfigurator<TEntity, TBaseContext, TError> IValidationConfigurator<TEntity, TBaseContext, TError>.ValidateWhen<TNewContext>(Predicate<IValidationRuleContext<TEntity, TNewContext>> condition, Action<IValidationConfigurator<TEntity, TNewContext, TError>> configurator, bool contextRequired)
        {
            _onBuilderExit();
            return _parent.ValidateWhen(condition, configurator, contextRequired);
        }

        IValidationConfigurator<TEntity, TBaseContext, TError> IValidationConfigurator<TEntity, TBaseContext, TError>.ValidateWhen(AsyncPredicate<IValidationRuleContext<TEntity, TBaseContext>> condition, Action<IValidationConfigurator<TEntity, TBaseContext, TError>> configurator)
        {
            _onBuilderExit();
            return _parent.ValidateWhen(condition, configurator);
        }

        IValidationConfigurator<TEntity, TBaseContext, TError> IValidationConfigurator<TEntity, TBaseContext, TError>.ValidateWhen<TNewContext>(AsyncPredicate<IValidationRuleContext<TEntity, TNewContext>> condition, Action<IValidationConfigurator<TEntity, TNewContext, TError>> configurator, bool contextRequired)
        {
            _onBuilderExit();
            return _parent.ValidateWhen(condition, configurator, contextRequired);
        }

        IValidationConfigurator<TEntity, TBaseContext, TError> IValidationConfigurator<TEntity, TBaseContext, TError>.ValidateNextWhen(AsyncPredicate<IValidationRuleContext<TEntity, TBaseContext>> condition)
        {
            _onBuilderExit();
            return _parent.ValidateNextWhen(condition);
        }

        IValidationConfigurator<TEntity, TBaseContext, TError> IValidationConfigurator<TEntity, TBaseContext, TError>.ValidateNextWhen(Predicate<IValidationRuleContext<TEntity, TBaseContext>> condition)
        {
            _onBuilderExit();
            return _parent.ValidateNextWhen(condition);
        }

        ISwitchRootConditionConfigurator<TEntity, TError, TBaseContext, TBaseContext, TValue1> IValidationConfigurator<TEntity, TBaseContext, TError>.Switch<TValue1>(Func<TEntity, TValue1> valueSelector, Predicate<IValidationRuleContext<TEntity, TBaseContext>> condition)
        {
            _onBuilderExit();
            return _parent.Switch(valueSelector, condition);
        }

        ISwitchRootConditionConfigurator<TEntity, TError, TBaseContext, TBaseContext, TValue1> IValidationConfigurator<TEntity, TBaseContext, TError>.Switch<TValue1>(Func<TEntity, TValue1> valueSelector, AsyncPredicate<IValidationRuleContext<TEntity, TBaseContext>> condition)
        {
            _onBuilderExit();
            return _parent.Switch(valueSelector, condition);
        }

        ISwitchRootConditionConfigurator<TEntity, TError, TBaseContext, TNewContext, TValue1> IValidationConfigurator<TEntity, TBaseContext, TError>.Switch<TNewContext, TValue1>(Func<TEntity, TValue1> valueSelector, Predicate<IValidationRuleContext<TEntity, TNewContext>> condition, bool contextRequired)
        {
            _onBuilderExit();
            return _parent.Switch(valueSelector, condition, contextRequired);
        }

        ISwitchRootConditionConfigurator<TEntity, TError, TBaseContext, TNewContext, TValue1> IValidationConfigurator<TEntity, TBaseContext, TError>.Switch<TNewContext, TValue1>(Func<TEntity, TValue1> valueSelector, AsyncPredicate<IValidationRuleContext<TEntity, TNewContext>> condition, bool contextRequired)
        {
            _onBuilderExit();
            return _parent.Switch(valueSelector, condition, contextRequired);
        }
        #endregion
    }
}
