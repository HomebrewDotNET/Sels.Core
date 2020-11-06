using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.Delegates
{
    public static class Delegates
    {
        public delegate TOut OutFunc<TIn, TOut>(out TIn argOne);
        public delegate TOut OutFunc<TIn, TInTwo, TOut>(TIn argOne, out TInTwo argTwo);
    }
}
