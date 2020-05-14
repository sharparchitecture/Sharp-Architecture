namespace MultiDatabase.Sample.Controllers
{
    using System;
    using System.Threading.Tasks;
    using DomainLayer.Entities;
    using JetBrains.Annotations;
    using Microsoft.AspNetCore.Mvc;
    using Models;
    using SharpArch.Domain.PersistenceSupport;
    using SharpArch.Web.AspNetCore.Transaction;


    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        readonly IRepository<User, long> _userRepository;
        readonly IRepository<LogMessage, int> _logRepository;

        /// <inheritdoc />
        public UserController()
        {// [NotNull] IRepository<User, long> userRepository, [NotNull] IRepository<LogMessage, int> logRepository
            //_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            //_logRepository = logRepository ?? throw new ArgumentNullException(nameof(logRepository));
        }


        [HttpPost]
        [Transaction]
        public async Task<ActionResult> Add(NewUser newUser)
        {
            var user = new User
            {
                Login = newUser.Login
            };
            await _userRepository.SaveAsync(user).ConfigureAwait(false);
            await _logRepository.SaveAsync(new LogMessage($"Created user '{newUser.Login}'")).ConfigureAwait(false);

            return NoContent();
        }
    }
}
