using AutoMapper;
using MediatR;
using SignaliteWebAPI.Application.Features.Users.ModifyUser;
using SignaliteWebAPI.Domain.DTOs.Users;
using SignaliteWebAPI.Infrastructure.Exceptions;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;

namespace SignaliteWebAPI.Application.Features.Users.GetUserInfo;

public class GetUserInfoHandler(IUserRepository userRepository, IMapper mapper) : IRequestHandler<GetUserInfoCommand, IUserDTO>
{
    public async Task<IUserDTO> Handle(GetUserInfoCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetUserById(request.UserId);
        if (!request.IsOwner)
        {
            var userDto = mapper.Map<UserDTO>(user);
            return userDto;
        }
        var ownUserDto = mapper.Map<OwnUserDTO>(user);
        return ownUserDto;
    }
}