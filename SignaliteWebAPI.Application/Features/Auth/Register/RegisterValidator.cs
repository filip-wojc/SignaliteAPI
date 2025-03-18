using FluentValidation;

namespace SignaliteWebAPI.Application.Features.Auth.Register;

public class RegisterValidator : AbstractValidator<RegisterCommand>
{
    public RegisterValidator()
    {
        const int minLength = 4;
        const int maxLength = 16;
        
        RuleFor(u => u.RegisterDto.Username)
            .NotNull().WithMessage("Username is required.")
            .NotEmpty().WithMessage("Username is empty")
            .Length(minLength, maxLength).WithMessage($"Username must be between {minLength} and {maxLength} characters.");
        RuleFor(u => u.RegisterDto.Email).EmailAddress().WithMessage("Invalid email address.");

        // TODO: Password, Name, Surname
    }
}