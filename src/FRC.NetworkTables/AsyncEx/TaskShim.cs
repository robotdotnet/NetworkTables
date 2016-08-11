using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// https://github.com/StephenCleary/AsyncEx/blob/master/Source/Nito.AsyncEx%20(NET45%2C%20Win8%2C%20WP8%2C%20WPA81)/Internal/TaskShim.cs
// ReSharper disable once CheckNamespace
namespace Nito.AsyncEx.Internal
{
    internal static class TaskShim
    {
        public static Task Run(Action func)
        {
            return Task.Run(func);
        }

        public static Task Run(Func<Task> func)
        {
            return Task.Run(func);
        }

        public static Task<T> Run<T>(Func<T> func)
        {
            return Task.Run(func);
        }

        public static Task<T> Run<T>(Func<Task<T>> func)
        {
            return Task.Run(func);
        }

        public static Task<T> FromResult<T>(T value)
        {
            return Task.FromResult(value);
        }

        public static Task<T[]> WhenAll<T>(IEnumerable<Task<T>> tasks)
        {
            return Task.WhenAll(tasks);
        }

        public static Task<T[]> WhenAll<T>(params Task<T>[] tasks)
        {
            return Task.WhenAll(tasks);
        }

        public static Task WhenAll(params Task[] tasks)
        {
            return Task.WhenAll(tasks);
        }

        public static Task WhenAll(IEnumerable<Task> tasks)
        {
            return Task.WhenAll(tasks);
        }

        public static Task<Task<TResult>> WhenAny<TResult>(IEnumerable<Task<TResult>> tasks)
        {
            return Task.WhenAny(tasks);
        }

        public static Task<Task> WhenAny(IEnumerable<Task> tasks)
        {
            return Task.WhenAny(tasks);
        }

        public static Task<Task<TResult>> WhenAny<TResult>(params Task<TResult>[] tasks)
        {
            return Task.WhenAny(tasks);
        }

        public static Task<Task> WhenAny(params Task[] tasks)
        {
            return Task.WhenAny(tasks);
        }
    }
}