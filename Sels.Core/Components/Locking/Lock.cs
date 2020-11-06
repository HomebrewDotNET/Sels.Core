using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Components.Locking
{
    public class Lock : IDisposable
    {
        protected readonly object _threadLock = new object();

        // Properties
        public bool IsValid { get; private set; }

        public object AcquiredBy { get; }
        public DateTime LockDate { get; }

        public Lock(object requester)
        {
            AcquiredBy = requester;
            IsValid = true;
            LockDate = DateTime.Now;
        }

        public override string ToString()
        {
            return $"Lock<{AcquiredBy},{LockDate},{IsValid}>";
        }

        public void Dispose()
        {
            lock (_threadLock)
            {
                IsValid = false;
            }
        }
    }
}
