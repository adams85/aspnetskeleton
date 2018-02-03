using System;

namespace AspNetSkeleton.Common.Infrastructure
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class LoggerOptionsAttribute : Attribute
    {
        public LoggerOptionsAttribute(string sourceName)
        {
            SourceName = sourceName;
        }

        public string SourceName { get; set; }
    }
}
