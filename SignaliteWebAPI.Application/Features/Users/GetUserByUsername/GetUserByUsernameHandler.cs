using AutoMapper;
using MediatR;
using SignaliteWebAPI.Domain.DTOs.Users;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;

namespace SignaliteWebAPI.Application.Features.Users.GetUserByUsername;

public class GetUserByUsernameHandler(IUserRepository userRepository, IMapper mapper) : IRequestHandler<GetUserByUsernameQuery, UserDTO>
{
    public async Task<UserDTO> Handle(GetUserByUsernameQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetUserByUsername(request.Username);
        var userDto = mapper.Map<UserDTO>(user);
        
        return userDto;
    }
}