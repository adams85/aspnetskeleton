using AspNetSkeleton.Common.Infrastructure;
using AspNetSkeleton.Service.Host.Core.Handlers.Mails;
using AspNetSkeleton.Service.Contract;
using AspNetSkeleton.Service.Contract.Commands;
using AspNetSkeleton.Service.Contract.DataObjects;
using AspNetSkeleton.Service.Contract.Queries;
using Autofac.Features.Indexed;
using Karambolo.Common.Logging;
using System;
using System.Globalization;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetSkeleton.Service.Host.Core.Infrastructure.BackgroundWork
{
    public class MailSenderProcess : IBackgroundProcess, IDisposable
    {
        readonly CancellationToken _shutdownToken;
        readonly Func<string, INotificationHandler> _notificationHandlerFactory;
        readonly IQueryDispatcher _queryDispatcher;
        readonly ICommandDispatcher _commandDispatcher;
        readonly IServiceHostCoreSettings _settings;
        readonly SmtpClient _smtpClient;

        [LoggerOptions("AspNetSkeleton::MailSenderProcess")]
        public ILogger Logger { get; set; }

        public MailSenderProcess(IShutDownTokenAccessor shutdownTokenAccessor, IIndex<string, INotificationHandler> notificationHandlers,
            IQueryDispatcher queryDispatcher, ICommandDispatcher commandDispatcher, IServiceHostCoreSettings settings)
        {
            Logger = NullLogger.Instance;

            _shutdownToken = shutdownTokenAccessor.ShutDownToken;
            _notificationHandlerFactory = c => notificationHandlers.TryGetValue(c, out INotificationHandler result) ? result : null;

            _queryDispatcher = queryDispatcher;
            _commandDispatcher = commandDispatcher;

            _settings = settings;

            _smtpClient = new SmtpClient();
        }

        async Task HandleAsync(NotificationData notification)
        {
            var handler = _notificationHandlerFactory(notification.Code);
            if (handler != null)
            {
                var mailMessage = handler.CreateMailMessage(notification);

                if (mailMessage != null)
                    await _smtpClient.SendMailAsync(mailMessage).ConfigureAwait(false);
            }
            else
                Logger.LogWarning("No handler found for notification {0}", notification.Code);

            // shutdown token not passed since notifications should be sent and deleted without interruption,
            // otherwise they could be sent multiple times
            // (for this reason, ExecuteAsync() should be awaited with timeout!)
            await _commandDispatcher.DispatchAsync(new DeleteNotificationCommand
            {
                Id = notification.Id
            }, CancellationToken.None).ConfigureAwait(false);
        }

        public async Task ExecuteAsync()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            while (true)
                try
                {
                    _shutdownToken.ThrowIfCancellationRequested();

                    // resetting state of notifications whose processing may have been interrupted
                    await _commandDispatcher.DispatchAsync(new MarkNotificationsCommand
                    {
                        State = NotificationState.Queued
                    }, _shutdownToken);

                    ListResult<NotificationData> batch;
                    do
                    {
                        _shutdownToken.ThrowIfCancellationRequested();

                        await _commandDispatcher.DispatchAsync(new MarkNotificationsCommand
                        {
                            State = NotificationState.Processing,
                            Count = _settings.MailSenderBatchSize,
                        }, _shutdownToken);

                        batch = await _queryDispatcher.DispatchAsync(new ListNotificationsQuery
                        {
                            State = NotificationState.Processing,
                            OrderColumns = new[] { nameof(NotificationData.CreatedAt) }
                        }, _shutdownToken);

                        // TODO: SmtpClient.SendAsync() is not thread-safe, simple parallelization won't work.
                        // If the application needs to send a large amount of mails,
                        // some kind of pooling or consumer/producer pattern should be implemented.
                        var n = batch.Rows.Length;
                        for (var i = 0; i < n; i++)
                            await HandleAsync(batch.Rows[i]);
                    }
                    while (batch.Rows.Length >= _settings.MailSenderBatchSize);

                    await Task.Delay(_settings.WorkerIdleWaitTime);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    Logger.LogCritical("{0}", ex);

                    await Task.Delay(_settings.WorkerIdleWaitTime);
                }
        }

        public void Dispose()
        {
            _smtpClient?.Dispose();
        }
    }
}