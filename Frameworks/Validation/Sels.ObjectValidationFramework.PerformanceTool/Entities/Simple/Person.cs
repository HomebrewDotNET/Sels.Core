using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.ObjectValidationFramework.PerformanceTool.Entities.Simple
{
    public class Person
    {
        public Person()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        public string[] NickNames { get; set; }
        public DateTime BirthDate { get; set; }
        public int Age => BirthDate.GetYearDifference();
        public Person Spouse { get; set; }

        public ICollection<Car> Cars { get; set; }
    }
}
