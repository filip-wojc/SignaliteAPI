using AutoMapper;
using MediatR;
using SignaliteWebAPI.Application.Features.Users.ModifyUser;
using SignaliteWebAPI.Domain.DTOs.Users;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;

namespace SignaliteWebAPI.Application.Features.Users.GetUserInfo;

public class GetUserInfoHandler(IUserRepository userRepository, IMapper mapper) : IRequestHandler<GetUserInfoCommand, UserDTO>
{
    public async Task<UserDTO> Handle(GetUserInfoCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetUserById(request.UserId);
        if (user == null)
            throw new NullReferenceException();
        var userDto = mapper.Map<UserDTO>(user);
        
        return userDto;
    }
}