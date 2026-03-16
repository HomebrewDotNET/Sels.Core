using Sels.Core.Extensions;
using Sels.ObjectValidationFramework.TestTool.Objects;
using Sels.ObjectValidationFramework.Profile;
using Sels.Core.Logging;
using System.Linq;

namespace Sels.ObjectValidationFramework.TestTool.ValidationProfiles
{
    public class PersonValidationProfile : ValidationProfile<string>
    {
        public PersonValidationProfile() : base(logger: LoggingServices.Loggers.FirstOrDefault())
        {
            IgnorePropertyFor<Person>(x => x.Parent);

            CreateValidationFor<Person>()
                .ForProperty(x => x.FirstName)
                    .ValidIf(x => x.Value.HasValue(), x => $"Cannot be null or whitespace. Was <{x.Value}>")
                .ForProperty(x => x.LastName)
                    .ValidIf(x => x.Value.HasValue(), x => $"Cannot be null or whitespace. Was <{x.Value}>")
                .Switch(x => x.Gender.Value, x => x.Source.Gender.HasValue)
                    .Case(Gender.Male)
                    .Case(Gender.Other)
                    .Then(b =>
                    {
                        b.ForProperty(x => x.NickName)
                            .CannotBeNullOrWhitespace();
                    })
                    .Default(b =>
                    {
                        b.ForProperty(x => x.NickName)
                            .MustBeNull();
                    })
                .ForProperty(x => x.Parent)
                    .InvalidIf(x => x.Value == null, x => $"Cannot be null")
                        // Only validate root person
                        .When(x => !x.Parents.HasValue())
                .ForProperty(x => x.Owner)
                    .InvalidIf(x => x.Value != null, x => $"Must be null");

            ImportFrom<AnimalValidationProfile>();
        }
    }
}
