using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Models
{
    /// <summary>
    /// Class that represents nothing. Can be used as parameters for generic types to fullfil the constraint.
    /// </summary>
    public class Null
    {
        private Null()
        {
                
        }

        /// <summary>
        /// The singleton instance.
        /// </summary>
        public static Null Value { get; } = new Null();
    }
}
