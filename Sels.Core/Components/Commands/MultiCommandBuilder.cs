using Sels.Core.Contracts.Commands;
using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Sels.Core.Components.Commands
{
    /// <summary>
    /// Builder for <see cref="IMultiCommandStartSetup{TChain}"/>, <see cref="IMultiCommandSetup{TChain}"/> and <see cref="IMultiCommandChain{TChain}"/>
    /// </summary>
    /// <typeparam name="TChain"></typeparam>
    public class MultiCommandBuilder<TChain> : IMultiCommandStartSetup<TChain>, IMultiCommandSetup<TChain>, IMultiCommandChain<TChain>
    {
        // Fields
        private readonly List<(TChain Chain, ICommand Command)> _intermediateCommands = new List<(TChain Chain, ICommand Command)>();

        // Properties
        /// <inheritdoc/>
        public ICommand StartCommand { get; private set; }
        /// <inheritdoc/>
        public ReadOnlyCollection<(TChain Chain, ICommand Command)> IntermediateCommands => new ReadOnlyCollection<(TChain Chain, ICommand Command)>(_intermediateCommands);
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
}
