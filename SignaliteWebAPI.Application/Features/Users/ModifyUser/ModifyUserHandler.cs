using MediatR;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;

namespace SignaliteWebAPI.Application.Features.Users.ModifyUser;

public class ModifyUserHandler(IUserRepository userRepository) : IRequestHandler<ModifyUserCommand>
{
    public async Task Handle(ModifyUserCommand request, CancellationToken cancellationToken)
    {
        await userRepository.ModifyUser(request.UserId, 
            request.Username, request.Email,
            request.Name, request.Surname);
    }
}