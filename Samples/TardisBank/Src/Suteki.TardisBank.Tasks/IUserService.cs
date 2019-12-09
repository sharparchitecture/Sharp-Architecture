namespace Suteki.TardisBank.Tasks
{
    using System.Threading;
    using System.Threading.Tasks;
    using Domain;


    public interface IUserService
    {
        Task<User> GetCurrentUser(CancellationToken cancellationToken = default);
        Task<User> GetUser(int userId, CancellationToken cancellationToken = default);
        Task<User> GetUserByUserName(string userName, CancellationToken cancellationToken = default);
        Task<User> GetUserByActivationKey(string activationKey, CancellationToken cancellationToken = default);
        Task SaveUser(User user, CancellationToken cancellationToken = default);
        Task DeleteUser(int userId, CancellationToken cancellationToken = default);
        
        bool AreNullOrNotRelated(Parent parent, Child child);
        Task<bool> IsNotChildOfCurrentUser(Child child, CancellationToken cancellationToken = default);
    }
}
