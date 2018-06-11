using System;
using System.Collections.Generic;
using System.Text;
using AspNetSkeleton.Common;
using AspNetSkeleton.Service.Contract.Commands;
using AspNetSkeleton.Service.Contract.DataObjects;

namespace AspNetSkeleton.Service.Helpers
{
    static class NotificationHelper
    {
        public static CreateNotificationCommand ToCommand<T>(this T @this, Action<T> setter)
            where T : INotificationArgs
        {
            return new CreateNotificationCommand
            {
                Code = @this.Code,
                Data = Polymorph.Create<object>(new Func<string>(() =>
                {
                    setter(@this);
                    return @this.Serialize();
                }))
            };
        }
    }
}
