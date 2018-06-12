using AspNetSkeleton.Common.DataTransfer;
using AspNetSkeleton.Common;

namespace AspNetSkeleton.Service.Contract
{
    public enum ServiceErrorCode
    {
        Unknown,

        ParamNotSpecified = 0x10,
        ParamNotValid,
        EntityNotFound,
        EntityNotUnique,
        EntityDependent,
    }

    public abstract class ServiceErrorException : WebApiErrorException
    {
        protected ServiceErrorException(ErrorData error) : base(error) { }

        public ServiceErrorCode ErrorCode => (ServiceErrorCode)Error.Code;
    }
}
