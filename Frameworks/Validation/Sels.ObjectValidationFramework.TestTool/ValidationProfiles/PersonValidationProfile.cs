using Sels.Core.Extensions;
using Sels.ObjectValidationFramework.TestTool.Objects;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Sels.ObjectValidationFramework.Templates.Profile;

namespace Sels.ObjectValidationFramework.TestTool.ValidationProfiles
{
    public class PersonValidationProfile : ValidationProfile<string>
    {
        public PersonValidationProfile() : base()
        {
            IgnorePropertyForFallthrough<Person>(x => x.Parent);

            CreateValidationFor<Person>()
                .ForProperty(x => x.FirstName).ValidIf(x => x.Value.HasValue(), x => $"{x.GetFullDisplayName()} cannot be null or whitespace. Was <{x.Value}>")
                .ForProperty(x => x.LastName).ValidIf(x => x.Value.HasValue(), x => $"{x.GetFullDisplayName()} cannot be null or whitespace. Was <{x.Value}>")
                .ForProperty(x => x.Parent).InvalidIf(x => x.Value == null, x => $"{x.GetFullDisplayName()} cannot be null")
                .ForProperty(x => x.Gender).InvalidIf(x => x.Value == Gender.Null, x => $"{x.GetFullDisplayName()} cannot be Null. Was <{x.Value}>")
                .ForProperty(x => x.Owner).InvalidIf(x => x.Value != null, x => $"{x.GetFullDisplayName()} must be null.")
                .ValidateWhen(x => x.Source.Gender == Gender.Male, x =>
                {
                    x.ForProperty(x => x.NickName).InvalidIf(x => !x.Value.HasValue(), x => $"{x.GetFullDisplayName()} cannot be null or whitespace. Was <{x.Value}>");
                });

            ImportFrom<AnimalValidationProfile>();
        }
    }
}
