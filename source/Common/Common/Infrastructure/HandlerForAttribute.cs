using System;

namespace AspNetSkeleton.Common.Infrastructure
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class HandlerForAttribute : Attribute
    {
        public HandlerForAttribute(string key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            Key = key;
        }

        public string Key { get; private set; }
    }
}
