using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.Factory
{
    /// <summary>
    /// Object that maps to a configuration section that contains settings to create objects.
    /// </summary>
    public class ObjectConstructionConfig
    {
        /// <summary>
        /// Factory identifier.
        /// </summary>
        public string Identifier { get; set; }
        /// <summary>
        /// Optional constructor arguments.
        /// </summary>
        public ObjectConstructionArgumentConfig[] Arguments { get; set; }
    }

    /// <summary>
    /// Object that maps to a configuration section that contains settings for constructor arguments.
    /// </summary>
    public class ObjectConstructionArgumentConfig
    {
        /// <summary>
        /// Constructor value.
        /// </summary>
        public string Value { get; set; }
        /// <summary>
        /// Optional type of argument.
        /// </summary>
        public string Type { get; set; } = typeof(string).FullName;
    }
}
