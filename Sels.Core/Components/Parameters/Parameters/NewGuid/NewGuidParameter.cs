using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.Parameters.Parameters
{
    public class NewGuidParameter : CachedParameter
    {
        // Constants
        public const string ParameterName = "NewGuid";

        public NewGuidParameter() : base(ParameterName)
        {

        }

        protected override string GenerateValue()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
