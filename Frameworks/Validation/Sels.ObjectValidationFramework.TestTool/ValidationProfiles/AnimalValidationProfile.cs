using Sels.ObjectValidationFramework.TestTool.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using Sels.ObjectValidationFramework.Templates.Profile;

namespace Sels.ObjectValidationFramework.TestTool.ValidationProfiles
{
    public class AnimalValidationProfile : ValidationProfile<string>
    {
        public AnimalValidationProfile()
        {
            IgnorePropertyForFallthrough<Animal>(x => x.Owner);

            CreateValidationFor<Animal>()
                .ForProperty(x => x.Age).ValidIf(x => x.Value > 0, x => $"{x.GetFullDisplayName()} must be above 0. Was <{x.Value}>");
        }
    }
}
