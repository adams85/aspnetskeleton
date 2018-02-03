using System;

namespace AspNetSkeleton.Core
{
    public class HttpResponseException : Exception
    {
        public HttpResponseException(int statusCode, object content = null) : base($"{statusCode} HTTP error response.")
        {
            StatusCode = statusCode;
            Content = content;
        }

        public int StatusCode { get; }
        public object Content { get; }
    }
}