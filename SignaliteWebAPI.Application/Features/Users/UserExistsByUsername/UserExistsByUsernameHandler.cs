using MediatR;
using SignaliteWebAPI.Application.Features.Auth.ExistsUserByUsername;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;

namespace SignaliteWebAPI.Application.Features.Users.UserExistsByUsername;

public class UserExistsByUsernameHandler(IUserRepository userRepository) : IRequestHandler<UserExistsByUsernameCommand, bool>
{
    public async Task<bool> Handle(UserExistsByUsernameCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetUserByUsernameNullable(request.Username);
        if (user == null)
            return false;
        
        return true;
    }
}