using AspNetSkeleton.Common.DataTransfer;
using AspNetSkeleton.Common.Utils;
using System.ComponentModel.DataAnnotations;

namespace AspNetSkeleton.Service.Contract
{
    public enum CommandErrorCode
    {
        Unknown = ServiceErrorCode.Unknown,

        [Display(Name = "Value for parameter {0} was not specified.")]
        ParamNotSpecified = ServiceErrorCode.ParamNotSpecified,

        [Display(Name = "Value of parameter {0} is not valid.")]
        ParamNotValid = ServiceErrorCode.ParamNotValid,

        [Display(Name = "Entity identified by parameter {0} was not found.")]
        EntityNotFound = ServiceErrorCode.EntityNotFound,

        [Display(Name = "Entity identified by parameter {0} is not unique.")]
        EntityNotUnique = ServiceErrorCode.EntityNotUnique,

        [Display(Name = "Entity identified by parameter {0} has dependencies.")]
        EntityDependent = ServiceErrorCode.EntityDependent,

        [Display(Name = "Limit of connected devices has been reached.")]
        DeviceLimitExceeded = 0x10000,

        [Display(Name = "Time required to disconnecting the device has not expired.")]
        DeviceDisconnectTimeNotExpired
    }

    public class CommandErrorException : ServiceErrorException
    {
        public CommandErrorException(ErrorData error) : base(error) { }

        public CommandErrorException(CommandErrorCode errorCode, params object[] args)
            : this(new ErrorData { Code = (int)errorCode, Args = args }) { }

        public new CommandErrorCode ErrorCode => (CommandErrorCode)Error.Code;

        public override string Message
        {
            get
            {
                var displayText = ErrorCode.DisplayText();
                return
                    displayText != null ?
                    string.Format(displayText, Error.Args) :
                    $"Command execution failed with error code {ErrorCode}.";
            }
        }
    }
}
