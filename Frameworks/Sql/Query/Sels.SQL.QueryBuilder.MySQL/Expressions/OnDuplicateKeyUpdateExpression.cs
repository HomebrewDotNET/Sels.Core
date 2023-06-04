using Sels.Core.Extensions;
using Sels.Core.Extensions.Linq;
using Sels.SQL.QueryBuilder.Builder;
using Sels.SQL.QueryBuilder.Builder.Expressions;
using Sels.SQL.QueryBuilder.Builder.Expressions.Update;
using Sels.SQL.QueryBuilder.Builder.Statement;
using Sels.SQL.QueryBuilder.MySQL.Builder.Statement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sels.SQL.QueryBuilder.MySQL.Expressions
{
    /// <summary>
    /// Expressions that represents the ON DUPLICATE KEY UPDATE expression for updating values during an insert if a record matching the primary key already exists.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to build the expression for</typeparam>
    public class OnDuplicateKeyUpdateExpression<TEntity> : BaseExpressionContainer, IOnDuplicateKeyUpdateBuilder<TEntity>, IStatementSetToBuilder<TEntity, IOnDuplicateKeyUpdateValueBuilder<TEntity>>, IOnDuplicateKeyUpdateValueBuilder<TEntity>, IOnDuplicateKeyUpdateChainedBuilder<TEntity>
    {
        // Constants
        /// <summary>
        /// The MySql keywords for on duplicate key update.
        /// </summary>
        public const string KeyWord = "ON DUPLICATE KEY UPDATE";

        // Fields
        private readonly List<SetExpression<TEntity, IOnDuplicateKeyUpdateChainedBuilder<TEntity>>> _expressions = new List<SetExpression<TEntity, IOnDuplicateKeyUpdateChainedBuilder<TEntity>>>();

        // Properties
        /// <summary>
        /// The expression containing what values to update.
        /// </summary>
        public SetExpression<TEntity, IOnDuplicateKeyUpdateChainedBuilder<TEntity>>[] Expressions => _expressions.ToArray();

        /// <inheritdoc/>
        public IOnDuplicateKeyUpdateBuilder<TEntity> And => this;
        /// <inheritdoc/>
        public IOnDuplicateKeyUpdateValueBuilder<TEntity> To => this;

        /// <inheritdoc cref="OnDuplicateKeyUpdateExpression{TEntity}"/>
        /// <param name="builder">Delegate that build the current instance</param>
        public OnDuplicateKeyUpdateExpression(Action<IOnDuplicateKeyUpdateBuilder<TEntity>> builder)
        {
            Build(builder.ValidateArgument(nameof(builder)));
        }

        /// <summary>
        /// Builds the current instance using <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">Delegate that build the current instance</param>
        /// <returns>The current instance</returns>
        public OnDuplicateKeyUpdateExpression<TEntity> Build(Action<IOnDuplicateKeyUpdateBuilder<TEntity>> builder)
        {
            builder.ValidateArgument(nameof(builder));
            _expressions.Clear();
            builder(this);
            if (!_expressions.HasValue()) throw new InvalidOperationException($"{builder} did not add any expressions");
            return this;
        }
        /// <inheritdoc/>
        public IOnDuplicateKeyUpdateChainedBuilder<TEntity> Expression(IExpression expression)
        {
            expression.ValidateArgument(nameof(expression));

            return _expressions.Last().Expression(expression);
        }

        /// <inheritdoc/>
        public override void ToSql(StringBuilder builder, Action<StringBuilder, IExpression> subBuilder, ExpressionCompileOptions options = ExpressionCompileOptions.None)
        {
            builder.ValidateArgument(nameof(builder));
            subBuilder.ValidateArgument(nameof(subBuilder));

            builder.Append(KeyWord).AppendSpace();

            Expressions.Execute((i, e) =>
            {
                subBuilder(builder, e);
                if (i < Expressions.Length - 1) builder.Append(',');
            });
        }
        /// <inheritdoc/>
        IStatementSetToBuilder<TEntity, IOnDuplicateKeyUpdateValueBuilder<TEntity>> IStatementSetBuilder<TEntity, IOnDuplicateKeyUpdateValueBuilder<TEntity>>.SetExpression(IExpression sqlExpression)
        {
            sqlExpression.ValidateArgument(nameof(sqlExpression));

            var setExpression = new SetExpression<TEntity, IOnDuplicateKeyUpdateChainedBuilder<TEntity>>(this, sqlExpression);
            _expressions.Add(setExpression);
            return this;
        }
    }
}
