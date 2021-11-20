using System;
using System.Collections.Generic;
using System.Text;
using Discord.WebSocket;
using Sels.Core.Discord.Contracts.Bot;

namespace Sels.Core.Discord.Contracts.Builder
{
    /// <summary>
    /// Builder for setting up a handler when a <see cref="IDiscordBot"/> receives a message.
    /// </summary>
    /// <typeparam name="TMessage">Type of message to handle</typeparam>
    public interface IDiscordBotMessageHandlerBuilder<TMessage> : IDiscordBotBuilder where TMessage : SocketMessage
    {

    }
}
