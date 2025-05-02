using MediatR;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;

namespace SignaliteWebAPI.Application.Features.Auth.ExistsUserByUsername;

public class ExistsUserByUsernameHandler(IUserRepository userRepository) : IRequestHandler<ExistsUserByUsernameCommand, bool>
{
    public async Task<bool> Handle(ExistsUserByUsernameCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetUserByUsername(request.Username);
        if (user == null)
            return false;
        
        return true;
    }
}