namespace Suteki.TardisBank.Tasks;

public interface IHttpContextService
{
    string UserName { get; }
    bool UserIsAuthenticated { get; }

    bool UserIsInRole(string role);
}
