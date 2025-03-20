using FluentValidation;
using SignaliteWebAPI.Domain.Interfaces.Repositories;

namespace SignaliteWebAPI.Application.Features.Users.AcceptFriendRequest;

public class AcceptFriendRequestValidator : AbstractValidator<AcceptFriendRequestCommand>
{
    private readonly IFriendsRepository _friendsRepository;
    public AcceptFriendRequestValidator(IFriendsRepository friendsRepository)
    {
        _friendsRepository = friendsRepository;
        
        RuleFor(c => c).MustAsync(UserHasFriendRequest)
            .WithMessage("User does not have a friend request with given id");
    }

    private async Task<bool> UserHasFriendRequest(AcceptFriendRequestCommand command, CancellationToken cancellationToken)
    {
        var friendRequests = await _friendsRepository.GetFriendRequests(command.UserId);
        if (friendRequests.Where(fr => fr.Id == command.FriendRequestId).Any())
        {
            return true;
        }
        return false;
    }
}