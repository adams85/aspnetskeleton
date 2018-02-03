using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using RazorEngine.Templating;
using System.IO;
using AspNetSkeleton.Service.Contract.DataObjects;
using AspNetSkeleton.Service.Host.Core.Infrastructure;

namespace AspNetSkeleton.Service.Host.Core.Handlers.Mails
{
    public interface INotificationHandler
    {
        MailMessage CreateMailMessage(NotificationData notification);
    }

    public abstract class NotificationHandler<TModel> : INotificationHandler
    {
        readonly IServiceHostEnvironment _hostEnvironment;
        readonly IRazorEngineService _razorEngine;

        protected NotificationHandler(IServiceHostEnvironment hostEnvironment, IRazorEngineService razorEngine)
        {
            _hostEnvironment = hostEnvironment;
            _razorEngine = razorEngine;
        }

        protected abstract TModel CreateModel(string data);

        protected abstract string GenerateSubject(TModel model);

        protected virtual string GetBodyTemplatePath(string code, TModel model)
        {
            return $"~/Templates/Mails/{code}.cshtml";
        }

        protected virtual string GenerateBody(string code, TModel model)
        {
            var modelType = model?.GetType();
            var templatePath = GetBodyTemplatePath(code, model);
            if (!_razorEngine.IsTemplateCached(templatePath, modelType))
            {
                var templatePhysicalPath = _hostEnvironment.MapPath(templatePath);
                var template = File.ReadAllText(templatePhysicalPath);
                _razorEngine.Compile(template, templatePath, modelType);
            }
            var result = _razorEngine.Run(templatePath, modelType, model, null);
            return result;
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

        public virtual MailMessage CreateMailMessage(NotificationData notification)
        {
            var model = CreateModel(notification.Data);

            var result = new MailMessage();

            result.Sender = new MailAddress(GetSender(model));
            result.From = new MailAddress(GetFrom(model));
            AddAddressesToCollection(result.To, GetTo(model));
            AddAddressesToCollection(result.CC, GetCc(model));
            AddAddressesToCollection(result.Bcc, GetBcc(model));

            result.Subject = GenerateSubject(model);

            result.Body = GenerateBody(notification.Code, model);
            result.IsBodyHtml = IsBodyHtml;
            
            return result;
        }
    }
}