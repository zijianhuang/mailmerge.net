using MailKit;
using Microsoft.Extensions.Logging;
using MimeKit;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Fonlow.Mail
{

    /// <summary>
    /// Queue mail for sending later. If mail system has problems, only 100 messages will be stored.
    /// This class is intend to be use in scenarios where not being able to send mail is not a critical problem. And fire and forget is more important.
    /// </summary>
    public sealed class MailQueue : IDisposable
    {
        readonly ConcurrentQueue<MimeMessage> pendingQueue;
        public MailQueue(MailSender sender, ILogger<EmailTrace> logger) :this(sender, logger as ILogger)
        {
        }

        public MailQueue(MailSender sender, ILogger logger)
        {
            this.logger = logger;
            this.mailSender = sender;
            pendingQueue = new ConcurrentQueue<MimeMessage>();
            timer = new Timer(TimerCallback, null, 1000, Timeout.Infinite);
        }

        readonly MailSender mailSender;
        readonly Timer timer;
        readonly Microsoft.Extensions.Logging.ILogger logger;
        const int maxQueueSize = 100;

        public event EventHandler DownToZero;

        public event EventHandler MaxQueueSizeReached;

        public event EventHandler<string> MailSendingErrorOccured;

        /// <summary>
        /// The client codes must make sure mimeMessage is not disposed, while the queue is responsible to disposing after sending.
        /// </summary>
        /// <param name="mimeMessage">Disposed after sending.</param>
        public void Pend(MimeMessage mimeMessage)
        {
            if (pendingQueue.Count >= maxQueueSize)
            {
                System.Diagnostics.Trace.TraceWarning($"There could be serious problem in the mail system since the MailQueue has {maxQueueSize} messages pending.");
                MaxQueueSizeReached?.Invoke(this, EventArgs.Empty);
                return;
            }

            pendingQueue.Enqueue(mimeMessage);
        }


        async Task SendAsync()
        {
            while (pendingQueue.TryDequeue(out MimeMessage mimeMessage))
            {
                try
                {
                    await mailSender.SendAsync(mimeMessage);
                }
                catch (CommandException ex)
                {
                    if (ex.Message.Contains("Throttling failure")) // Throttling failure: Daily message quota exceeded.
                    {
                        throw;
                    }

                    var msg = $"Failed to send Email \"{mimeMessage.Subject}\" to {mimeMessage.To.ToString()} becase {ex.Message}";
                    logger.LogWarning(msg);
                    MailSendingErrorOccured?.Invoke(this, msg);
                }
                catch (MailKit.Security.SslHandshakeException ex)
                {
                    logger.LogError(ex.Message);
                }
                finally
                {
                    logger.LogInformation($"Left in Queue: {pendingQueue.Count}");
                    mimeMessage.Dispose();
                    if (pendingQueue.Count == 0)
                    {
                        DownToZero?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
        }


        async void TimerCallback(Object stateInfo)
        {
            await SendAsync();
            timer.Change(1000, Timeout.Infinite);// 1 second is a good number, with optimal performance, 0.5 second does not make performance noticablly better.
        }

        public string From => mailSender.From;

        #region IDisposable pattern
        bool disposed = false;

        void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    timer.Dispose();
                }

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

    }
}
