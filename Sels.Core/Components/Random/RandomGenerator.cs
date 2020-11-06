using System;
using System.Collections.Generic;
using System.Text;
using SystemRandom = System.Random;

namespace Sels.Core.Components.Random
{
    public static class RandomGenerator
    {
        private static SystemRandom _random = new SystemRandom();
        private static object _threadlock = new object();

        public static int GetRandomNumber(int max)
        {
            return GetRandomNumber(0, max);
        }
        public static int GetRandomNumber(int min, int max)
        {
            lock (_threadlock)
            {
                return _random.Next(min, max);
            }
        }
    }
}
