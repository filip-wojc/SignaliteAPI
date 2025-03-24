using FluentValidation;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;

namespace SignaliteWebAPI.Application.Features.Friends.GetUserFriends;

public class GetUserFriendsValidator : AbstractValidator<GetUserFriendsQuery>
{
    private readonly IUserRepository _userRepository;

    public GetUserFriendsValidator(IUserRepository userRepository)
    {
        _userRepository = userRepository;

        RuleFor(q => q.UserId).MustAsync(UserExists).WithMessage("User not found");
    }

    private async Task<bool> UserExists(int userId, CancellationToken cancellationToken)
    {
        return await _userRepository.GetUserById(userId) != null;
    }
}