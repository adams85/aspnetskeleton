using AspNetSkeleton.Service.Contract.Commands;
using AspNetSkeleton.Common.Cli;
using System.Collections.Generic;
using AspNetSkeleton.Common.Infrastructure;

namespace AspNetSkeleton.AdminTools.Operations.Users
{
    [HandlerFor(Name)]
    public class CreateUserOperation : ApiOperation
    {
        public const string Name = "create-user";
        const int defaultDeviceLimit = 2;

        public CreateUserOperation(string[] args, IApiOperationContext context) : base(args, context) { }

        protected override int MandatoryArgCount => 2;

        protected override IEnumerable<string> GetUsage()
        {
            yield return $"{Context.AppName} {Name} <user-name> <password> [/a(=is-approved)] [/e=<email=user-name>] [/d=<device-limit={defaultDeviceLimit}>] [/fn=<first-name>] [/ln=<last-name>]";
        }        

        protected override void ExecuteCore()
        {
            var userName = MandatoryArgs[0];
            var password = MandatoryArgs[1];

            var isApproved = OptionalArgs.ContainsKey("a");

            if (!OptionalArgs.TryGetValue("e", out string email))
                email = userName;

            int deviceLimit;
            if (OptionalArgs.TryGetValue("d", out string deviceLimitString))
            {
                if (!int.TryParse(deviceLimitString, out deviceLimit))
                    throw new OperationErrorException("Device limit is not a number.");
            }
            else
                deviceLimit = defaultDeviceLimit;

            if (!OptionalArgs.TryGetValue("fn", out string firstName))
                firstName = null;

            if (!OptionalArgs.TryGetValue("ln", out string lastName))
                lastName = null;

            object key = null;
            Command(new CreateUserCommand
            {
                UserName = userName,
                Email = email,
                Password = password,
                IsApproved = isApproved,
                CreateProfile = true,
                DeviceLimit = deviceLimit,
                FirstName = firstName,
                LastName = lastName,
                OnKeyGenerated = (c, k) => key = k,
            });

            Context.Out.WriteLine($"User created successfully with id {key}.");
        }
    }
}
