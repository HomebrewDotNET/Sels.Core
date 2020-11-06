using System;
using System.Collections.Generic;
using System.Text;
using SystemConsole = System.Console;

namespace Sels.Core.Components.Console
{
    public static class ConsoleHelper
    {
        private const ConsoleColor _defaultForegroundColor = ConsoleColor.Gray;
        private const ConsoleColor _defaultBackgroundColor = ConsoleColor.Black;

        private static object _threadlock = new object();

        public static void Run(Action entryMethod)
        {
            try
            {
                entryMethod();
            }
            catch(Exception ex)
            {
                SystemConsole.WriteLine($"Something went wrong while execuring console app: {Environment.NewLine + ex.ToString()}");
            }
            finally
            {
                SystemConsole.WriteLine("Press any key to close");
                SystemConsole.ReadKey();
            }
        }

        public static void Run(Action entryMethod, EventHandler exitHandler)
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(exitHandler);

            Run(entryMethod);
        }

        public static void WriteLine(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string message)
        {
            lock (_threadlock)
            {
                SystemConsole.ForegroundColor = foregroundColor;
                SystemConsole.BackgroundColor = backgroundColor;

                SystemConsole.WriteLine(message);

                ResetColors();
            }    
        }

        public static void WriteLine(ConsoleColor foregroundColor, string message)
        {
            WriteLine(foregroundColor, _defaultBackgroundColor, message);
        }

        private static void ResetColors()
        {
            SystemConsole.ForegroundColor = _defaultForegroundColor;
            SystemConsole.BackgroundColor = _defaultBackgroundColor;
        }

        public static void WriteLine(ConsoleColor yellow, object p)
        {
            throw new NotImplementedException();
        }
    }
}
