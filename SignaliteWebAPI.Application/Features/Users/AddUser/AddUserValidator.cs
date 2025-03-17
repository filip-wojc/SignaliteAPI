using FluentValidation;
using SignaliteWebAPI.Application.Features.User.AddUser;

namespace SignaliteWebAPI.Application.Features.Users.AddUser;

public class AddUserValidator : AbstractValidator<AddUserCommand>
{
    public AddUserValidator()
    {
        const int minLength = 4;
        const int maxLength = 16;
        
        RuleFor(u => u.RegisterUserDto.Username)
            .NotNull().WithMessage("Username is required.")
            .NotEmpty().WithMessage("Username is empty")
            .Length(minLength, maxLength).WithMessage($"Username must be between {minLength} and {maxLength} characters.");
        RuleFor(u => u.RegisterUserDto.Email).EmailAddress().WithMessage("Invalid email address.");

        // TODO: Password, Name, Surname
    }
}