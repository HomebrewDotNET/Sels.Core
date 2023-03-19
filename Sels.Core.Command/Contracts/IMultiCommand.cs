using System.Collections.ObjectModel;

namespace Sels.Core.Command.Contracts.Commands
{
    /// <summary>
    /// Used to start to setup a command chain for a command that consists of multiple commands.
    /// </summary>
    /// <typeparam name="TChain">Type of objects that tells how commands should be chained</typeparam>
    public interface IMultiCommandStartSetup<TChain>
    {
        /// <summary>
        /// Sets <paramref name="startCommand"/> as the first command to be executed.
        /// </summary>
        /// <param name="startCommand">Command to execute first</param>
        /// <returns>Setup object to continue building the command chain or returns the <see cref="IMultiCommandChain{TChain}"/></returns>
        IMultiCommandSetup<TChain> StartWith(ICommand startCommand);
    }

    /// <summary>
    /// Used to setup and build a command chain for a command that consists of multiple commands.
    /// </summary>
    public interface IMultiCommandSetup<TChain>
    {
        /// <summary>
        /// Continues the previous command with <paramref name="command"/>.
        /// </summary>
        /// <param name="chain">How the previous <see cref="ICommand"/> should be chained with <paramref name="command"/></param>
        /// <param name="command">Command to chain</param>
        /// <returns>Object to continue building the chain</returns>
        IMultiCommandSetup<TChain> ContinueWith(TChain chain, ICommand command);

        /// <summary>
        /// Finished the command chain with <paramref name="finalCommand"/> and returns the full command chain.
        /// </summary>
        /// <param name="finalChain">How the previous <see cref="ICommand"/> should be chained with <paramref name="finalCommand"/></param>
        /// <param name="finalCommand">Final command in the chain that will be executed</param>
        /// <returns>The configured command chain</returns>
        IMultiCommandChain<TChain> EndWith(TChain finalChain, ICommand finalCommand);
    }

    /// <summary>
    /// Represents the order in which command are executed for a command that consists of multiple commands.
    /// </summary>
    public interface IMultiCommandChain<TChain>
    {
        /// <summary>
        /// First command in the chain that will be executed first.
        /// </summary>
        ICommand StartCommand { get; }
        /// <summary>
        /// List of ordered commands that will be executed in order after <see cref="StartCommand"/>.
        /// </summary>
        ReadOnlyCollection<(TChain Chain, ICommand Command)> IntermediateCommands { get; }
        /// <summary>
        /// How <see cref="StartCommand"/> or the last command in <see cref="IntermediateCommands"/> should be linked to <see cref="FinalCommand"/>.
        /// </summary>
        TChain FinalChain { get; }
        /// <summary>
        /// Final command in the chain that will be executed.
        /// </summary>
        ICommand FinalCommand { get; }
    }
}
