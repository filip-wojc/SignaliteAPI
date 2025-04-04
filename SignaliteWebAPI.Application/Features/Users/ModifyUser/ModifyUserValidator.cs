using FluentValidation;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;

namespace SignaliteWebAPI.Application.Features.Users.ModifyUser;

public class ModifyUserValidator : AbstractValidator<ModifyUserCommand>
{
    private readonly IUserRepository _userRepository;
    
    public ModifyUserValidator(IUserRepository userRepository)
    {
        _userRepository = userRepository;
        const int minLengthUsername = 4;
        const int maxLengthUsername = 16;
        
        RuleFor(u => u.ModifyUserDTO.Name)
            .NotNull().WithMessage("Name is required.")
            .NotEmpty().WithMessage("Name is empty.");

        RuleFor(u => u.ModifyUserDTO.Surname)
            .NotNull().WithMessage("Surname is required.")
            .NotEmpty().WithMessage("Surname is empty.");
        
        RuleFor(u => u.ModifyUserDTO.Username)
            .NotNull().WithMessage("Username is required.")
            .NotEmpty().WithMessage("Username is empty.")
            .Length(minLengthUsername, maxLengthUsername).WithMessage($"Username must be between {minLengthUsername} and {maxLengthUsername} characters.")
            .MustAsync((command, username, cancellationToken) => IsUsernameUnique(command.UserId, username, cancellationToken)).WithMessage("Username is already taken");
        
        RuleFor(u => u.ModifyUserDTO.Email)
            .NotNull().WithMessage("Email is required.")
            .NotEmpty().WithMessage("Email is empty.")
            .EmailAddress().WithMessage("Invalid email address.")
            .MustAsync((command, email, cancellationToken) => IsEmailUnique(command.UserId, email, cancellationToken)).WithMessage("Email is already taken");
    }
    
    private async Task<bool> IsUsernameUnique(int userId, string username, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByUsername(username);
        
        if (user == null)
            return true;
        
        if (user.Id == userId)
            return true;
        
        return false;
    }
    private async Task<bool> IsEmailUnique(int userId, string email, CancellationToken cancellationToken)
    { 
        var user = await _userRepository.GetUserByEmail(email);
        
        if (user == null)
            return true;
        
        if (user.Id == userId)
            return true;
        
        return false;
    }
}