using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace System
{
    public static class IDisposableExtensions
    {
        /// <summary>
        /// Try to dispose <paramref name="disposable"/>. Returns true when <paramref name="disposable"/> is disposed without throwing exceptions.
        /// </summary>
        /// <param name="disposable">Object to dispose</param>
        /// <param name="exceptionHandler">Optional exception handler</param>
        /// <returns>True if <paramref name="disposable"/> is not null and if dispose could be called without exceptions</returns>
        public static bool TryDispose(this IDisposable disposable, Action<Exception> exceptionHandler = null)
        {
            if (disposable.HasValue())
            {
                try
                {
                    disposable.Dispose();
                    return true;
                }
                catch (Exception ex)
                {
                    try
                    {
                        if (exceptionHandler.HasValue())
                        {
                            exceptionHandler(ex);
                        }
                    }
                    catch { }
                }
            }

            return false;
        }
    }
}
