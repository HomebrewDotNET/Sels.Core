using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core
{
    public static class Constants
    {
        #region String
        public static class Strings
        {
            public const string Space = " ";
            public const string Tab = "\t";
            public const string Comma = ",";
        }
        #endregion

        public static class Config
        {
            public const string DefaultAppSettingsFile = "AppSettings.json";

            public static class Sections
            {
                public const string DefaultObjectAliases = "ObjectAliases";

                public const string AppSettings = "AppSettings";
                public const string ConnectionStrings = "ConnectionStrings";
            }
        }

    }
}
