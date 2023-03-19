using Sels.Core.Command.Contracts.Commands;
using System.Collections.ObjectModel;
using Sels.Core.Extensions;

namespace Sels.Core.Command.Components.Commands
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    /// <summary>
    /// Builder for <see cref="IMultiCommandStartSetup{TChain}"/>, <see cref="IMultiCommandSetup{TChain}"/> and <see cref="IMultiCommandChain{TChain}"/>
    /// </summary>
    /// <typeparam name="TChain">Type of object used that dictates how to link commands</typeparam>
    public class MultiCommandBuilder<TChain> : IMultiCommandStartSetup<TChain>, IMultiCommandSetup<TChain>, IMultiCommandChain<TChain>
    {
        // Fields
        private readonly List<(TChain Chain, ICommand Command)> _intermediateCommands = new();

        // Properties
        /// <inheritdoc/>
        public ICommand StartCommand { get; private set; }
        /// <inheritdoc/>
        public ReadOnlyCollection<(TChain Chain, ICommand Command)> IntermediateCommands => new(_intermediateCommands);
        /// <inheritdoc/>
        public TChain FinalChain { get; private set; }
        /// <inheritdoc/>
        public ICommand FinalCommand { get; private set; }
        /// <inheritdoc/>
        public IMultiCommandSetup<TChain> StartWith(ICommand startCommand)
        {
            startCommand.ValidateArgument(nameof(startCommand));

            StartCommand = startCommand;

            return this;
        }
        /// <inheritdoc/>
        public IMultiCommandSetup<TChain> ContinueWith(TChain chain, ICommand command)
        {
            command.ValidateArgument(nameof(command));

            _intermediateCommands.Add((chain, command));

            return this;
        }
        /// <inheritdoc/>
        public IMultiCommandChain<TChain> EndWith(TChain finalChain, ICommand finalCommand)
        {
            finalCommand.ValidateArgument(nameof(finalCommand));

            FinalChain = finalChain;
            FinalCommand = finalCommand;

            return this;
        }


    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
