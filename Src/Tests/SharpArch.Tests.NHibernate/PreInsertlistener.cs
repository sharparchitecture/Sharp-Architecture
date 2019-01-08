// ReSharper disable PublicMembersMustHaveComments
// ReSharper disable InternalMembersMustHaveComments

namespace Tests.SharpArch.NHibernate
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using global::NHibernate.Event;


    [Serializable]
    internal class PreInsertListener : IPreInsertEventListener
    {
        public Task<bool> OnPreInsertAsync(PreInsertEvent @event, CancellationToken cancellationToken)
        {
            return PreUpdateListener.True;
        }

        public bool OnPreInsert(PreInsertEvent @event)
        {
            return true;
        }
    }


    [Serializable]
    public class PreUpdateListener : IPreUpdateEventListener
    {
        internal static readonly Task<bool> True = Task.FromResult(true);

        public Task<bool> OnPreUpdateAsync(PreUpdateEvent @event, CancellationToken cancellationToken)
        {
            return True;
        }

        public bool OnPreUpdate(PreUpdateEvent @event)
        {
            return true;
        }
    }
}
