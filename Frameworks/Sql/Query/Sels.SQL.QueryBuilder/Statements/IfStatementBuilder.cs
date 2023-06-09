using Sels.Core.Extensions;
using Sels.SQL.QueryBuilder.Builder;
using Sels.SQL.QueryBuilder.Builder.Compilation;
using Sels.SQL.QueryBuilder.Builder.Expressions;
using Sels.SQL.QueryBuilder.Builder.Statement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.SQL.QueryBuilder.Statements
{
    /// <inheritdoc cref="IIfStatementBuilder"/>
    public class IfStatementBuilder : IIfStatementBuilder, IIfFullStatementBuilder, IIfConditionOrBodyStatementBuilder, IEnumerable<IExpression>
    {
        // Fields
        private readonly IExpressionCompiler _expressionCompiler;
        private readonly IStatementCompiler<IIfStatementBuilder> _compiler;
        private List<IExpression> _conditions = new List<IExpression>();
        private IMultiStatementBuilder _bodyBuilder;
        private Dictionary<List<IExpression>, IMultiStatementBuilder> _elseIfStatements = new Dictionary<List<IExpression>, IMultiStatementBuilder>();
        private IMultiStatementBuilder _elseBodyBuilder;

        // State
        private List<IExpression> _currentConditions;
        private IMultiStatementBuilder _currentBody;

        // Properties
        /// <inheritdoc/>
        public IExpression[] ConditionExpressions => _conditions.ToArray();
        /// <inheritdoc/>
        public IMultiStatementBuilder BodyBuilder { 
            get {
                if (_bodyBuilder == null) _bodyBuilder = new MultiStatementBuilder(_expressionCompiler);
                return _bodyBuilder;
            } 
        }
        /// <inheritdoc/>
        public IReadOnlyDictionary<IExpression[], IMultiStatementBuilder> ElseIfStatements => new ReadOnlyDictionary<IExpression[], IMultiStatementBuilder>(_elseIfStatements.ToDictionary(x => x.Key.ToArray(), x => x.Value));
        /// <inheritdoc/>
        public IMultiStatementBuilder ElseBodyBuilder
        {
            get
            {
                if (_elseBodyBuilder == null) _elseBodyBuilder = new MultiStatementBuilder(_expressionCompiler);
                return _elseBodyBuilder;
            }
        }
        /// <inheritdoc/>
        public IExpression[] InnerExpressions => this.ToArray();
        /// <inheritdoc/>
        public IIfConditionOrBodyStatementBuilder ElseIf {
            get {
                _currentConditions = new List<IExpression>();
                _currentBody = new MultiStatementBuilder(_expressionCompiler);
                _elseIfStatements.Add(_currentConditions, _currentBody);
                return this;
            }
        }
        /// <inheritdoc/>
        public IIfBodyStatementBuilder Else
        {
            get
            {
                _currentBody = ElseBodyBuilder;
                return this;
            }
        }

        /// <inheritdoc cref="IfStatementBuilder"/>
        /// <param name="expressionCompiler">Expression compiler used to create <see cref="MultiStatementBuilder"/></param>
        /// <param name="compiler">Used to compile the current builder into SQL</param>
        public IfStatementBuilder(IExpressionCompiler expressionCompiler, IStatementCompiler<IIfStatementBuilder> compiler)
        {
            _expressionCompiler = expressionCompiler.ValidateArgument(nameof(expressionCompiler));
            _compiler = compiler.ValidateArgument(nameof(compiler));

            _currentConditions = _conditions;
            _currentBody = BodyBuilder;
        }

        /// <inheritdoc/>
        public IIfConditionOrBodyStatementBuilder When(IExpression expression)
        {
            expression.ValidateArgument(nameof(expression));
            if (_currentConditions == null) throw new InvalidOperationException($"No conditions to set");

            _currentConditions.Add(expression);
            return this;
        }
        /// <inheritdoc/>
        public IIfFullStatementBuilder Then(Action<IMultiStatementBuilder> builderAction)
        {
            builderAction.ValidateArgument(nameof(builderAction));
            if(_currentBody == null) throw new InvalidOperationException($"No body to configure");

            builderAction(_currentBody);
            return this;
        }

        /// <inheritdoc/>
        public StringBuilder Build(StringBuilder builder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            _compiler.CompileTo(builder, this, null, options);
            return builder;
        }
        /// <inheritdoc/>
        public IEnumerator<IExpression> GetEnumerator()
        {
            foreach(var expression in _conditions)
            {
                yield return expression;
            }
            foreach(var expression in _bodyBuilder.InnerExpressions)
            {
                yield return expression;
            }

            foreach(var elseIfStatement in _elseIfStatements)
            {
                foreach (var expression in elseIfStatement.Key)
                {
                    yield return expression;
                }
                foreach (var expression in elseIfStatement.Value.InnerExpressions)
                {
                    yield return expression;
                }
            }

            if(_elseBodyBuilder != null)
            {
                foreach (var expression in _elseBodyBuilder.InnerExpressions)
                {
                    yield return expression;
                }
            }
        }
        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
