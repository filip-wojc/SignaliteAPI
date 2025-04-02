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
            .MustAsync(IsUsernameUnique).WithMessage("Username is already taken");
        
        RuleFor(u => u.ModifyUserDTO.Email)
            .NotNull().WithMessage("Email is required.")
            .NotEmpty().WithMessage("Email is empty.")
            .EmailAddress().WithMessage("Invalid email address.")
            .MustAsync(IsEmailUnique).WithMessage("Email is already taken");
    }
    
    private async Task<bool> IsUsernameUnique(string username, CancellationToken cancellationToken)
    {
        return await _userRepository.GetUserByUsername(username) == null;
    }
    private async Task<bool> IsEmailUnique(string email, CancellationToken cancellationToken)
    { 
        return await _userRepository.GetUserByEmail(email) == null;
    }
}