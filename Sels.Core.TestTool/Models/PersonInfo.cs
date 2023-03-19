using Sels.Core.Conversion.Attributes.KeyValue;
using System;
using System.Collections.Generic;

namespace Sels.Core.TestTool.Models
{
    public class PersonInfo
    {
        public string Name { get; set; }
        public string FamilyName { get; set; }
        [Key(key: "BirthDate")]
        public DateTime BirthDay { get; set; }
        [Key(key: "Role")]
        public List<string> JobRoles { get; set; }
        public List<double> Earnings { get; set; }
        public bool IsGraduated { get; set; }
    }

    public class PersonInfo<TId> : PersonInfo
    {
        public TId Id { get; set; }
    }
}
