using Sels.Core.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Excel.Export
{
    public class ExcelExportResourceBundle : IEnumerable<(object Identifier, object Resource)>
    {
        // Properties
        public Dictionary<object, object> Resources { get; set; }
        public List<object> AnonymousResources { get; set; }

        public ExcelExportResourceBundle()
        {

        }

        public ExcelExportResourceBundle Add(object resource)
        {
            resource.ValidateVariable(nameof(resource));

            return this;
        }

        public IEnumerator<(object Identifier, object Resource)> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
