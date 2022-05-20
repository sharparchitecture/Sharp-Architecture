namespace SharpArch.NHibernate;

using Domain;
using global::NHibernate;


/// <summary>
///     Converter between <see cref="Enums.LockMode" /> and <see cref="LockMode" />.
/// </summary>
[PublicAPI]
public static class LockModeConvertExtensions
{
    static readonly IReadOnlyDictionary<LockMode, Enums.LockMode> _nHibernateToSharpArchMap = new Dictionary<LockMode, Enums.LockMode>(5)
    {
        [LockMode.None] = Enums.LockMode.None,
        [LockMode.Read] = Enums.LockMode.Read,
        [LockMode.Upgrade] = Enums.LockMode.Upgrade,
        [LockMode.UpgradeNoWait] = Enums.LockMode.UpgradeNoWait,
        [LockMode.Write] = Enums.LockMode.Write
    };

    /// <summary>
    ///     Translates a domain layer lock mode into an NHibernate lock mode.
    ///     This is provided to facilitate developing the domain layer without a direct dependency on the
    ///     NHibernate assembly.
    /// </summary>
    public static LockMode ToNHibernate(this Enums.LockMode lockMode)
        => lockMode switch
        {
            Enums.LockMode.None => LockMode.None,
            Enums.LockMode.Read => LockMode.Read,
            Enums.LockMode.Upgrade => LockMode.Upgrade,
            Enums.LockMode.UpgradeNoWait => LockMode.UpgradeNoWait,
            Enums.LockMode.Write => LockMode.Write,
            _ => throw new ArgumentOutOfRangeException(nameof(lockMode), lockMode,
                $"The provided lock mode , '{lockMode}', could not be translated into an NHibernate.LockMode. "
                + "This is probably because NHibernate was updated and now has different lock modes "
                + "which are out of synch with the lock modes maintained in the domain layer.")
        };

    /// <summary>
    ///     Translates an NHibernate lock mode into a domain layer lock mode.
    ///     This is provided to facilitate developing the domain layer without a direct dependency on the
    ///     NHibernate assembly.
    /// </summary>
    public static Enums.LockMode ToNHibernate(this LockMode lockMode)
    {
        if (_nHibernateToSharpArchMap.TryGetValue(lockMode, out var convertedMode)) return convertedMode;
        throw new ArgumentOutOfRangeException(nameof(lockMode), lockMode,
            $"The provided lock mode , '{lockMode}', could not be translated into an SharpArch.LockMode. "
            + "This is probably because NHibernate was updated and now has different lock modes "
            + "which are out of synch with the lock modes maintained in the domain layer.");
    }
}
