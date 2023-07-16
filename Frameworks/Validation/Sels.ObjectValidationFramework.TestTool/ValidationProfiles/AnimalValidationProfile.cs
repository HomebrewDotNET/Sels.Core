using Sels.ObjectValidationFramework.TestTool.Objects;
using Sels.ObjectValidationFramework.Profile;
using Sels.Core.Logging;
using System.Linq;

namespace Sels.ObjectValidationFramework.TestTool.ValidationProfiles
{
    public class AnimalValidationProfile : ValidationProfile<string>
    {
        public AnimalValidationProfile() : base(logger: LoggingServices.Loggers.FirstOrDefault())
        {
            IgnorePropertyFor<Animal>(x => x.Owner);

            CreateValidationFor<Animal>()
                .ForProperty(x => x.Age).ValidIf(x => x.Value > 0, x => $"Must be above 0. Was <{x.Value}>");
        }
    }
}
