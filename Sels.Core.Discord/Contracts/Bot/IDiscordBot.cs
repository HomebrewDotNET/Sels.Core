using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Discord.Contracts.Bot
{
    /// <summary>
    /// Represents a Discord bot with extended functionality.
    /// </summary>
    public interface IDiscordBot : IDisposable
    {
        // Properties
        /// <summary>
        /// Name of this discord bot.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Socket client used by this bot. Initialized using <see cref="Initialize"/>.
        /// </summary>
        public DiscordSocketClient Client { get; }

        /// <summary>
        /// Initializes the bot using the internal settings. 
        /// </summary>
        Task Initialize();
    }
}
