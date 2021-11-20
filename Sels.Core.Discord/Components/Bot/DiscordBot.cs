using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Sels.Core.Discord.Contracts.Bot;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Logging.Advanced;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Discord.Components.Bot
{
    /// <summary>
    /// Object that acts as a wrapper for a Discord bot client.
    /// </summary>
    public class DiscordBot : IDiscordBot
    {
        // Fields
        private readonly string _token;

        // Properties
        public string Name { get; }
        public DiscordSocketClient Client { get; }

        /// <summary>
        /// Loggers that allows this discord bot to log meessages.
        /// </summary>
        public List<ILogger> Loggers { get; } = new List<ILogger>();

        #region Handlers
        /// <summary>
        /// Handlers that handles any messages received by the bot.
        /// </summary>
        //public List<DiscordBotMessageHandler> MessageHandlers { get; } = new List<DiscordBotMessageHandler>();
        #endregion

        /// <summary>
        /// Object that acts as a wrapper for a Discord bot client.
        /// </summary>
        /// <param name="name">Display name for this discord bot</param>
        /// <param name="tokenFile">File containing the token used to authenticate with Discord</param>
        public DiscordBot(string name, FileInfo tokenFile) : this(name, tokenFile.ValidateArgumentExists(nameof(tokenFile)).Read().Trim())
        {

        }

        /// <summary>
        /// Object that acts as a wrapper for a Discord bot client.
        /// </summary>
        /// <param name="name">Display name for this discord bot</param>
        /// <param name="token">Token used to authenticate with Discord</param>
        public DiscordBot(string name, string token)
        {
            Name = name.ValidateArgumentNotNullOrWhitespace(nameof(name));
            _token = token.ValidateArgumentNotNullOrWhitespace(nameof(token));

            Client = new DiscordSocketClient();

            Client.Log += Log;
        }

        public async Task Initialize()
        {
            using (Loggers.TraceMethod(this))
            {
                using(Loggers.TraceAction($"Login and start Discord client for <{Name}>"))
                {
                    await Client.LoginAsync(TokenType.Bot, _token);
                    await Client.StartAsync();
                }
            }
        }

        private Task Log(LogMessage message)
        {
            Loggers.Log($"{Name}: {message}");

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            using (Loggers.TraceMethod(this))
            {
                using (Loggers.TraceAction(LogLevel.Debug, $"Dispose Discord client for <{Name}>"))
                {
                    Client.Dispose();
                }
            }
        }
    }
}
