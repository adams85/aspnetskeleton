using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetSkeleton.Service.Host.Core.Infrastructure.Mailing
{
    public class PickupDirMailClient : MailTransport
    {
        readonly string _path;

        public PickupDirMailClient() : this(Environment.CurrentDirectory) { }

        public PickupDirMailClient(string path) : base(new NullProtocolLogger())
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            _path = path;
        }

        public override object SyncRoot => this;

        protected override string Protocol
        {
            get { return "smtp"; }
        }

        public override HashSet<string> AuthenticationMechanisms => default;

        public override int Timeout
        {
            get => default;
            set { }
        }

        public override bool IsConnected => default;

        public override bool IsSecure => default;

        public override bool IsAuthenticated => default;

        static void AddUnique(IList<MailboxAddress> recipients, HashSet<string> unique, IEnumerable<MailboxAddress> mailboxes)
        {
            foreach (var mailbox in mailboxes)
                if (unique.Add(mailbox.Address))
                    recipients.Add(mailbox);
        }

        static MailboxAddress GetMessageSender(MimeMessage message)
        {
            if (message.ResentSender != null)
                return message.ResentSender;

            if (message.ResentFrom.Count > 0)
                return message.ResentFrom.Mailboxes.FirstOrDefault();

            if (message.Sender != null)
                return message.Sender;

            return message.From.Mailboxes.FirstOrDefault();
        }

        static IList<MailboxAddress> GetMessageRecipients(MimeMessage message)
        {
            var unique = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var recipients = new List<MailboxAddress>();

            if (message.ResentSender != null || message.ResentFrom.Count > 0)
            {
                AddUnique(recipients, unique, message.ResentTo.Mailboxes);
                AddUnique(recipients, unique, message.ResentCc.Mailboxes);
                AddUnique(recipients, unique, message.ResentBcc.Mailboxes);
            }
            else
            {
                AddUnique(recipients, unique, message.To.Mailboxes);
                AddUnique(recipients, unique, message.Cc.Mailboxes);
                AddUnique(recipients, unique, message.Bcc.Mailboxes);
            }

            return recipients;
        }

        async Task WriteAsync(FormatOptions options, MimeMessage message, CancellationToken cancellationToken = default, ITransferProgress progress = null)
        {
            var format = options.Clone();
            format.HiddenHeaders.Add(HeaderId.ContentLength);
            format.HiddenHeaders.Add(HeaderId.ResentBcc);
            format.HiddenHeaders.Add(HeaderId.Bcc);
            format.NewLineFormat = NewLineFormat.Dos;

            // prepare the message
            message.Prepare(EncodingConstraint.SevenBit, 998);

            var n = 16;
            while (true)
            {
                var path = Path.Combine(_path, Guid.NewGuid().ToString() + ".eml");

                try
                {
                    using (var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.Read))
                    {
                        await message.WriteToAsync(format, stream, cancellationToken).ConfigureAwait(false);

                        if (progress != null)
                        {
                            var numWritten = stream.Length;
                            progress.Report(numWritten, numWritten);
                        }

                        return;
                    }
                }
                catch (IOException) when (--n > 0) { }
            }
        }

        public override void Send(FormatOptions options, MimeMessage message, CancellationToken cancellationToken = default, ITransferProgress progress = null)
        {
            SendAsync(options, message, cancellationToken, progress).GetAwaiter().GetResult();
        }

        public override Task SendAsync(FormatOptions options, MimeMessage message, CancellationToken cancellationToken = default, ITransferProgress progress = null)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            if (message == null)
                throw new ArgumentNullException(nameof(message));

            var recipients = GetMessageRecipients(message);
            var sender = GetMessageSender(message);

            if (sender == null)
                throw new InvalidOperationException("No sender has been specified.");

            if (recipients.Count == 0)
                throw new InvalidOperationException("No recipients have been specified.");

            return WriteAsync(options, message, cancellationToken, progress);
        }

        public override void Send(FormatOptions options, MimeMessage message, MailboxAddress sender, IEnumerable<MailboxAddress> recipients, CancellationToken cancellationToken = default, ITransferProgress progress = null)
        {
            SendAsync(options, message, sender, recipients, cancellationToken, progress).GetAwaiter().GetResult();
        }

        public override Task SendAsync(FormatOptions options, MimeMessage message, MailboxAddress sender, IEnumerable<MailboxAddress> recipients, CancellationToken cancellationToken = default, ITransferProgress progress = null)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            if (message == null)
                throw new ArgumentNullException(nameof(message));

            if (sender == null)
                throw new ArgumentNullException(nameof(sender));

            if (recipients == null)
                throw new ArgumentNullException(nameof(recipients));

            var unique = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var rcpts = new List<MailboxAddress>();

            AddUnique(rcpts, unique, recipients);

            if (rcpts.Count == 0)
                throw new InvalidOperationException("No recipients have been specified.");

            // cloning message
            using (var ms = new MemoryStream())
            {
                message.WriteTo(ms, cancellationToken);
                ms.Position = 0;
                message = MimeMessage.Load(ms, cancellationToken);
            }

            message.From.Clear();
            message.ResentFrom.Clear();
            message.Sender = null;
            message.ResentSender = null;

            message.From.Add(sender);

            message.To.Clear();
            message.ResentTo.Clear();
            message.Cc.Clear();
            message.ResentCc.Clear();
            message.Bcc.Clear();
            message.ResentBcc.Clear();

            message.To.AddRange(rcpts);

            return WriteAsync(options, message, cancellationToken, progress);
        }

        public override void Connect(string host, int port = 0, SecureSocketOptions options = SecureSocketOptions.Auto, CancellationToken cancellationToken = default) { }

        public override Task ConnectAsync(string host, int port = 0, SecureSocketOptions options = SecureSocketOptions.Auto, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public override void Connect(Socket socket, string host, int port = 0, SecureSocketOptions options = SecureSocketOptions.Auto, CancellationToken cancellationToken = default) { }

        public override Task ConnectAsync(Socket socket, string host, int port = 0, SecureSocketOptions options = SecureSocketOptions.Auto, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public override void Connect(Stream stream, string host, int port = 0, SecureSocketOptions options = SecureSocketOptions.Auto, CancellationToken cancellationToken = default) { }

        public override Task ConnectAsync(Stream stream, string host, int port = 0, SecureSocketOptions options = SecureSocketOptions.Auto, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public override void Authenticate(Encoding encoding, ICredentials credentials, CancellationToken cancellationToken = default) { }

        public override Task AuthenticateAsync(Encoding encoding, ICredentials credentials, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public override void Authenticate(SaslMechanism mechanism, CancellationToken cancellationToken = default) { }

        public override Task AuthenticateAsync(SaslMechanism mechanism, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public override void Disconnect(bool quit, CancellationToken cancellationToken = default) { }

        public override Task DisconnectAsync(bool quit, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public override void NoOp(CancellationToken cancellationToken = default) { }

        public override Task NoOpAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
