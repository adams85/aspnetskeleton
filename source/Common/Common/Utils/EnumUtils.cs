using Karambolo.Common;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace AspNetSkeleton.Common.Utils
{
    public static class EnumUtils
    {
        static DisplayAttribute GetDisplayTextAttribute(Type type, string value)
        {
            var member = type.GetMember(value.ToString());
            return
                !ArrayUtils.IsNullOrEmpty(member) ?
                member[0].GetAttributes<DisplayAttribute>().FirstOrDefault() :
                null;
        }

        public static string DisplayText(this Enum @this)
        {
            var attribute = GetDisplayTextAttribute(@this.GetType(), @this.ToString());
            return attribute?.Name;
        }

        public static string Abbreviation(this Enum @this)
        {
            var attribute = GetDisplayTextAttribute(@this.GetType(), @this.ToString());
            return attribute?.ShortName;
        }
    }
}
