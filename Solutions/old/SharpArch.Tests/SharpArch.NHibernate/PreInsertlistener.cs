// ReSharper disable PublicMembersMustHaveComments
// ReSharper disable InternalMembersMustHaveComments

namespace Tests.SharpArch.NHibernate
{
    using System;
    using global::NHibernate.Event;

    [Serializable]
    internal class PreInsertListener : IPreInsertEventListener
    {
        public bool OnPreInsert(PreInsertEvent @event)
        {
            return true;
        }
    }

    [Serializable]
    public class PreUpdateListener : IPreUpdateEventListener
    {
        public bool OnPreUpdate(PreUpdateEvent @event)
        {
            return true;
        }
    }
}
