namespace TaskTronic.Messages
{
    using Hangfire;
    using MassTransit;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Hosting;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using TaskTronic.Data.Models;

    // for background and long running tasks
    public class MessagesHostedService : IHostedService
    {
        private readonly IRecurringJobManager recurringJobManager;
        private readonly DbContext dbContext;
        private readonly IBus publisher;

        public MessagesHostedService(
            IRecurringJobManager recurringJobManager,
            DbContext dbContext,
            IBus publisher)
        {
            this.recurringJobManager = recurringJobManager;
            this.dbContext = dbContext;
            this.publisher = publisher;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this.recurringJobManager.AddOrUpdate(
                nameof(MessagesHostedService),
                () => this.ProccessPendingMessages(),
                "*/5 * * * * *"); // Cron job expression syntax -> runs in every 5 sec

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;

        // must be public and not async in order to work with recurring job manager
        // hangfire will handle all failures if something went wrong with the method below
        public void ProccessPendingMessages()
        {
            var messages = this.dbContext
                .Set<Message>()
                .Where(m => !m.Published)
                .OrderBy(m => m.Id)
                .ToList();

            foreach (var message in messages)
            {
                this.publisher.Publish(message.Data, message.Type);

                message.MarkedAsPublished();

                this.dbContext.SaveChanges();
            }
        }
    }
}
