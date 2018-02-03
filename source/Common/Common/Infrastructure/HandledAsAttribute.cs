using System;

namespace AspNetSkeleton.Common.Infrastructure
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class HandledAsAttribute : Attribute
    {
        public HandledAsAttribute(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            Type = type;
        }

        public Type Type { get; private set; }
    }
}
