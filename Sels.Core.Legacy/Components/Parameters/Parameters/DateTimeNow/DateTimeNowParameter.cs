using System;

namespace Sels.Core.Components.Parameters.Parameters
{
    public class DateTimeNowParameter : Parameter
    {
        // Constants
        public const string ParameterName = "DateTimeNow";

        public DateTimeNowParameter() : base(ParameterName)
        {

        }

        public override object BeginScope()
        {
            return null;
        }

        protected override string GenerateValue(object scope)
        {
            return DateTime.Now.ToString();
        }

        protected override string GenerateValue(object scope, string argument)
        {
            return DateTime.Now.ToString(argument);
        }
    }
}
