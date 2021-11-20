using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.ObjectValidationFramework.PerformanceTool.Entities.Simple
{
    public class Car
    {
        public Car()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public DateTime ProductionDate { get; set; }
    }
}
