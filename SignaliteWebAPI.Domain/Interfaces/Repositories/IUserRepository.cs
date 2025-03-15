using SignaliteWebAPI.Domain.Models;

namespace SignaliteWebAPI.Domain.Interfaces.Repositories;

public interface IUserRepository
{
    Task AddUser(User user);
}