using Microsoft.Extensions.Caching.Memory;
using Sels.ObjectValidationFramework.Profile;
using Sels.SQL.QueryBuilder.Builder;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.SQL.QueryBuilder.MySQL
{
    /// <summary>
    /// Exposes extra options for <see cref="MySqlCompiler"/>.
    /// </summary>
    public class MySqlCompilerOptions
    {
        /// <summary>
        /// The threshold above which a warning will be logged when any method on <see cref="MySqlCompiler"/> takes longer to execute.
        /// </summary>
        public TimeSpan PerformanceWarningDurationThreshold { get; set; } = TimeSpan.FromMilliseconds(100);
        /// <summary>
        /// The threshold above which an error will be logged when any method on <see cref="MySqlCompiler"/> takes longer to execute.
        /// </summary>
        public TimeSpan PerformanceErrorDurationThreshold { get; set; } = TimeSpan.FromMilliseconds(500);
    }
    /// <summary>
    /// Contains the validation rules for <see cref="MySqlCompilerOptions"/>.
    /// </summary>
    internal class MySqlCompilerOptionsValidationProfile : ValidationProfile<string>
    {
        /// <inheritdoc cref="MySqlCompilerOptionsValidationProfile"/>
        public MySqlCompilerOptionsValidationProfile()
        {
            CreateValidationFor<MySqlCompilerOptions>()
                .ForProperty(x => x.PerformanceErrorDurationThreshold)
                    .ValidIf(x => x.Value > x.Source.PerformanceWarningDurationThreshold, x => $"Must be larger than <{nameof(x.Source.PerformanceWarningDurationThreshold)}>");
        }
    }
}
