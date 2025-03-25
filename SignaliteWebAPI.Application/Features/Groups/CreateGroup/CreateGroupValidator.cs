using FluentValidation;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;

namespace SignaliteWebAPI.Application.Features.Groups.CreateGroup;

public class CreateGroupValidator : AbstractValidator<CreateGroupCommand>
{
    private readonly IUserRepository _userRepository;
    public CreateGroupValidator(IUserRepository userRepository)
    {
        _userRepository = userRepository;
        
        RuleFor(c => c.Name).NotEmpty().NotNull().MaximumLength(32);
        RuleFor(c => c.OwnerId).MustAsync(UserExists).WithMessage("User not found");
    }

    private async Task<bool> UserExists(int userId, CancellationToken cancellationToken)
    {
        return await _userRepository.GetUserById(userId) != null;
    }
}