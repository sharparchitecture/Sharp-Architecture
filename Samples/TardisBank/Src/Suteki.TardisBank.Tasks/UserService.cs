namespace Suteki.TardisBank.Tasks
{
    using System;
    using System.Linq;

    using SharpArch.Domain.PersistenceSupport;

    using Domain;

    public interface IUserService
    {
        User CurrentUser { get; }
        User GetUser(int userId);
        User GetUserByUserName(string userName);
        User GetUserByActivationKey(string activationKey);
        void SaveUser(User user);
        void DeleteUser(int userId);
        
        bool AreNullOrNotRelated(Parent parent, Child child);
        bool IsNotChildOfCurrentUser(Child child);
    }

    public class UserService : IUserService
    {
        readonly IHttpContextService _context;

        private readonly ILinqRepository<Parent> _parentRepository;

        readonly ILinqRepository<User> _userRepository;

        public UserService(IHttpContextService context, ILinqRepository<Parent> parentRepository, ILinqRepository<User> userRepository)
        {
            _context = context;
            _parentRepository = parentRepository;
            _userRepository = userRepository;
        }

        public User CurrentUser
        {
            get 
            {
                if (!_context.UserIsAuthenticated) return null;

                return GetUserByUserName(_context.UserName);
            }
        }

        public User GetUser(int userId)
        {
            return _userRepository.FindOne(userId);
        }

        public User GetUserByUserName(string userName)
        {
            if (userName == null)
            {
                throw new ArgumentNullException(nameof(userName));
            }

            return _userRepository.FindAll().FirstOrDefault(u => u.UserName == userName);
        }

        public User GetUserByActivationKey(string activationKey)
        {
            if (activationKey == null)
            {
                throw new ArgumentNullException(nameof(activationKey));
            }

            return _parentRepository.FindAll().SingleOrDefault(x => x.ActivationKey == activationKey);            
        }

        public void SaveUser(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            
            _userRepository.Save(user);
        }

        public bool AreNullOrNotRelated(Parent parent, Child child)
        {
            if (parent == null || child == null) return true;

            if (!parent.HasChild(child))
            {
                throw new TardisBankException("'{0}' is not a child of '{1}'", child.UserName, parent.UserName);
            }

            return false;
        }

        public bool IsNotChildOfCurrentUser(Child child)
        {
            var parent = CurrentUser as Parent;
            return (child == null) || (parent == null) || (!parent.HasChild(child));
        }

        public void DeleteUser(int userId)
        {
            _userRepository.Delete(userId);
        }
    }
}