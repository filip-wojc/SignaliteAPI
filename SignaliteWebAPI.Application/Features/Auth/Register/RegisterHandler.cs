using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;

namespace SignaliteWebAPI.Application.Features.Auth.Register;

public class RegisterHandler(IUserRepository repository, IMapper mapper, IPasswordHasher<Domain.Models.User> hasher) : IRequestHandler<RegisterCommand>
{
    public async Task Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var user = mapper.Map<Domain.Models.User>(request.RegisterDto);
        user.HashedPassword = hasher.HashPassword(user, request.RegisterDto.Password);
        await repository.AddUser(user);
    }
}