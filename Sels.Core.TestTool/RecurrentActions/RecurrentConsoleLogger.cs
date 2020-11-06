using Sels.Core.Components.Caching;
using Sels.Core.Components.Console;
using Sels.Core.Components.Random;
using Sels.Core.Components.RecurrentAction;
using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Timers;
using Sels.Core.Extensions.Object.ItemContainer;
using Sels.Core.Extensions.Object.String;

namespace Sels.Core.TestTool.RecurrentActions
{
    public class RecurrentConsoleLogger : IRecurrentAction<string>
    {
        private int _workSpeed;
        private int _workLoad;

        private static readonly InMemoryCache<string> _cache = new InMemoryCache<string>();

        // Cache Keys
        private const string MessagesKey = "Messages";
        private const string ModsKey = "Mods";
        private const string ModMessagesKey = "ModMessages";

        // Cache Contents
        private readonly List<string> _messages = new List<string>() { 
            "{0} is modding some stuff",
            "{0} is doing some stuff",
            "{0} is preparing to destroy the world",
            "{0} is looking at the stars",
            "{0} is wondering if monstrous size has intrinsic merit",
            "{0} got too overconfident",
            "{0} is staring into the abyss",
            "{0} is praising the sun"
        };

        private readonly List<string> _modMessages = new List<string>() {
            "{0} is working on {1}",
            "{0} is wondering when {1} is going to release",
            "{0} decided to work on {1} for a bit",
            "{0} is wondering if he should work on {1} or play instead"
        };

        private readonly List<string> _mods = new List<string>() {
            "Monster Variety Project",
            "Banner Bearer",
            "Banished Hunter",
            "Hollow Knight Trinkets",
            "Chimera",
            "M.E.P.E"
        };

        #region Interface
        public string Identifier { get; }

        public int DownTime { get; }

        public Action<string, CancellationToken> EntryMethod => (x, y) => DoStuff(x, y);

        public RecurrentActionDelegates.RecurrentActionExceptionHandler<string> ExceptionHandler => ConsoleExceptionHandler;

        public RecurrentActionDelegates.RecurrentActionElapsedHandler<string> ElaspedHandler => ConsoleElapsedHandler;
        #endregion

        public RecurrentConsoleLogger(string identifier, int workSpeed, int downtime, int workLoad)
        {
            Identifier = identifier;
            _workSpeed = workSpeed;
            DownTime = downtime;
            _workLoad = workLoad;

            _cache.TryAdd(MessagesKey, _messages);
            _cache.TryAdd(ModsKey, _mods);
            _cache.TryAdd(ModMessagesKey, _modMessages);
        }

        public void DoStuff(string identifier, CancellationToken token)
        {
            for(int i = 0; i < _workLoad; i++)
            {
                if (token.IsCancellationRequested)
                {
                    Console.WriteLine($"{identifier} was asked to stop doing stuff");
                    break;
                }
                else
                {
                    switch (RandomGenerator.GetRandomNumber(3))
                    {
                        case 0:
                            var modToWorkOn = _cache.Get<List<string>>(ModsKey).GetRandomItem();
                            ConsoleHelper.WriteLine(ConsoleColor.Yellow ,_cache.Get<List<string>>(ModMessagesKey).GetRandomItem().FormatString(identifier, modToWorkOn));
                            break;
                        default:
                            Console.WriteLine(_cache.Get<List<string>>(MessagesKey).GetRandomItem().FormatString(identifier));
                            break;
                    }

                    Thread.Sleep(_workSpeed);
                }            
            }

            ConsoleHelper.WriteLine(ConsoleColor.Red, $"{identifier} is done doing stuff");
        }

        public void ConsoleExceptionHandler(string identifier, string sourceMethod, Exception exception)
        {
            Console.WriteLine($"Oh no something went wrong with {identifier} while executing {sourceMethod}: {Environment.NewLine + exception.ToString()}");
        }

        public void ConsoleElapsedHandler(string identifier, object sender, ElapsedEventArgs args)
        {
            ConsoleHelper.WriteLine(ConsoleColor.Cyan ,$"{identifier} is starting to do stuff at {args.SignalTime}");
        }
    }
}
