namespace Suteki.TardisBank.Tasks;

using Domain;
using NHibernate.Linq;
using SharpArch.Domain.PersistenceSupport;


public class UserService : IUserService
{
    readonly IHttpContextService _context;

    readonly ILinqRepository<Parent, int> _parentRepository;

    readonly ILinqRepository<User, int> _userRepository;

    public UserService(IHttpContextService context, ILinqRepository<Parent, int> parentRepository, ILinqRepository<User, int> userRepository)
    {
        _context = context;
        _parentRepository = parentRepository;
        _userRepository = userRepository;
    }

    public Task<User?> GetCurrentUser(CancellationToken cancellationToken)
    {
        if (!_context.UserIsAuthenticated) return Task.FromResult<User?>(null);

        return GetUserByUserName(_context.UserName, cancellationToken);
    }

    public Task<User?> GetUser(int userId, CancellationToken cancellationToken)
        => _userRepository.FindOneAsync(userId, cancellationToken);

    public Task<User?> GetUserByUserName(string userName, CancellationToken cancellationToken)
    {
        if (userName == null)
        {
            throw new ArgumentNullException(nameof(userName));
        }

        return _userRepository.FindAll().Where(u => u.UserName == userName).FirstOrDefaultAsync(cancellationToken)!;
    }

    public async Task<User?> GetUserByActivationKey(string activationKey, CancellationToken cancellationToken)
    {
        if (activationKey == null)
        {
            throw new ArgumentNullException(nameof(activationKey));
        }

        var res = await _parentRepository.FindAll().Where(x => x.ActivationKey == activationKey).SingleOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
        return res;
    }

    public Task SaveUser(User user, CancellationToken cancellationToken)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));

        return _userRepository.SaveAsync(user, cancellationToken);
    }

    public bool AreNullOrNotRelated(Parent? parent, Child? child)
    {
        if (parent == null || child == null) return true;

        if (!parent.HasChild(child))
        {
            throw new TardisBankException("'{0}' is not a child of '{1}'", child.UserName, parent.UserName);
        }

        return false;
    }

    public async Task<bool> IsNotChildOfCurrentUser(Child? child, CancellationToken cancellationToken)
    {
        var parent = (await GetCurrentUser(cancellationToken).ConfigureAwait(false)) as Parent;
        return (child == null) || (parent == null) || (!parent.HasChild(child));
    }

    public Task DeleteUser(int userId, CancellationToken cancellationToken)
        => _userRepository.DeleteAsync(userId, cancellationToken);
}
