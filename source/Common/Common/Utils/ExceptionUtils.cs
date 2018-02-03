using System;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace AspNetSkeleton.Common.Utils
{
    public static class ExceptionUtils
    {
        public static T UnwrapTargetInvocationException<T>(Func<T> func)
        {
            try { return func(); }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }

        public static void UnwrapTargetInvocationException(Action action)
        {
            UnwrapTargetInvocationException<object>(() =>
            {
                action();
                return null;
            });
        }
    }
}
