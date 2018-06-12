using Karambolo.Common;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace AspNetSkeleton.Common.Utils
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = false)]
    public class DisplayTextAttribute : Attribute
    {
        public DisplayTextAttribute(string displayText)
        {
            DisplayText = displayText;
        }

        public DisplayTextAttribute(string displayText, string shortText)
            : this(displayText)
        {
            ShortText = shortText;
        }

        public string DisplayText { get; }
        public string ShortText { get; }
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

        public static string ShortText(this Enum @this)
        {
            var attribute = GetDisplayTextAttribute(@this.GetType(), @this.ToString());
            return attribute?.ShortText;
        }
    }
}
