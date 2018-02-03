using AspNetSkeleton.Common.DataTransfer;
using AspNetSkeleton.Common.Utils;
using AspNetSkeleton.Common;

namespace AspNetSkeleton.Api.Contract
{
    public enum ApiErrorCode
    {
        Unknown,

        [DisplayText("Request had missing or invalid parameters. {0}")]
        InvalidRequest,

        [DisplayText("Device is not allowed.")]
        DeviceNotAllowed,
    }

    public class ApiErrorException : WebApiErrorException
    {
        public ApiErrorException(ApiErrorCode errorCode, params object[] args)
            : this(new ErrorData { Code = (int)errorCode, Args = args }, null) { }

        public ApiErrorException(ErrorData error, string authToken)
            : base(error)
        {
            AuthToken = authToken;
        }

        public ApiErrorCode ErrorCode => (ApiErrorCode)Error.Code;

        public string AuthToken { get; }

        public override string Message
        {
            get
            {
                var displayText = ErrorCode.DisplayText();
                return
                    displayText != null ?
                    string.Format(displayText, Error.Args) :
                    $"API request failed with error code {ErrorCode}.";
            }
        }
    }
}
