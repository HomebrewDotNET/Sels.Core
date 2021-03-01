using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.Performance
{
    public class PerformanceResult<T>
    {
        // Fields
        private List<double> _performanceResults = new List<double>();

        // Properties
        public double Minimum { 
            get {
                return _performanceResults.GetMinimum();
            }
        }
        public double Average { 
            get {
                return _performanceResults.GetAverage();
            } 
        }
        public double Maximum { 
            get{
                return _performanceResults.GetMaximum();
            } 
        }
        public T Identifier { get; }

        internal PerformanceResult(T identifier)
        {
            Identifier = identifier;
        }

        internal void AddResult(double result)
        {
            _performanceResults.Add(result);
        }

        public override string ToString()
        {
            return $"Result from {Identifier}: Minimum: {Minimum}ms, Average: {Average}ms, Maximum: {Maximum}ms, Total Runs: {_performanceResults.Count}";
        }
    }
}
