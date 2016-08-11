using System;
using System.Threading;

namespace NetworkTables.Extensions
{
    internal static class AutoResetEventExtensions
    {
        /// <summary>
        /// Extension for AutoReset that takes a mutex and a ref entered bool, 
        /// and unlocks the mutex before waiting for the specified timeout. 
        /// The predicate checks if something goes wrong.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="mutex"></param>
        /// <param name="lockEntered"></param>
        /// <param name="timeout"></param>
        /// <param name="pred"></param>
        /// <returns></returns>
        public static bool WaitTimeout(this AutoResetEvent e, object mutex, ref bool lockEntered,
            TimeSpan timeout, Func<bool> pred)
        {
            //Throw if thread currently doesn't own the lock
            if (!Monitor.IsEntered(mutex))
            {
                throw new SynchronizationLockException();
            }
            if (timeout < TimeSpan.Zero && timeout != Timeout.InfiniteTimeSpan)
                timeout = TimeSpan.Zero;
            //While pred is false.
            // ReSharper disable once LoopVariableIsNeverChangedInsideLoop
            while (!pred())
            {
                Monitor.Exit(mutex);
                lockEntered = false;
                if (!e.WaitOne(timeout))
                {
                    //Timed out
                    Monitor.Enter(mutex, ref lockEntered);
                    return pred();
                }
                Monitor.Enter(mutex, ref lockEntered);
            }

            return true;
        }

        public static bool WaitTimeout(this AutoResetEvent e, object mutex, ref bool lockEntered, TimeSpan timeout)
        {
            //Throw if thread currently doesn't own the lock
            if (!Monitor.IsEntered(mutex))
            {
                throw new SynchronizationLockException();
            }
            if (timeout < TimeSpan.Zero && timeout != Timeout.InfiniteTimeSpan)
                timeout = TimeSpan.Zero;
            Monitor.Exit(mutex);
            lockEntered = false;
            if (!e.WaitOne(timeout))
            {
                //Timed out
                Monitor.Enter(mutex, ref lockEntered);
                return false;
            }
            Monitor.Enter(mutex, ref lockEntered);
            return true;
        }

        public static void Wait(this AutoResetEvent e, object mutex, ref bool lockEntered, Func<bool> pred)
        {
            //Throw if thread currently doesn't own the lock
            if (!Monitor.IsEntered(mutex))
            {
                throw new SynchronizationLockException();
            }
            //While pred is false.
            // ReSharper disable once LoopVariableIsNeverChangedInsideLoop
            while (!pred())
            {
                Monitor.Exit(mutex);
                lockEntered = false;
                e.WaitOne();
                Monitor.Enter(mutex, ref lockEntered);
            }
        }

        public static void Wait(this AutoResetEvent e, object mutex, ref bool lockEntered)
        {
            //Throw if thread currently doesn't own the lock
            if (!Monitor.IsEntered(mutex))
            {
                throw new SynchronizationLockException();
            }
            Monitor.Exit(mutex);
            lockEntered = false;
            e.WaitOne();
            Monitor.Enter(mutex, ref lockEntered);
        }
    }
}
