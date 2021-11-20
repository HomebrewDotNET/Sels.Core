using Sels.Core.Extensions;
using Sels.ObjectValidationFramework.PerformanceTool.Entities.Simple;
using Sels.ObjectValidationFramework.Templates.Profile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sels.Core.Extensions.Linq;

namespace Sels.ObjectValidationFramework.PerformanceTool.Profiles
{
    public class PersonValidationProfile : ValidationProfile<string>
    {
        // Constants
        private const int LegalAgeToOwnCars = 16;
        private const int LegalAgeToMarry = 18;

        public PersonValidationProfile()
        {
            IgnorePropertyForFallthrough<Person>(x => x.Spouse);

            CreateValidationFor<Person>()
                .ForProperty(x => x.FirstName).CannotBeNullOrWhitespace()
                .ForProperty(x => x.LastName).CannotBeNullOrWhitespace()
                .ForProperty(x => x.BirthDate).MustBeInThePast()
                .ForElements(x => x.NickNames).CannotBeNullOrWhitespace()
                .ValidateWhen(x => x.Source.Age < LegalAgeToOwnCars, x =>
                {
                    x.ForProperty(x => x.Cars).MustBeEmpty(x => $"Person[{x.Source.Id}]: People under the age of {LegalAgeToOwnCars} cannot own any cars. Age: <{x.Source.Age}>. Cars Owned: <{x.Value.GetCount()}>");
                })
                .ValidateWhen(x => x.Source.Age < LegalAgeToMarry, x =>
                {
                    x.ForProperty(x => x.Spouse).MustBeNull(x => $"Person[{x.Source.Id}]: People under the age of {LegalAgeToMarry} cannot be married. Age: <{x.Source.Age}>. Married to <{x.Value.FullName}>");
                });


            CreateValidationFor<Car>()
                .ForProperty(x => x.Brand).CannotBeNullOrWhitespace()
                .ForProperty(x => x.Model).CannotBeNullOrWhitespace()
                .ForProperty(x => x.ProductionDate).MustBeInThePast();           
        }
    }
}
