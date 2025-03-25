using FluentValidation;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;

namespace SignaliteWebAPI.Application.Features.Groups.AddUserToGroup;

public class AddUserToGroupValidator : AbstractValidator<AddUserToGroupCommand>
{
    private readonly IUserRepository _userRepository;

    public AddUserToGroupValidator(IUserRepository userRepository)
    {
        _userRepository = userRepository;

        RuleFor(u => u.UserId).MustAsync(UserExists).WithMessage("User does not exist");
    }

    private async Task<bool> UserExists(int userId, CancellationToken cancellationToken)
    {
        return await _userRepository.GetUserById(userId) != null;
    }
}