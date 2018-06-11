using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetSkeleton.Common
{
    interface IPolymorphValueAccessor
    {
        object Value { get; }
    }

    public static class Polymorph
    {
        public static Polymorph<T> Create<T>(T value) where T : class
        {
            return new Polymorph<T>(value);
        }
    }

    // wrapper type for workaround inconsistent type name handling of JSON.NET (no type info for primitive, converted objects)
    public struct Polymorph<T> : IPolymorphValueAccessor where T : class
    {
        static Polymorph()
        {
            if (typeof(T).IsSealed)
                throw new ArgumentException("Type is not polymorphic.", nameof(T));
        }

        public Polymorph(T value)
        {
            Value = value;
        }

        public T Value { get; }

        object IPolymorphValueAccessor.Value => Value;

        public override string ToString()
        {
            return Value?.ToString() ?? string.Empty;
        }
    }
}
