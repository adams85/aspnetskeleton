using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using AspNetSkeleton.Base;
using AspNetSkeleton.Core;
using AspNetSkeleton.Service.Contract;
using AspNetSkeleton.Service.Contract.Commands;
using AspNetSkeleton.Service.Contract.DataObjects;
using AspNetSkeleton.Service.Contract.Queries;
using AspNetSkeleton.Service.Host.Core.Handlers.Mails;
using AspNetSkeleton.Service.Host.Core.Infrastructure.Mailing;
using Autofac.Features.Indexed;
using MailKit;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MimeKit;

namespace AspNetSkeleton.Service.Host.Core.Infrastructure.BackgroundWork
{
    public class MailSenderProcess : IBackgroundProcess, IDisposable
    {
        static Func<MailTransport> GetMailClientFactory(MailSettings mailSettings, string pickupBasePath)
        {
            if (!mailSettings.UsePickupDir)
            {
                var timeout = checked((int)mailSettings.Timeout.TotalMilliseconds);
                return () => new SmtpClient { Timeout = timeout };
            }
            else
            {
                var pickupDirPath = mailSettings.PickupDirPath ?? string.Empty;
                if (!Path.IsPathRooted(pickupDirPath))
                    pickupDirPath = Path.Combine(pickupBasePath, pickupDirPath);

                return () => new PickupDirMailClient(pickupDirPath);
            }
        }

        readonly Func<string, INotificationHandler> _notificationHandlerFactory;
        readonly IQueryDispatcher _queryDispatcher;
        readonly ICommandDispatcher _commandDispatcher;
        readonly ServiceHostCoreSettings _settings;
        readonly MailSettings _mailSettings;
        readonly Func<MailTransport> _mailClientFactory;
        MailTransport _mailClient;

        public ILogger Logger { get; set; }

        public MailSenderProcess(IIndex<string, INotificationHandler> notificationHandlers,
            IQueryDispatcher queryDispatcher, ICommandDispatcher commandDispatcher,
            IAppEnvironment environment, IOptions<ServiceHostCoreSettings> settings, IOptions<MailSettings> mailSettings)
        {
            Logger = NullLogger.Instance;

            _notificationHandlerFactory = c => notificationHandlers.TryGetValue(c, out INotificationHandler result) ? result : null;

            _queryDispatcher = queryDispatcher;
            _commandDispatcher = commandDispatcher;

            _settings = settings.Value;

            _mailSettings = mailSettings.Value;
            _mailClientFactory = GetMailClientFactory(_mailSettings, environment.AppBasePath);
        }

        async Task SendBatchAsync(NotificationData[] batch, CancellationToken shutDownToken)
        {
            var n = batch.Length;
            if (n == 0)
                return;

            // 1. creating messages

            var messageFactoryTasks = new Task<MimeMessage>[n];
            for (var i = 0; i < n; i++)
            {
                var notification = batch[i];

                var handler = _notificationHandlerFactory(notification.Code);
                messageFactoryTasks[i] =
                    handler != null ?
                    handler.CreateMailMessageAsync(notification, shutDownToken) :
                    Task.FromException<MimeMessage>(new ApplicationException($"No handler found for notification {notification.Code}"));
            }

            try { await Task.WhenAll(messageFactoryTasks); }
            catch { }

            // short-circuit if cancellation was requested
            shutDownToken.ThrowIfCancellationRequested();

            // 2. handling failures

            Task<MimeMessage> messageFactoryTask;
            var successCount = 0;
            for (var i = 0; i < n; i++)
                if ((messageFactoryTask = messageFactoryTasks[i]).IsFaulted)
                {
                    try { await messageFactoryTask; }
                    catch (Exception ex) { Logger.LogError(ex, "Message could not be created."); }

                    // marking the notifications as failed
                    await _commandDispatcher.DispatchAsync(new MarkNotificationsCommand
                    {
                        Id = batch[i].Id,
                        NewState = NotificationState.Failed,
                    }, shutDownToken);
                }
                else
                    successCount++;

            if (successCount == 0)
                return;

            // 3. sending messages

            if (_mailClient == null)
                _mailClient = _mailClientFactory();

            try
            {
                await _mailClient.ConnectAsync(_mailSettings.Host, _mailSettings.Port, _mailSettings.Security, shutDownToken);
            }
            catch (Exception ex)
            {
                var mailClient = _mailClient;
                _mailClient = null;
                mailClient.Dispose();
                ExceptionDispatchInfo.Capture(ex).Throw();
            }

            try
            {
                if (_mailSettings.Authenticate)
                    await _mailClient.AuthenticateAsync(_mailSettings.UserName, _mailSettings.Password, shutDownToken);

                for (var i = 0; i < n; i++)
                    if ((messageFactoryTask = messageFactoryTasks[i]).IsCompletedSuccessfully)
                    {
                        await _mailClient.SendAsync(await messageFactoryTask, shutDownToken);

                        // shutdown token not passed since notifications should be sent and deleted without interruption,
                        // otherwise they could be sent multiple times
                        // (for this reason, ExecuteAsync() should be awaited with timeout!)
                        await _commandDispatcher.DispatchAsync(new DeleteNotificationCommand
                        {
                            Id = batch[i].Id
                        }, CancellationToken.None);
                    }
            }
            finally
            {
                await _mailClient.DisconnectAsync(quit: true, shutDownToken);
            }
        }

        public async Task ExecuteAsync(CancellationToken shutDownToken)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            var stateToReset = NotificationState.Processing | NotificationState.Failed;
            while (true)
                try
                {
                    shutDownToken.ThrowIfCancellationRequested();

                    // resetting state of notifications whose processing...
                    // a) may have been interrupted or 
                    // b) failed during the previous run
                    await _commandDispatcher.DispatchAsync(new MarkNotificationsCommand
                    {
                        State = stateToReset,
                        NewState = NotificationState.Queued
                    }, shutDownToken);

                    stateToReset = NotificationState.Processing;

                    ListResult<NotificationData> batch;
                    do
                    {
                        shutDownToken.ThrowIfCancellationRequested();

                        // marking a batch of notifications for processing
                        await _commandDispatcher.DispatchAsync(new MarkNotificationsCommand
                        {
                            State = NotificationState.Queued,
                            NewState = NotificationState.Processing,
                            Count = _settings.MailSenderBatchSize,
                        }, shutDownToken);

                        // retrieving batch
                        batch = await _queryDispatcher.DispatchAsync(new ListNotificationsQuery
                        {
                            State = NotificationState.Processing,
                            OrderColumns = new[] { nameof(NotificationData.CreatedAt) }
                        }, shutDownToken);

                        await SendBatchAsync(batch.Rows, shutDownToken);
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
                    Logger.LogCritical(ex, "Critical error.");

                    await Task.Delay(_settings.WorkerIdleWaitTime);
                }
        }

        public void Dispose()
        {
            _mailClient?.Dispose();
        }
    }
}