using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Sels.Core.Discord.Contracts.Bot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static Sels.Core.Delegates.Async;

namespace Sels.Core.Discord.Contracts.Builder
{
    /// <summary>
    /// Builder that exposes methods for building a <see cref="IDiscordBot"/> using a fluent syntax.
    /// </summary>
    public interface IDiscordBotBuilder
    {
        #region Logging
        /// <summary>
        /// Adds a logger that allows the Discord bot to log.
        /// </summary>
        /// <param name="logger">Logger to add to the bot</param>
        /// <returns>Current builder</returns>
        IDiscordBotBuilder AddLogger(ILogger logger);
        /// <summary>
        /// Adds loggers that allow the Discord bot to log.
        /// </summary>
        /// <param name="loggers">Loggers to add to the bot</param>
        /// <returns>Current builder</returns>
        IDiscordBotBuilder AddLoggers(IEnumerable<ILogger> loggers);
        #endregion

        #region Handlers
        /// <summary>
        /// Creates a new builder for creating a message handler. Handler is executes when a <see cref="IDiscordBot"/> received a message.
        /// </summary>
        /// <typeparam name="TMessage">Type of message to handle</typeparam>
        /// <param name="action">Action to execute</param>
        /// <returns>Builder for configuring the message handler</returns>
        IDiscordBotMessageHandlerBuilder<TMessage> OnMessageReceived<TMessage>(AsyncAction<IDiscordBot, TMessage> action) where TMessage : SocketMessage;
        #endregion

        #region Build
        /// <summary>
        /// Builds a new <see cref="IDiscordBot"/> using the settings configured on this builder.
        /// </summary>
        /// <param name="name">Display name for the discord bot</param>
        /// <param name="token">Discord token for authenticating the newly created Discord bot</param>
        /// <returns>A new instance of <see cref="IDiscordBot"/></returns>
        IDiscordBot Build(string name, string token);

        /// <summary>
        /// Builds a new <see cref="IDiscordBot"/> using the settings configured on this builder.
        /// </summary>
        /// <param name="name">Display name for the discord bot</param>
        /// <param name="tokenFile">File containing the Discrod token for authenticating the newly created Discord bot</param>
        /// <returns>A new instance of <see cref="IDiscordBot"/></returns>
        IDiscordBot Build(string name, FileInfo tokenFile);

        /// <summary>
        /// Clears any configured settings in this builder so it can be configured from scratch again. 
        /// </summary>
        void Clear();
        #endregion


    }
}
