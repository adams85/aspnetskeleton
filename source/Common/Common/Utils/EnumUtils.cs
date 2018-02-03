using Karambolo.Common;
using System;
using System.Linq;

namespace AspNetSkeleton.Common.Utils
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = false)]
    public class DisplayTextAttribute : Attribute
    {
        public DisplayTextAttribute(string displayName)
        {
            DisplayText = displayName;
        }

        public DisplayTextAttribute(string displayName, string abbreviation)
            : this(displayName)
        {
            ShortText = abbreviation;
        }

        public string DisplayText { get; private set; }
        public string ShortText { get; private set; }
    }

    public static class EnumUtils
    {
        static DisplayTextAttribute GetDisplayTextAttribute(Type type, string value)
        {
            var member = type.GetMember(value.ToString());
            return
                !ArrayUtils.IsNullOrEmpty(member) ?
                member[0].GetAttributes<DisplayTextAttribute>().FirstOrDefault() :
                null;
        }

        public static string DisplayText(this Enum @this)
        {
            var attribute = GetDisplayTextAttribute(@this.GetType(), @this.ToString());
            return attribute?.DisplayText;
        }

        public static string Abbreviation(this Enum @this)
        {
            var attribute = GetDisplayTextAttribute(@this.GetType(), @this.ToString());
            return attribute?.ShortText;
        }
    }
}
