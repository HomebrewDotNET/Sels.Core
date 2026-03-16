using Sels.ObjectValidationFramework.Profile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.DistributedLocking.IntegrationTester.Tests
{
    /// <summary>
    /// Exposes extra settings for <see cref="AssertionTester"/>.
    /// </summary>
    public class AssertionTesterOptions
    {
        /// <summary>
        /// Optional regex filters to mimit which assertions can run. When null/empty all assertions can run.
        /// </summary>
        public string[] Filters { get; set; }
        /// <summary>
        /// How long to wait for some test cases to complete
        /// </summary>
        public TimeSpan MaxWaitTime { get; set; } = TimeSpan.FromSeconds(10);
    }

    /// <summary>
    /// Validation for <see cref="AssertionTesterOptions"/>.
    /// </summary>
    public class AssertionTesterOptionsValidationProfile : ValidationProfile<string>
    {
        /// <inheritdoc cref="AssertionTesterOptionsValidationProfile"/>
        public AssertionTesterOptionsValidationProfile()
        {
            CreateValidationFor<AssertionTesterOptions>()
                .ForElements(x => x.Filters)
                    .CannotBeNullOrWhitespace();
        }
    }
}
