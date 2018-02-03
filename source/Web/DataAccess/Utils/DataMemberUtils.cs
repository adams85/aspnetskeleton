using System;
using System.Reflection;

namespace AspNetSkeleton.DataAccess.Utils
{
    public static class DataMemberUtils
    {
        public static T Get<T>(this MemberInfo @this, Func<PropertyInfo, T> getterIfProperty, Func<FieldInfo, T> getterIfField)
        {
            return
                @this.MemberType == MemberTypes.Property ? getterIfProperty((PropertyInfo)@this) :
                @this.MemberType == MemberTypes.Field ? getterIfField((FieldInfo)@this) :
                throw new ArgumentException("Invalid member type.", nameof(@this));
        }

        public static Type MemberType(this MemberInfo @this)
        {
            return @this.Get(pi => pi.PropertyType, fi => fi.FieldType);
        }
    }
}
