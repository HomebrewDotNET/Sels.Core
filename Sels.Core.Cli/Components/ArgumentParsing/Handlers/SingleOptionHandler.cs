using Sels.Core.Cli.Templates.ArgumentParsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sels.Core.Extensions.Linq;

namespace Sels.Core.Cli.ArgumentParsing.Handlers
{
    internal class SingleOptionHandler<T, TArg> : BaseOptionHandler<T, TArg>
    {
        // Fields
        private readonly string? _format;
        private readonly bool _duplicatesAllowed;

        /// <inheritdoc cref="SingleOptionHandler{T, TArg}"/>.
        /// <param name="format">The format for the option and value. If left to null the next arg after an option will be used as the value</param>
        /// <param name="duplicatesAllowed">If the option is allowed to be defined multiple times</param>
        /// <param name="parser">The parser using to create this handler</param>
        /// <param name="optionPrefix">The prefix to use for the short option</param>
        /// <param name="longOptionPrefix">The prefix to use for the full option</param>
        /// <param name="option">The option name</param>
        /// <param name="longOption">The full option name</param>
        public SingleOptionHandler(string? format, bool duplicatesAllowed, ArgumentParser<T> parser, char optionPrefix, string longOptionPrefix, char? option, string? longOption) : base(parser, optionPrefix, longOptionPrefix, option, longOption)
        {
            _format = format;
            _duplicatesAllowed = duplicatesAllowed;
        }

        /// <inheritdoc/>
        public override bool TryParseArguments(string[] args, IResultBuilder builder, out TArg? parsed, out string[] modifiedArgs)
        {
            using (_loggers.TraceMethod(this))
            {
                parsed = default;
                modifiedArgs = args;

                _loggers.Log($"{DisplayName} parsing <{args.Length}> arguments");

                // Look for indexes of args matching the option
                List<int> optionIndexes = new List<int>();
                args.Execute((i, arg) =>
                {
                    if(arg.StartsWith(FullOption) || arg.StartsWith(FullLongOption)) optionIndexes.Add(i);
                });

                throw new NotImplementedException();
            }
        }       
    }
}
