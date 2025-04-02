using AutoMapper;
using MediatR;
using SignaliteWebAPI.Infrastructure.Exceptions;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;

namespace SignaliteWebAPI.Application.Features.Users.ModifyUser;

public class ModifyUserHandler(IUserRepository userRepository, IMapper mapper) : IRequestHandler<ModifyUserCommand>
{
    public async Task Handle(ModifyUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetUserById(request.UserId);
        mapper.Map(request.ModifyUserDTO, user);
        await userRepository.ModifyUser(user);
    }
}