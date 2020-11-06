using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.Performance
{
    public interface IPerformanceCase<TTCaseIdentifier, TCaseData>
    {
        TTCaseIdentifier Identifier { get; }
        Func<TCaseData> CaseSetup { get; }
        Action<TCaseData> CaseAction { get; }
        Action<TCaseData> CaseCleanup { get; }
        int NumberOfRuns { get; }
    }
}
