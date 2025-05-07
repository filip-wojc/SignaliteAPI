using FluentValidation;
using SignaliteWebAPI.Infrastructure.Interfaces.Repositories;

namespace SignaliteWebAPI.Application.Features.Friends.SendFriendRequest;

public class SendFriendRequestValidator : AbstractValidator<SendFriendRequestCommand>
{
    private readonly IFriendsRepository _friendsRepository;
    private readonly IUserRepository _userRepository;
    public SendFriendRequestValidator(IFriendsRepository friendsRepositoryrepository, IUserRepository userRepository)
    {
        _friendsRepository = friendsRepositoryrepository;
        _userRepository = userRepository;
        
        RuleFor(c => c.RecipientUsername)
            .NotEmpty()
            .NotNull()
            .MustAsync(UserExistsByUsername).WithMessage("Recipient with given id does not exist");
        RuleFor(c => c.SenderId)
            .NotEmpty()
            .NotNull()
            .MustAsync(UserExistsById).WithMessage("Sender with given id does not exist");

        RuleFor(c => c).MustAsync(FriendRequestNotExist)
            .WithMessage("Friend request between these users already exist");
        RuleFor(c => c).Must(IsSenderAndRecipientNotSame)
            .WithMessage("Sender and recipient cant be same");
    }

    private async Task<bool> UserExistsByUsername(string username, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByUsernameNullable(username);
        return user != null;
    }
    
    private async Task<bool> UserExistsById(int userId, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByIdNullable(userId);
        return user != null;
    }
    
    private async Task<bool> FriendRequestNotExist(SendFriendRequestCommand command, CancellationToken cancellationToken)
    {
        var recipientUser = await _userRepository.GetUserByUsername(command.RecipientUsername);
        bool exists = await _friendsRepository.FriendRequestExists(
            command.SenderId, 
            recipientUser.Id
        );
        
        return !exists;
    }

    private bool IsSenderAndRecipientNotSame(SendFriendRequestCommand command)
    {
        return command.SenderUsername != command.RecipientUsername;
    }
}