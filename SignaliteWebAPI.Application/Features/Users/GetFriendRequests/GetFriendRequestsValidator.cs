using FluentValidation;
using SignaliteWebAPI.Domain.Interfaces.Repositories;

namespace SignaliteWebAPI.Application.Features.Users.GetFriendRequests;

public class GetFriendRequestsValidator : AbstractValidator<GetFriendRequestsQuery>
{
    private readonly IUserRepository _userRepository;
    public GetFriendRequestsValidator(IUserRepository userRepository)
    {
        _userRepository = userRepository;
        RuleFor(fr => fr.UserId)
            .NotNull()
            .NotEmpty()
            .MustAsync(UserExists).WithMessage("User not found");
    }
    
    private async Task<bool> UserExists(int userId, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserById(userId);
        return user != null;
    }
}