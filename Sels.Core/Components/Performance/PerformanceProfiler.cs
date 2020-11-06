using Sels.Core.Extensions;
using Sels.Core.Extensions.General.Generic;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.Performance
{
    public class PerformanceProfiler<TProfilerIdentifier, TCaseIdentifier, TCaseData> : IDisposable
    {
        // Fields
        private object _threadLock = new object();
        private bool _isDisposed;

        private Action _profilerSetup;
        private Action _profilerCleanUp;

        private List<PerformanceCase<TCaseIdentifier, TCaseData>> _performanceCases = new List<PerformanceCase<TCaseIdentifier, TCaseData>>();

        // Properties
        public TProfilerIdentifier Identifier { get; }

        public PerformanceProfiler(TProfilerIdentifier identifier, Action profilerSetup, Action profilerCleanup)
        {
            Identifier = identifier;
            _profilerSetup = profilerSetup;
            _profilerCleanUp = profilerCleanup;

            Setup();
        }

        public PerformanceProfiler(TProfilerIdentifier identifier)
        {
            Identifier = identifier;

            Setup();
        }

        public void AddCase(TCaseIdentifier identifier, int numberOfRuns, Action<TCaseData> caseAction)
        {
            _performanceCases.Add(new PerformanceCase<TCaseIdentifier, TCaseData>(identifier, caseAction, numberOfRuns));
        }

        public void AddCase(TCaseIdentifier identifier, int numberOfRuns, Action<TCaseData> caseAction, Func<TCaseData> caseSetup, Action<TCaseData> caseCleanup)
        {
            _performanceCases.Add(new PerformanceCase<TCaseIdentifier, TCaseData>(identifier, caseAction, numberOfRuns, caseSetup, caseCleanup));
        }

        public void AddCase(IPerformanceCase<TCaseIdentifier, TCaseData> performanceCase)
        {
            AddCase(performanceCase.Identifier, performanceCase.NumberOfRuns, performanceCase.CaseAction, performanceCase.CaseSetup, performanceCase.CaseCleanup);
        }

        public IEnumerable<PerformanceResult<TCaseIdentifier>> RunAllCases()
        {
            var results = new List<PerformanceResult<TCaseIdentifier>>();

            if (_performanceCases.HasValue())
            {
                _profilerSetup?.Invoke();

                foreach(var performanceCase in _performanceCases)
                {
                    results.Add(performanceCase.RunCase());
                }

                _profilerCleanUp?.Invoke();
            }

            return results;
        }

        private void Setup()
        {
            _profilerSetup?.Invoke();
        }

        private void Cleanup()
        {
            _profilerCleanUp?.Invoke();
            _performanceCases = null;
        }

        public void Dispose()
        {
            lock (_threadLock)
            {
                if (!_isDisposed)
                {
                    Cleanup();

                    _isDisposed = true;
                }
            }
        }
    }
}
