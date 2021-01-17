using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.TestTool.ExportEntities
{
    public class Job
    {
        public Job(int id, string title)
        {
            Id = id;
            Title = title;
        }

        public int Id { get; set; }
        public string Title { get; set; }
    }
}
