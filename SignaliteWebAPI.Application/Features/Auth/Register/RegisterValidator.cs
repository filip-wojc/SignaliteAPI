using FluentValidation;
using SignaliteWebAPI.Domain.Interfaces.Repositories;

namespace SignaliteWebAPI.Application.Features.Auth.Register;

public class RegisterValidator : AbstractValidator<RegisterCommand>
{
    private readonly IUserRepository _userRepository;
    
    public RegisterValidator(IUserRepository userRepository)
    {
        _userRepository = userRepository;
        const int minLengthPassword = 8;
        const int minLengthUsername = 4;
        const int maxLengthUsername = 16;
        
        RuleFor(u => u.RegisterDto.Name)
            .NotNull().WithMessage("Name is required.")
            .NotEmpty().WithMessage("Name is empty.");

        RuleFor(u => u.RegisterDto.Surname)
            .NotNull().WithMessage("Surname is required.")
            .NotEmpty().WithMessage("Surname is empty.");

        RuleFor(u => u.RegisterDto.Password)
            .NotNull().WithMessage("Password is required.")
            .NotEmpty().WithMessage("Password is empty.")
            .MinimumLength(minLengthPassword).WithMessage("Password must be at least 8 characters long.")
            .Matches(@"[a-z]").WithMessage("Password  must contain at least one lowercase letter.")
            .Matches(@"[A-Z]").WithMessage("Password  must contain at least one uppercase letter.")
            .Matches(@"[@$!%*?&\.\^]").WithMessage("Password  must contain at least one special character.")
            .Matches(@"\d").WithMessage("Password  must contain at least one number.");;
        
        RuleFor(u => u.RegisterDto.Username)
            .NotNull().WithMessage("Username is required.")
            .NotEmpty().WithMessage("Username is empty.")
            .Length(minLengthUsername, maxLengthUsername).WithMessage($"Username must be between {minLengthUsername} and {maxLengthUsername} characters.")
            .MustAsync(IsUsernameUnique).WithMessage("Username is already taken");
        
        RuleFor(u => u.RegisterDto.Email)
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