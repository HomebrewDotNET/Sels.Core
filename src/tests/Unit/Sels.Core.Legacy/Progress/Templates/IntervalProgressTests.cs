using Sels.Core.Progress.Templates;

namespace Sels.Core.Legacy.Tests.Progress.Templates
{
    /// <summary>Contract tests for interval progress timing.</summary>
    [Trait("Category", "Unit")]
    public class IntervalProgressTests
    {
        /// <summary>Zero interval delivers each report and updates LastReported.</summary>
        [Fact]
        public void Report_ZeroInterval_DeliversEveryValue()
        {
            var sut = new ProgressProbe(TimeSpan.Zero);

            sut.Report(4);
            sut.Report(7);

            Assert.Equal(new[] { 4, 7 }, sut.Values);
            Assert.Equal(7, sut.LastReported);
        }

        /// <summary>A stopped stopwatch suppresses delivery but still updates LastReported.</summary>
        [Fact]
        public void Report_IntervalNotElapsed_UpdatesValueWithoutDelivery()
        {
            var sut = new ProgressProbe(TimeSpan.FromHours(1));
            sut.StopClock();

            sut.Report(9);

            Assert.Empty(sut.Values);
            Assert.Equal(9, sut.LastReported);
        }

        /// <summary>Negative intervals are rejected by the approved argument-guard migration.</summary>
        [Fact]
        public void Constructor_NegativeInterval_ThrowsArgumentException()
        {
            var exception = Assert.Throws<ArgumentException>(() => new ProgressProbe(TimeSpan.FromTicks(-1)));

            Assert.Contains("interval", exception.Message);
        }

        private sealed class ProgressProbe : IntervalProgress<int>
        {
            public List<int> Values { get; } = new();

            public ProgressProbe(TimeSpan interval) : base(interval)
            {
            }

            public void StopClock() => _stopwatch.Stop();

            protected override void OnReport(int value) => Values.Add(value);
        }
    }
}