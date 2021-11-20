using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.ObjectValidationFramework.PerformanceTool.Entities.Simple
{
    public static class SimpleObjects
    {
        public static Person ValidAdultPerson { get; set; }
        public static Person Spouse { get; set; }
        public static Car ValidBmw { get; set; }
        public static Car ValidMini { get; set; }
        public static Car ValidPeugeot { get; set; }

        public static Person InvalidAdultPerson { get; set; }
        public static Car InvalidBmw { get; set; }
        public static Car InvalidMini { get; set; }
        public static Car InvalidPeugeot { get; set; }

        static SimpleObjects()
        {
            var birthDate = DateTime.ParseExact("19980104", "yyyyMMdd", null);

            ValidBmw = new Car()
            {
                Brand = "BMW",
                Model = "116d Hatchback",
                ProductionDate = DateTime.Now.AddYears(-1)
            };

            ValidMini = new Car()
            {
                Brand = "Mini Cooper",
                Model = "Basic",
                ProductionDate = DateTime.Now.AddYears(-4)
            };

            ValidPeugeot = new Car()
            {
                Brand = "Peugeot",
                Model = "206",
                ProductionDate = birthDate.AddYears(1)
            };

            ValidAdultPerson = new Person()
            {
                FirstName = "Jens",
                LastName = "Sels",
                NickNames = new[] {"Dragonborn", "Smalle"},
                BirthDate = birthDate,
                Cars = new List<Car>()
                {
                    ValidBmw,
                    ValidMini,
                    ValidPeugeot
                }
            };

            Spouse = new Person()
            {
                FirstName = "Lynn",
                LastName = "Laridon",
                BirthDate = DateTime.ParseExact("19970813", "yyyyMMdd", null),
                Spouse = ValidAdultPerson
            };

            ValidAdultPerson.Spouse = Spouse;

            InvalidBmw = new Car()
            {
                Brand = "BMW",
                Model = "",
                ProductionDate = DateTime.Now.AddYears(-1)
            };

            InvalidMini = new Car()
            {
                Brand = "\t",
                Model = "Basic",
                ProductionDate = DateTime.Now.AddYears(-4)
            };

            InvalidPeugeot = new Car()
            {
                Brand = "Peugeot",
                Model = "206",
                ProductionDate = DateTime.Now.AddYears(5)
            };

            InvalidAdultPerson = new Person()
            {
                FirstName = "    ",
                LastName = "Sels",
                BirthDate = DateTime.Now.AddYears(15),
                NickNames = new[] {"\t", "   ", ""},
                Spouse = Spouse,
                Cars = new List<Car>()
                {
                    InvalidBmw,
                    InvalidMini,
                    InvalidPeugeot
                }
            };
        }
    }
}
