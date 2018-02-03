using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using AspNetSkeleton.Service.Contract.DataObjects;
using RazorLight;
using System.Threading.Tasks;
using System.Threading;
using Karambolo.Common;

namespace AspNetSkeleton.Service.Host.Core.Handlers.Mails
{
    public interface INotificationHandler
    {
        Task<MailMessage> CreateMailMessageAsync(NotificationData notification, CancellationToken cancellationToken);
    }

    public abstract class NotificationHandler<TModel> : INotificationHandler
    {
        readonly IRazorLightEngine _razorEngine;

        protected NotificationHandler(IRazorLightEngine razorEngine)
        {
            _razorEngine = razorEngine;
        }

        protected abstract TModel CreateModel(string data);

        protected abstract string GenerateSubject(TModel model);

        protected virtual string GetBodyTemplatePath(string code, TModel model)
        {
            return $"Mails/{code}.cshtml";
        }

        protected virtual Task<string> GenerateBodyAsync(string code, TModel model, CancellationToken cancellationToken)
        {
            var modelType = model?.GetType();
            var templatePath = GetBodyTemplatePath(code, model);

            return _razorEngine.CompileRenderAsync(templatePath, model).AsCancellable(cancellationToken);
        }

        protected abstract string GetSender(TModel model);

        protected virtual string GetFrom(TModel model)
        {
            return GetSender(model);
        }

        protected abstract IEnumerable<string> GetTo(TModel model);

        protected virtual IEnumerable<string> GetCc(TModel model)
        {
            return Enumerable.Empty<string>();
        }

        protected virtual IEnumerable<string> GetBcc(TModel model)
        {
            return Enumerable.Empty<string>();
        }

        protected virtual bool IsBodyHtml => true;

        static void AddAddressesToCollection(MailAddressCollection collection, IEnumerable<string> addresses)
        {
            foreach (var address in addresses)
                collection.Add(new MailAddress(address));
        }

        public async Task<MailMessage> CreateMailMessageAsync(NotificationData notification, CancellationToken cancellationToken)
        {
            var model = CreateModel(notification.Data);

            var result = new MailMessage();

            result.Sender = new MailAddress(GetSender(model));
            result.From = new MailAddress(GetFrom(model));
            AddAddressesToCollection(result.To, GetTo(model));
            AddAddressesToCollection(result.CC, GetCc(model));
            AddAddressesToCollection(result.Bcc, GetBcc(model));

            result.Subject = GenerateSubject(model);

            result.Body = await GenerateBodyAsync(notification.Code, model, cancellationToken).ConfigureAwait(false);
            result.IsBodyHtml = IsBodyHtml;

            return result;
        }
    }
}