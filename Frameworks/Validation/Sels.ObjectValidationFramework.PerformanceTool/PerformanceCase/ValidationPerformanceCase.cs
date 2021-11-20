using Sels.Core.Components.Performance;
using Sels.Core.Extensions;
using Sels.ObjectValidationFramework.Templates.Profile;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace Sels.ObjectValidationFramework.PerformanceTool.PerformanceCase
{
    public class ValidationPerformanceCase<TObject> : IPerformanceCase<string, string>
    {
        // Fields
        private bool _verbose = true;

        // Properties
        public string Identifier { get; }

        public Func<string> CaseSetup => null;

        public Action<string> CaseAction { get; }

        public Action<string> CaseCleanup { get; }

        public int NumberOfRuns { get; }

        // State
        private IEnumerable<string> errors;

        public ValidationPerformanceCase(string identifier, int numberOfRuns, TObject objectToValidate, ValidationProfile<string> profile, bool verbose = true)
        {
            Identifier = identifier;
            NumberOfRuns = numberOfRuns;
            _verbose = verbose;

            CaseAction = x => {
                errors = profile.Validate(objectToValidate);
            };

            CaseCleanup = x =>
            {
                if(_verbose) Console.WriteLine($"Errors: {(errors.HasValue() ? errors.JoinStringNewLine() : "None")}");
            };
        }
    }
}
