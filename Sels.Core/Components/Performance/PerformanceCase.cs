using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Sels.Core.Components.Performance
{
    public class PerformanceCase<TCaseIdentifier, TCaseData>
    {
        // Fields
        private Func<TCaseData> _caseSetup;
        private Action<TCaseData> _caseAction;
        private Action<TCaseData> _caseCleanUp;

        // Properties
        public TCaseIdentifier Identifier { get; }
        public int NumberOfRuns { get; }

        internal PerformanceCase(TCaseIdentifier identifier, Action<TCaseData> caseAction, int numberOfRuns, Func<TCaseData> caseSetup, Action<TCaseData> caseCleanup)
        {
            Identifier = identifier;
            _caseAction = caseAction;
            NumberOfRuns = numberOfRuns;
            _caseSetup = caseSetup;
            _caseCleanUp = caseCleanup;
        }

        internal PerformanceCase(TCaseIdentifier identifier, Action<TCaseData> caseAction, int numberOfRuns)
        {
            Identifier = identifier;
            _caseAction = caseAction;
            NumberOfRuns = numberOfRuns;
        }

        internal PerformanceResult<TCaseIdentifier> RunCase()
        {
            var result = new PerformanceResult<TCaseIdentifier>(Identifier);

            var caseCounter = 0;

            while(caseCounter < NumberOfRuns)
            {
                var caseData = _caseSetup != null ? _caseSetup.Invoke() : default;

                var timer = new Stopwatch();

                timer.Start();
                _caseAction(caseData);
                timer.Stop();

                _caseCleanUp?.Invoke(caseData);

                result.AddResult(timer.ElapsedMilliseconds);

                caseCounter++;
            }
                
            return result;
        }
    }
}
