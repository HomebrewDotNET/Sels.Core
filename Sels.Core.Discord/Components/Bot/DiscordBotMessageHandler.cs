using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Sels.Core.Discord.Contracts.Bot;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Logging;
using Sels.Core.Extensions.Logging.Advanced;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Sels.Core.Delegates.Async;

namespace Sels.Core.Discord.Components.Bot
{
    /// <summary>
    /// Handles messages received by a DiscordBot.
    /// </summary>
    /// <typeparam name="TMessage">Type of message to handle</typeparam>
    public class DiscordBotMessageHandler<TMessage> : IDisposable where TMessage : SocketMessage
    {
        // Fields
        private readonly IDiscordBot _parent;
        private readonly IEnumerable<ILogger> _loggers;

        // Properties
        /// <summary>
        /// Optional list of conditions that a <see cref="SocketMessage"/> must pass before executing <see cref="Handlers"/>.
        /// </summary>
        public List<AsyncCondition<IDiscordBot, TMessage>> Conditions { get; } = new List<AsyncCondition<IDiscordBot, TMessage>>();
        /// <summary>
        /// List of actions to execute when a DiscordBot receives a message.
        /// </summary>
        public List<AsyncAction<IDiscordBot, TMessage>> Handlers { get; } = new List<AsyncAction<IDiscordBot, TMessage>>();

        /// <summary>
        /// Handles messages received by a DiscordBot.
        /// </summary>
        /// <param name="discordBot">Discord bot that can be used to reply to messages</param>
        /// <param name="loggers">Optional loggers that allows this handler to log</param>
        public DiscordBotMessageHandler(IDiscordBot discordBot, IEnumerable<ILogger> loggers = null)
        {
            _parent = discordBot.ValidateArgument(nameof(discordBot));
            _loggers = loggers;

            using (_loggers.TraceAction(LogLevel.Debug, $"Register message handler for <{discordBot.Name}>"))
            {
                discordBot.Client.MessageReceived += HandleMessage;
            }
        }

        private async Task HandleMessage(SocketMessage message)
        {
            using (_loggers.TraceMethod(this))
            {
                try
                {
                    var typedMessage = message as TMessage;

                    // Only execute when message is of type TMessage
                    if (typedMessage == null) return;                    

                    // If any conditions fails we don't execute any handlers.
                    if (await Conditions.AnyAsync(async x => !await x(_parent, typedMessage))) return;

                    // Execute handlers
                    Handlers.ExecuteAsync(x => x(_parent, typedMessage));
                }
                catch(Exception ex)
                {
                    _loggers.Log($"Message handler ran into an issue when executing internal handlers", ex);
                }
            }
        }

        public void Dispose()
        {
            using (_loggers.TraceMethod(this))
            {
                using (_loggers.TraceAction(LogLevel.Debug, $"Unregister message handler for <{_parent.Name}>"))
                {
                    _parent.Client.MessageReceived -= HandleMessage;
                }
            }
        }
    }
}
