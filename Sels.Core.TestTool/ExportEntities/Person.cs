using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.TestTool.ExportEntities
{
    public class Person
    {

        public Person(string firstName, string lastName, int jobId)
        {
            FirstName = firstName;
            LastName = lastName;
            JobId = jobId;
        }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int JobId { get; set; }
    }
}
