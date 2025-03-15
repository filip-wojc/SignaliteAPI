using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using SignaliteWebAPI.Application.Features.User.AddUser;
using SignaliteWebAPI.Domain.Interfaces.Repositories;


namespace SignaliteWebAPI.Application.Features.Users.AddUser;

public class AddUserHandler(IUserRepository repository, IMapper mapper, IPasswordHasher<Domain.Models.User> hasher) : IRequestHandler<AddUserCommand>
{
    public async Task Handle(AddUserCommand request, CancellationToken cancellationToken)
    {
        var user = mapper.Map<Domain.Models.User>(request.RegisterUserDto);
        user.HashedPassword = hasher.HashPassword(user, request.RegisterUserDto.Password);
        await repository.AddUser(user);
    }
}