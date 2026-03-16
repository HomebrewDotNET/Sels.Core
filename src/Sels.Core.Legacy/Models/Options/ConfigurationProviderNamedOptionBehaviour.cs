using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Options
{
    /// <summary>
    /// Dictates how a <see cref="OptionConfigurationProvider{TOptions}"/> handles named options when binding from config.
    /// </summary>
    public enum ConfigurationProviderNamedOptionBehaviour
    {
        /// <summary>
        /// Name is ignored and default section name is used.
        /// </summary>
        Ignore = 0,
        /// <summary>
        /// Option name is appended after section name using format {SectionName.OptionsName} (e.g Section name = MyOption, options name = SomeName: Config section binded from will be MyOption.SomeName)
        /// </summary>
        Suffix = 1,
        /// <summary>
        /// Option name is prepended before section name using format {OptionsName.SectionName} (e.g Section name = MyOption, options name = SomeName: Config section binded from will be SomeName.MyOption)
        /// </summary>
        Prefix = 2,
        /// <summary>
        /// Option name will be the sub section name. (e.g Section name = MyOption, options name = SomeName: Config section binded from will be MyOptions->SomeName)
        /// </summary>
        SubSection = 3
    }
}
