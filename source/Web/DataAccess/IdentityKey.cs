using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Expressions;
using LinqToDB.Mapping;
using LinqToDB.SqlQuery;
using System;
using System.Collections.Generic;

namespace AspNetSkeleton.DataAccess
{
    interface IIdentityKeyValueSetter
    {
        void SetValue(object value);
    }

    public abstract class IdentityKey : IComparable
    {
        class ConvertProvider<T> : IGenericInfoProvider where T : struct
        {
            public void SetInfo(MappingSchema mappingSchema)
            {
                mappingSchema.SetConverter<IdentityKey<T>, T>(v => v.Value);

                mappingSchema.SetConverter<IdentityKey<T>, DataParameter>(v => new DataParameter
                {
                    Value = v?.Value,
                    DataType = SqlDataType.GetDataType(typeof(T)).DataType
                });

                mappingSchema.SetConverter<T, IdentityKey<T>>(From);

                mappingSchema.SetConverter<DataParameter, IdentityKey<T>>(v =>
                {
                    if (v.DataType != SqlDataType.GetDataType(typeof(T)).DataType)
                        throw new InvalidOperationException("Parameter and identity key type mismatch.");

                    return v.Value != null ? From(MappingSchema.Default.ChangeTypeTo<T>(v.Value)) : null;
                });
            }
        }

        internal static readonly MappingSchema MappingSchema;

        static IdentityKey()
        {
            MappingSchema = new MappingSchema();

            // registering supported identity key types
            MappingSchema.AddScalarType(typeof(IdentityKey<int>), DataType.Int32);
            MappingSchema.AddScalarType(typeof(IdentityKey<long>), DataType.Int64);
            MappingSchema.AddScalarType(typeof(IdentityKey<decimal>), DataType.Decimal);

            // registering converters which are able to convert to and from the underlying type
            MappingSchema.SetGenericConvertProvider(typeof(ConvertProvider<>));
        }

        public static bool IsIdentityKeyType(Type type)
        {
            return type.IsGenericType && type.IsAbstract && type.IsClass && type.GetGenericTypeDefinition() == typeof(IdentityKey<>);
        }

        public static bool IsValidKeyType(Type type)
        {
            return type == typeof(int) || type == typeof(long) || type == typeof(decimal);
        }

        #region Static
        class Static<T> : IdentityKey<T>
            where T : struct
        {
            public Static(T value)
            {
                Value = value;
            }

            public override bool IsAvailable => true;

            public override T Value { get; }
        }

        public static IdentityKey From(object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var keyType = value.GetType();
            if (!IsValidKeyType(keyType))
                throw new ArgumentException("Key type is invalid.", nameof(value));

            return (IdentityKey)Activator.CreateInstance(typeof(Static<>).MakeGenericType(keyType), value);
        }

        public static IdentityKey<T> From<T>(T value) where T : struct
        {
            if (!IsValidKeyType(typeof(T)))
                throw new ArgumentException("Key type is invalid.", nameof(T));

            return new Static<T>(value);
        }
        #endregion

        #region Promise
        class Promise<T> : IdentityKey<T>, IIdentityKeyValueSetter
            where T : struct
        {
            T? _value;

            public override bool IsAvailable => _value != null;

            public override T Value
            {
                get
                {
                    if (!IsAvailable)
                        throw new InvalidOperationException("Value is not available yet.");

                    return _value.Value;
                }
            }

            public void SetValue(object value)
            {
                if (IsAvailable)
                    throw new InvalidOperationException("Value is already available.");

                _value =
                    value.GetType() == typeof(T) ?
                    (T)value :
                    MappingSchema.Default.ChangeTypeTo<T>(value);
            }
        }

        internal static IdentityKey CreatePromise(Type type, out IIdentityKeyValueSetter valueSetter)
        {
            if (!IsValidKeyType(type))
                throw new ArgumentException("Key type is invalid.", nameof(type));

            var result = (IdentityKey)Activator.CreateInstance(typeof(Promise<>).MakeGenericType(type));
            valueSetter = (IIdentityKeyValueSetter)result;
            return result;
        }
        #endregion

        public static bool operator ==(IdentityKey left, IdentityKey right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(IdentityKey left, IdentityKey right)
        {
            return !(left == right);
        }

        public abstract object ValueObject { get; }
        public abstract bool IsAvailable { get; }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        public override bool Equals(object obj)
        {
            throw new NotImplementedException();
        }

        public abstract int CompareTo(object obj);

        public IdentityKey<T> As<T>() where T : struct
        {
            return (IdentityKey<T>)this;
        }
    }

    public abstract class IdentityKey<T> : IdentityKey, IEquatable<IdentityKey<T>>, IEquatable<T>, IComparable<IdentityKey<T>>, IComparable<T>
        where T : struct
    {
        public abstract T Value { get; }
        public sealed override object ValueObject => Value;

        public override int GetHashCode()
        {
            return IsAvailable ? Value.GetHashCode() : base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return 
                obj is IdentityKey<T> other ? Equals(other) : 
                obj is T otherValue ? Equals(otherValue) :
                false;
        }

        public bool Equals(IdentityKey<T> other)
        {
            if (other == null)
                return false;

            return IsAvailable ? EqualityComparer<T>.Default.Equals(Value, other.Value) : ReferenceEquals(this, other);
        }

        public bool Equals(T other)
        {
            return IsAvailable && EqualityComparer<T>.Default.Equals(Value, other);
        }

        public override int CompareTo(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            return
                obj is IdentityKey<T> other ? CompareTo(other) :
                obj is T otherValue ? CompareTo(otherValue) :
                throw new ArgumentException("Type of comparand is incompatible.", nameof(obj));
        }

        public int CompareTo(IdentityKey<T> other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            if (!IsAvailable || !other.IsAvailable)
                throw new InvalidOperationException("Value is not available yet.");

            return Comparer<T>.Default.Compare(Value, other.Value);
        }

        public int CompareTo(T other)
        {
            if (!IsAvailable)
                throw new InvalidOperationException("Value is not available yet.");

            return Comparer<T>.Default.Compare(Value, other);
        }

        public override string ToString()
        {
            return IsAvailable ? Value.ToString() : "(n/a)";
        }
    }
}
