namespace Tests.SharpArch.NHibernate;

using global::NHibernate.Event;


[Serializable]
class PreInsertListener : IPreInsertEventListener
{
    public Task<bool> OnPreInsertAsync(PreInsertEvent @event, CancellationToken cancellationToken)
        => PreUpdateListener.True;

    public bool OnPreInsert(PreInsertEvent @event)
        => true;
}


[Serializable]
public class PreUpdateListener : IPreUpdateEventListener
{
    internal static readonly Task<bool> True = Task.FromResult(true);

    public Task<bool> OnPreUpdateAsync(PreUpdateEvent @event, CancellationToken cancellationToken)
        => True;

    public bool OnPreUpdate(PreUpdateEvent @event)
        => true;
}
