using MediatR;
using Microsoft.AspNetCore.Identity;
using SignaliteWebAPI.Application.Exceptions;
using SignaliteWebAPI.Application.Features.Users.ModifyUser;
using SignaliteWebAPI.Domain.DTOs.Auth;
using SignaliteWebAPI.Infrastructure.Exceptions;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;

namespace SignaliteWebAPI.Application.Features.Users.ChangePassword;

public class ChangePasswordHandler(IUserRepository userRepository, IPasswordHasher<Domain.Models.User> passwordHasher) : IRequestHandler<ChangePasswordCommand>
{
    public async Task Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetUserById(request.UserId);
        
        var passwordVerificationResult = passwordHasher.VerifyHashedPassword(
            user, user.HashedPassword, request.ChangePasswordDto.OldPassword);
            
        if (passwordVerificationResult == PasswordVerificationResult.Failed)
            throw new BadRequestException("Invalid old password");
        
        var hashedPassword = passwordHasher.HashPassword(user, request.ChangePasswordDto.NewPassword);
        user.HashedPassword = hashedPassword;
        
        await userRepository.ChangePassword(user);
    }
}