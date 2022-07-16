using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Web.Blazor
{
    /// <summary>
    /// Contains static values for Balzor servers apps.
    /// </summary>
    public static class BlazorConstants
    {
        /// <summary>
        /// Constants related to forms.
        /// </summary>
        public static class Form
        {
            /// <summary>
            /// The fake field name used to display global validation error messages that aren't tied to a field.
            /// </summary>
            public const string GlobalFieldName = "__Global__";
        }
        /// <summary>
        /// Constants related to the web app configuration.
        /// </summary>
        public static class Config
        {
            /// <summary>
            /// Constants related the the jwt configuration.
            /// </summary>
            public static class Jwt
            {
                /// <summary>
                /// The jwt section name.
                /// </summary>
                public const string Section = nameof(Jwt);

                /// <summary>
                /// Config key pointing to the jwt issues.
                /// </summary>
                public const string Issuer = nameof(Issuer);
                /// <summary>
                /// Config key pointing to the jwt audience.
                /// </summary>
                public const string Audience = nameof(Audience);
                /// <summary>
                /// Config key pointing to the secret used to create the jwt token.
                /// </summary>
                public const string Secret = nameof(Secret);
            }
        }
    }
}
