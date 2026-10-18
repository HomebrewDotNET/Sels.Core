using Sels.Core.Progress;

namespace Sels.Core.Legacy.Tests.Progress
{
    /// <summary>Contract tests for delegate-based interval progress.</summary>
    [Trait("Category", "Unit")]
    public class DelegateIntervalProgressTests
    {
        /// <summary>Zero interval reports every value to the supplied action.</summary>
        [Fact]
        public void Report_ZeroInterval_InvokesActionWithValue()
        {
            var reports = new List<int>();
            var sut = new DelegateIntervalProgress<int>(TimeSpan.Zero, reports.Add);

            sut.Report(3);
            sut.Report(5);

            Assert.Equal(new[] { 3, 5 }, reports);
            Assert.Equal(5, sut.LastReported);
        }

        /// <summary>A null reporting action is rejected with its parameter name.</summary>
        [Fact]
        public void Constructor_NullReportAction_ThrowsArgumentNullException()
        {
            Action<int>? report = null;

            var exception = Assert.Throws<ArgumentNullException>(() => new DelegateIntervalProgress<int>(TimeSpan.Zero, report!));

            Assert.Equal("reportAction", exception.ParamName);
        }
    }
}