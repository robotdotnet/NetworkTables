using System;

// https://github.com/StephenCleary/AsyncEx/blob/master/Source/Enlightenment/Nito.AsyncEx.Enlightenment%20(NET4%2C%20Win8%2C%20SL5%2C%20WP8%2C%20WPA81)/Internal/PlatformEnlightenment/ExceptionEnlightenment.cs
// ReSharper disable once CheckNamespace
namespace Nito.AsyncEx.Internal.PlatformEnlightenment
{
    internal static class ExceptionEnlightenment
    {
        // ReSharper disable once UnusedParameter.Global
        public static Exception PrepareForRethrow(Exception exception)
        {
            throw Enlightenment.Exception();
        }
    }
}