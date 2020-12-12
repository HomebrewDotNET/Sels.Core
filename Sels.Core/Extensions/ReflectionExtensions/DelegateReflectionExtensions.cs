using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Extensions.Reflection.Delegates
{
    public static class DelegateReflectionExtensions
    {
        public static bool AreTheSame(this Delegate source, Delegate target)
        {
            while (source.Target is Delegate)
                source = source.Target as Delegate;
            while (target.Target is Delegate)
                target = target.Target as Delegate;

            if (source == target)
                return true;

            if (source == null || target == null)
                return false;

            if (source.Target != target.Target)
                return false;
            byte[] sourceBody = source.Method.GetMethodBody().GetILAsByteArray();
            byte[] targetBody = target.Method.GetMethodBody().GetILAsByteArray();
            if (sourceBody.Length != targetBody.Length)
                return false;
            for (int i = 0; i < sourceBody.Length; i++)
            {
                if (sourceBody[i] != targetBody[i])
                    return false;
            }
            return true;
        }

        #region AddUnique
        public static void AddUnique(this List<Delegate> delegates, Delegate delegateToAdd)
        {
            bool containsItem = false;

            foreach (var delegateToCheck in delegates)
            {
                if (delegateToCheck.AreTheSame(delegateToAdd))
                {
                    containsItem = true;
                    break;
                }
            }

            if (!containsItem)
            {
                delegates.Add(delegateToAdd);
            }
        }

        public static void AddUnique<T>(this List<Predicate<T>> delegates, Predicate<T> delegateToAdd)
        {
            bool containsItem = false;

            foreach (var delegateToCheck in delegates)
            {
                if (delegateToCheck.AreTheSame(delegateToAdd))
                {
                    containsItem = true;
                    break;
                }
            }

            if (!containsItem)
            {
                delegates.Add(delegateToAdd);
            }
        }
        #endregion


    }
}
