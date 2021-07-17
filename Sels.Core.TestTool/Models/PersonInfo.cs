using Sels.Core.Components.Serialization.KeyValue.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.TestTool.Models
{
    public class PersonInfo
    {
        public string Name { get; set; }
        public string FamilyName { get; set; }
        [KeyValueProperty(key: "BirthDate")]
        public DateTime BirthDay { get; set; }
        [KeyValueCollection(key: "Role")]
        public List<string> JobRoles { get; set; }
        public List<double> Earnings { get; set; }
        public bool IsGraduated { get; set; }
    }
}
