namespace Suteki.TardisBank.Tasks
{
    using System;
    using System.Linq;

    using Domain;

    using NHibernate;
    using NHibernate.Linq;

    public interface ISchedulerService
    {
        void ExecuteUpdates(DateTime now);
    }

    public class SchedulerService : ISchedulerService
    {
        private ISession _session;

        public SchedulerService(ISession session)
        {
            _session = session;
        }

        /// <summary>
        /// Gets all outstanding scheduled updates and performs the update.
        /// </summary>
        public void ExecuteUpdates(DateTime now)
        {
            var today = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59);

            var results = _session.Query<Child>().
                Where(c => c.Account.PaymentSchedules.Any(p => p.NextRun < today)).Fetch(c => c.Account);

            foreach (var child in results.ToList())
            {
                child.Account.TriggerScheduledPayments(now);
            }
        }
    }
}