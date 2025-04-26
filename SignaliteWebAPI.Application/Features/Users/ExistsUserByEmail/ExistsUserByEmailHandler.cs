using MediatR;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;

namespace SignaliteWebAPI.Application.Features.Users.ExistsByEmail;

public class ExistsUserByEmailHandler(IUserRepository userRepository) : IRequestHandler<ExistsUserByEmailCommand, bool>
{
    public async Task<bool> Handle(ExistsUserByEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetUserByEmail(request.Email);
        if (user == null)
            return false;
        
        return true;
    }
}