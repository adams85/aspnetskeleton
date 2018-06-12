using System;
using AspNetSkeleton.Common;

namespace AspNetSkeleton.Service.Contract.Commands
{
    public class CreateUserCommand : IKeyGeneratorCommand
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool IsApproved { get; set; }

        public bool CreateProfile { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public int DeviceLimit { get; set; }

        public Action<ICommand, Polymorph<object>> OnKeyGenerated { get; set; }
    }

    public class DeleteUserCommand : ICommand
    {
        public string UserName { get; set; }
    }

    public class ApproveUserCommand : ICommand
    {
        public string UserName { get; set; }
        public bool Verify { get; set; }
        public string VerificationToken { get; set; }
    }

    public class ResetPasswordCommand : ICommand
    {
        public string UserName { get; set; }
        public TimeSpan TokenExpirationTimeSpan { get; set; }
    }

    public class ChangePasswordCommand : ICommand
    {
        public string UserName { get; set; }
        public string NewPassword { get; set; }
        public bool Verify { get; set; }
        public string VerificationToken { get; set; }
    }

    public class RegisterUserActivityCommand : ICommand
    {
        public string UserName { get; set; }
        public bool? SuccessfulLogin { get; set; }
        public bool UIActivity { get; set; }
    }

    public class LockUserCommand : ICommand
    {
        public string UserName { get; set; }
    }

    public class UnlockUserCommand : ICommand
    {
        public string UserName { get; set; }
    }

    public class ConnectDeviceCommand : ICommand
    {
        public int UserId { get; set; }
        public string DeviceId { get; set; }
        public string DeviceName { get; set; }
    }

    public class DisconnectDeviceCommand : ICommand
    {
        public int UserId { get; set; }
        public string DeviceId { get; set; }
        public TimeSpan DisconnectTimeSpan { get; set; }
    }
}
