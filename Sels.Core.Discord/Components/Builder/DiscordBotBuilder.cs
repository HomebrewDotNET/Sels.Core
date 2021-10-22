using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Sels.Core.Discord.Components.Bot;
using Sels.Core.Discord.Contracts.Bot;
using Sels.Core.Discord.Contracts.Builder;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Execution;
using Sels.Core.Extensions.Linq;
using Sels.Core.Extensions.Logging.Advanced;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sels.Core.Discord.Components.Builder
{
    /// <summary>
    /// Builder that creates new instances of <see cref="DiscordBot"/>.
    /// </summary>
    public class DiscordBotBuilder : IDiscordBotMessageHandlerBuilder, IDiscordBotBuilder
    {
        // Fields
        private readonly List<Action<DiscordBot>> _builderActions = new List<Action<DiscordBot>>();
        private readonly IEnumerable<ILogger> _loggers;

        public DiscordBotBuilder(IEnumerable<ILogger> loggers)
        {
            _loggers = loggers;
        }

        public DiscordBotBuilder(ILogger logger) : this(logger.AsArrayOrDefault())
        {

        }

        #region Logging
        public IDiscordBotBuilder AddLogger(ILogger logger)
        {
            using var disposable = _loggers.TraceMethod(this);

            if (logger.HasValue())
            {
                AddBuilderAction(x => x.Loggers.Add(logger));
            }

            return this;
        }

        public IDiscordBotBuilder AddLoggers(IEnumerable<ILogger> loggers)
        {
            using var disposable = _loggers.TraceMethod(this);

            loggers.Execute(x => AddLogger(x));

            return this;
        }

        #endregion

        #region Building
        public IDiscordBot Build(string name, string token)
        {
            using var disposable = _loggers.TraceMethod(this);

            _loggers.Debug($"Building Discord bot with name <{name}>");
            var bot = new DiscordBot(name, token);
            Build(bot);
            return bot;
        }

        public IDiscordBot Build(string name, FileInfo tokenFile)
        {
            using var disposable = _loggers.TraceMethod(this);

            _loggers.Debug($"Building Discord bot with name <{name}>");
            var bot = new DiscordBot(name, tokenFile);
            Build(bot);
            return bot;
        }

        public void Clear()
        {
            using var disposable = _loggers.TraceMethod(this);

            _builderActions.Clear();
        }
        #endregion

        #region Message Handler
        public IDiscordBotMessageHandlerBuilder OnMessageReceived(Delegates.Async.AsyncAction<IDiscordBot, SocketMessage> action)
        {
            using var disposable = _loggers.TraceMethod(this);

            action.ValidateArgument(nameof(action));
            _loggers.Debug($"Creating new message handler build action");

            AddBuilderAction(bot => bot.MessageHandlers.Add(new DiscordBotMessageHandler(bot, bot.Loggers)));

            return this;
        }
        #endregion

        private void Build(DiscordBot bot)
        {
            using var disposable = _loggers.TraceMethod(this);

            bot.ValidateArgument(nameof(bot));

            foreach (var action in _builderActions)
            {
                action(bot);
            }
        }
        private void AddBuilderAction(Action<DiscordBot> builderAction)
        {
            using var disposable = _loggers.TraceMethod(this);

            builderAction.ValidateArgument(nameof(builderAction));

            _builderActions.Add(builderAction);
        }
    }
}
